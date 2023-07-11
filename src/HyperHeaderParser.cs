using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FluentResults;

namespace OoLunar.HyperSharp
{
    public sealed class HyperHeaderParser
    {
        public static async Task<Result<HyperContext>> TryParseHeadersAsync(int maxHeaderSize, NetworkStream networkStream)
        {
            if (maxHeaderSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHeaderSize), "Max header size must be greater than zero.");
            }
            ArgumentNullException.ThrowIfNull(networkStream);

            HyperHeaderCollection headers = new();
            PipeReader pipeReader = PipeReader.Create(networkStream);
            ReadResult readResult = await pipeReader.ReadAsync();
            if (readResult.Buffer.Length == 0)
            {
                return Result.Fail("There was no data to be read.");
            }

            SequencePosition sequencePosition = default;
            Result<(HttpMethod Method, Uri Route, Version Version)> requestLineResult = TryParseRequestLine(readResult, maxHeaderSize, ref sequencePosition);
            if (requestLineResult.IsFailed)
            {
                return Result.Fail(requestLineResult.Errors);
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
                // End of headers
                else if (headerResult.Value == default)
                {
                    break;
                }

                pipeReader.AdvanceTo(sequencePosition);
                headers.AddHeaderValue(headerResult.Value.Name, headerResult.Value.Value);
                readResult = await pipeReader.ReadAsync();
            }

            // The method reads and parses headers in a loop until it encounters an error or a null header.
            // In the case of malicious input, this could potentially lead to an infinite loop if the end of the headers is never found.
            // TODO: Check if the last position is the end of the buffer, and if not, return an error
            return Result.Ok(new HyperContext(
                requestLineResult.Value.Method,
                requestLineResult.Value.Route,
                requestLineResult.Value.Version,
                headers,
                pipeReader,
                PipeWriter.Create(networkStream)
            ));
        }

        private static Result<(HttpMethod Method, Uri Route, Version Version)> TryParseRequestLine(ReadResult result, int maxHeaderSize, ref SequencePosition sequencePosition)
        {
            SequenceReader<byte> sequenceReader = new(result.Buffer);
            if (!sequenceReader.TryReadTo(out ReadOnlySpan<byte> requestLine, "\r\n"u8, advancePastDelimiter: true))
            {
                return Result.Fail("Invalid data in request line.");
            }
            else if (requestLine.Length > maxHeaderSize)
            {
                return Result.Fail("Request line length exceeds max header size.");
            }

            // Split the request line into method, path, and version
            int firstSpaceIndex = requestLine.IndexOf((byte)' ');
            int lastSpaceIndex = requestLine.LastIndexOf((byte)' ');
            HttpMethod httpMethod;
            try
            {
                // Unfortunately, HttpMethod doesn't have a TryParse method, so we have to use the constructor
                httpMethod = new(Encoding.ASCII.GetString(requestLine[..firstSpaceIndex]));
            }
            catch (Exception)
            {
                return Result.Fail("Invalid HTTP method passed.");
            }

            if (firstSpaceIndex == -1
                || lastSpaceIndex == -1
                || firstSpaceIndex == lastSpaceIndex
                || !Uri.TryCreate(Encoding.ASCII.GetString(requestLine[(firstSpaceIndex + 1)..lastSpaceIndex]), UriKind.Absolute, out Uri? httpRoute))
            {
                return Result.Fail("Invalid route specified.");
            }

            Version httpVersion = Encoding.ASCII.GetString(requestLine[(lastSpaceIndex + 1)..]).ToLowerInvariant() switch
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
                return Result.Fail("Request line length exceeds max header size.");
            }
            else if (header.Length == 0)
            {
                // We've reached the end of the headers
                return Result.Ok();
            }

            // Find the index of the separator (':') in the header line
            int separatorIndex = header.IndexOf((byte)':');
            if (separatorIndex == -1)
            {
                return Result.Fail("Invalid header data.");
            }

            // Check for invalid characters in the header name
            string name = Encoding.ASCII.GetString(header[..separatorIndex]).Trim();
            if (!IsToken(name))
            {
                return Result.Fail("Invalid header name.");
            }

            sequencePosition = sequenceReader.Position;
            return Result.Ok((
                name,
                Encoding.ASCII.GetString(header[(separatorIndex + 1)..]).Trim()
            ));
        }

        private static bool IsToken(string value)
        {
            foreach (char c in value)
            {
                // Check for non-token characters (non-printable ASCII or any of the separators)
                if (c is < (char)0x20 or >= (char)0x7F || "()<>@,;:\\\"/[]?={}".Contains(c))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
