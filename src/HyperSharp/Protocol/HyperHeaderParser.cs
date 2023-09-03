using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HyperSharp.Results;

namespace HyperSharp.Protocol
{
    /// <summary>
    /// Provides methods for parsing HTTP headers, including the start line.
    /// </summary>
    public static class HyperHeaderParser
    {
        /// <summary>
        /// Parses the headers of an HTTP request, returning a <see cref="HyperContext"/> if successful.
        /// </summary>
        /// <param name="maxHeaderSize">The maximum size of each header.</param>
        /// <param name="connection">The connection to read the headers from.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="Result{T}"/> containing a <see cref="HyperContext"/>.</returns>
        public static async ValueTask<Result<HyperContext>> TryParseHeadersAsync(int maxHeaderSize, HyperConnection connection, CancellationToken cancellationToken = default)
        {
            if (maxHeaderSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHeaderSize), "Max header size must be greater than zero.");
            }
            ArgumentNullException.ThrowIfNull(connection);

            ReadResult readResult = await connection.StreamReader.ReadAsync(cancellationToken);
            if (readResult.IsCanceled)
            {
                return Result.Failure<HyperContext>("The operation was cancelled.");
            }
            else if (readResult.Buffer.Length == 0)
            {
                return Result.Failure<HyperContext>("There was no data to be read.");
            }

            SequencePosition sequencePosition = default;
            Result startLineResult = TryParseStartLine(readResult, maxHeaderSize, ref sequencePosition, out HttpMethod? method, out Uri? route, out Version? version);
            if (!startLineResult.IsSuccess)
            {
                return Result.Failure<HyperContext>(startLineResult.Errors);
            }
            else if (cancellationToken.IsCancellationRequested)
            {
                return Result.Failure<HyperContext>("The operation was cancelled.");
            }

            connection.StreamReader.AdvanceTo(sequencePosition);
            readResult = await connection.StreamReader.ReadAsync(cancellationToken);

            HyperHeaderCollection headers = new();
            while (!readResult.IsCompleted && !readResult.IsCanceled)
            {
                if (readResult.Buffer.Length > maxHeaderSize)
                {
                    return Result.Failure<HyperContext>("Data exceeds the max header size.");
                }

                Result<bool> headerResult = TryParseHeader(readResult, maxHeaderSize, ref sequencePosition, ref headers);
                if (!headerResult.IsSuccess)
                {
                    return Result.Failure<HyperContext>(headerResult.Errors);
                }
                connection.StreamReader.AdvanceTo(sequencePosition);

                // End of headers
                if (!headerResult.Value)
                {
                    break;
                }

                readResult = await connection.StreamReader.ReadAsync(cancellationToken);
            }

