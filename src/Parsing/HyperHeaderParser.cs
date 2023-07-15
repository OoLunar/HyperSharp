using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FluentResults;

namespace OoLunar.HyperSharp.Parsing
{
    public sealed class HyperHeaderParser
    {
        // https://www.rfc-editor.org/rfc/rfc9110#section-9.1
        // All HTTP methods are case-sensitive.
        private static readonly KeyValuePair<byte[], HttpMethod>[] HttpMethods = new KeyValuePair<byte[], HttpMethod>[]
        {
            new("GET"u8.ToArray(), HttpMethod.Get),
            new("HEAD"u8.ToArray(), HttpMethod.Head),
            new("POST"u8.ToArray(), HttpMethod.Post),
            new("PUT"u8.ToArray(), HttpMethod.Put),
            new("DELETE"u8.ToArray(), HttpMethod.Delete),
            new("CONNECT"u8.ToArray(), HttpMethod.Connect),
            new("OPTIONS"u8.ToArray(), HttpMethod.Options),
            new("TRACE"u8.ToArray(), HttpMethod.Trace),
            new("PATCH"u8.ToArray(), HttpMethod.Patch),
        };

        public static async Task<Result<HyperContext>> TryParseHeadersAsync(int maxHeaderSize, NetworkStream networkStream)
        {
            if (maxHeaderSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHeaderSize), "Max header size must be greater than zero.");
            }
            ArgumentNullException.ThrowIfNull(networkStream);

            HyperHeaderCollection headers = new();
            PipeReader pipeReader = PipeReader.Create(networkStream, new StreamPipeReaderOptions(leaveOpen: true));
            ReadResult readResult = await pipeReader.ReadAsync();
            if (readResult.Buffer.Length == 0)
            {
                return Result.Fail("There was no data to be read.");
            }

            SequencePosition sequencePosition = default;
            Result<(HttpMethod Method, Uri Route, Version Version)> startLineResult = TryParseStartLine(readResult, maxHeaderSize, ref sequencePosition);
            if (startLineResult.IsFailed)
            {
                return Result.Fail(startLineResult.Errors);
            }

            pipeReader.AdvanceTo(sequencePosition);
            readResult = await pipeReader.ReadAsync();
            while (!readResult.IsCompleted)
            {
                if (readResult.Buffer.Length > maxHeaderSize)
                {
                    return Result.Fail("Data exceeds the max header size.");
                }

                Result<(string Name, string Value)> headerResult = TryParseHeader(readResult, maxHeaderSize, ref sequencePosition);
                if (headerResult.IsFailed)
                {
                    return Result.Fail(headerResult.Errors);
                }
                pipeReader.AdvanceTo(sequencePosition);

                // End of headers
                if (headerResult.Value == default)
                {
                    break;
                }

                headers.AddHeaderValue(headerResult.Value.Name, headerResult.Value.Value);
                readResult = await pipeReader.ReadAsync();
            }

            // The method reads and parses headers in a loop until it encounters an error or a null header.
            // In the case of malicious input, this could potentially lead to an infinite loop if the end of the headers is never found.
            // TODO: Check if the last position is the end of the buffer, and if not, return an error
            return Result.Ok(new HyperContext(
                startLineResult.Value.Method,
                startLineResult.Value.Route,
                startLineResult.Value.Version,
                headers,
                pipeReader,
                PipeWriter.Create(networkStream, new StreamPipeWriterOptions(leaveOpen: true))
            ));
        }

        private static Result<(HttpMethod Method, Uri Route, Version Version)> TryParseStartLine(ReadResult result, int maxHeaderSize, ref SequencePosition sequencePosition)
        {
            SequenceReader<byte> sequenceReader = new(result.Buffer);
            if (!sequenceReader.TryReadTo(out ReadOnlySpan<byte> startLine, "\r\n"u8, advancePastDelimiter: true))
            {
                return Result.Fail("Invalid data in the start line.");
            }
            else if (startLine.Length > maxHeaderSize)
            {
                return Result.Fail("Start line length exceeds max header size.");
            }

            // Split the start line into method, path, and version
            int firstSpaceIndex = startLine.IndexOf((byte)' ');
            int lastSpaceIndex = startLine.LastIndexOf((byte)' ');
            if (firstSpaceIndex == -1
                || lastSpaceIndex == -1
                || firstSpaceIndex == lastSpaceIndex)
            {
                return Result.Fail("Invalid start line data.");
            }

            HttpMethod? httpMethod = null;
            foreach (KeyValuePair<byte[], HttpMethod> httpMethodPair in HttpMethods)
            {
                if (startLine[..firstSpaceIndex].SequenceEqual(httpMethodPair.Key))
                {
                    httpMethod = httpMethodPair.Value;
                    break;
                }
            }

            if (httpMethod is null)
            {
                return Result.Fail("Invalid HTTP method specified.");
            }

            if (!Uri.TryCreate(Encoding.ASCII.GetString(startLine[(firstSpaceIndex + 1)..lastSpaceIndex]), UriKind.Absolute, out Uri? httpRoute))
            {
                return Result.Fail("Invalid route specified.");
            }

            Version httpVersion = Encoding.ASCII.GetString(startLine[(lastSpaceIndex + 1)..]).ToLowerInvariant() switch
            {
                "http/1.0" => HttpVersion.Version10,
                "http/1.1" => HttpVersion.Version11,
                _ => HttpVersion.Unknown
            };

            if (httpVersion == HttpVersion.Unknown)
            {
                return Result.Fail("Invalid HTTP version specified.");
            }

            sequencePosition = sequenceReader.Position;
            return Result.Ok((httpMethod, httpRoute, httpVersion));
        }

        private static Result<(string Name, string Value)> TryParseHeader(ReadResult result, int maxHeaderSize, ref SequencePosition sequencePosition)
        {
            SequenceReader<byte> sequenceReader = new(result.Buffer);
            if (!sequenceReader.TryReadTo(out ReadOnlySpan<byte> header, "\r\n"u8, advancePastDelimiter: true))
            {
                return Result.Fail("Invalid header data.");
            }
            else if (header.Length > maxHeaderSize)
            {
                return Result.Fail("Header line length exceeds max header size.");
            }
            else if (header.Length == 0)
            {
                // We've reached the end of the headers
                // Skip the next two bytes (\r\n)
                sequencePosition = sequenceReader.Position;
                return Result.Ok();
            }

            // Find the index of the separator (':') in the header line
            int separatorIndex = header.IndexOf((byte)':');
            if (separatorIndex == -1)
            {
                return Result.Fail("Invalid header data.");
            }

            sequencePosition = sequenceReader.Position;
            return Result.Ok((
                name: Encoding.ASCII.GetString(header[..separatorIndex]).Trim(),
                value: Encoding.ASCII.GetString(header[(separatorIndex + 1)..]).Trim()
            ));
        }
    }
}