            return readResult.IsCanceled || cancellationToken.IsCancellationRequested
                ? Result.Failure<HyperContext>("The operation was cancelled.")
                : Result.Success<HyperContext>(new(
                    method!,
                    route!,
                    version!,
                    headers,
                    connection
                ));
        }

        private static Result TryParseStartLine(ReadResult result, int maxHeaderSize, ref SequencePosition sequencePosition, out HttpMethod? method, out Uri? route, out Version? version)
        {
            method = default;
            route = default;
            version = default;

            SequenceReader<byte> sequenceReader = new(result.Buffer);
            if (!sequenceReader.TryReadTo(out ReadOnlySpan<byte> startLine, "\r\n"u8, advancePastDelimiter: true))
            {
                return Result.Failure("Invalid data in the start line.");
            }
            else if (startLine.Length > maxHeaderSize)
            {
                return Result.Failure("Start line length exceeds max header size.");
            }

            // Split the start line into method, path, and version
            int firstSpaceIndex = startLine.IndexOf((byte)' ');
            int lastSpaceIndex = startLine.LastIndexOf((byte)' ');
            if (firstSpaceIndex == -1
                || lastSpaceIndex == -1
                || firstSpaceIndex == lastSpaceIndex)
            {
                return Result.Failure("Invalid start line data.");
            }

            // https://www.rfc-editor.org/rfc/rfc9110#section-9.1
            // All HTTP methods are case-sensitive.
            ReadOnlySpan<byte> methodSpan = startLine[..firstSpaceIndex];
            if (methodSpan.SequenceEqual("GET"u8))
            {
                method = HttpMethod.Get;
            }
            else if (methodSpan.SequenceEqual("HEAD"u8))
            {
                method = HttpMethod.Head;
            }
            else if (methodSpan.SequenceEqual("POST"u8))
            {
                method = HttpMethod.Post;
            }
            else if (methodSpan.SequenceEqual("PUT"u8))
            {
                method = HttpMethod.Put;
            }
            else if (methodSpan.SequenceEqual("DELETE"u8))
            {
                method = HttpMethod.Delete;
            }
            else if (methodSpan.SequenceEqual("CONNECT"u8))
            {
                method = HttpMethod.Connect;
            }
            else if (methodSpan.SequenceEqual("OPTIONS"u8))
            {
                method = HttpMethod.Options;
            }
            else if (methodSpan.SequenceEqual("TRACE"u8))
            {
                method = HttpMethod.Trace;
            }
            else if (methodSpan.SequenceEqual("PATCH"u8))
            {
                method = HttpMethod.Patch;
            }
            else
            {
                return Result.Failure("Invalid HTTP method specified.");
            }

            if (!Uri.TryCreate(Encoding.ASCII.GetString(startLine[(firstSpaceIndex + 1)..lastSpaceIndex]), UriKind.Relative, out route))
            {
                return Result.Failure("Invalid route specified.");
            }

            ReadOnlySpan<byte> versionSpan = startLine[(lastSpaceIndex + 1)..];
            Span<byte> loweredVersionSpan = stackalloc byte[versionSpan.Length];
            for (int i = 0; i < versionSpan.Length; i++)
            {
                // Lowercase h, t or p
                loweredVersionSpan[i] = versionSpan[i] is >= 65 and <= 90 ? (byte)(versionSpan[i] + 32) : versionSpan[i];
            }

            if (loweredVersionSpan.SequenceEqual("http/1.0"u8))
            {
                version = HttpVersion.Version10;
            }
            else if (loweredVersionSpan.SequenceEqual("http/1.1"u8))
            {
                version = HttpVersion.Version11;
            }
            else
            {
                return Result.Failure("Invalid HTTP version specified.");
            }

            sequencePosition = sequenceReader.Position;
            return Result.Success();
        }

        private static Result<bool> TryParseHeader(ReadResult result, int maxHeaderSize, ref SequencePosition sequencePosition, ref HyperHeaderCollection headers)
        {
            SequenceReader<byte> sequenceReader = new(result.Buffer);
            if (!sequenceReader.TryReadTo(out ReadOnlySpan<byte> header, "\r\n"u8, advancePastDelimiter: true))
            {
                return Result.Failure<bool>("Invalid header data.");
            }
            else if (header.Length > maxHeaderSize)
            {
                return Result.Failure<bool>("Header line length exceeds max header size.");
            }
            else if (header.Length == 0)
            {
                // We've reached the end of the headers
                // Skip the next two bytes (\r\n)
                sequencePosition = sequenceReader.Position;
                return Result.Success(false);
            }

            // Find the index of the separator (':') in the header line
            int separatorIndex = header.IndexOf((byte)':');
            if (separatorIndex == -1)
            {
                return Result.Failure<bool>("Invalid header data.");
            }

            ReadOnlySpan<byte> headerValue = header[(separatorIndex + 1)..].TrimWhitespace();
            if (!HyperHeaderCollection.IsValidValue(headerValue))
            {
                return Result.Failure<bool>("Invalid header value specified.");
            }

            headers.UnsafeAdd(Encoding.ASCII.GetString(header[..separatorIndex].TrimWhitespace()), headerValue.ToArray());
            sequencePosition = sequenceReader.Position;
            return Result.Success(true);
        }

        private static ReadOnlySpan<byte> TrimWhitespace(this ReadOnlySpan<byte> span)
        {
            int startIndex = 0;
            int endIndex = span.Length - 1;

            while (startIndex < span.Length && char.IsWhiteSpace((char)span[startIndex]))
            {
                startIndex++;
            }

            while (endIndex >= 0 && char.IsWhiteSpace((char)span[endIndex]))
            {
                endIndex--;
            }

            return span[startIndex..(endIndex + 1)];
        }
    }
}
