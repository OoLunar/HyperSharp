using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoLunar.HyperSharp.Protocol;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Tests.HyperSharp
{
    [TestClass]
    public sealed class HyperHeaderParserTests
    {
        private static readonly byte[] _headers = "GET / HTTP/1.1\r\nHost: localhost:8080\r\nUser-Agent: Mozilla/5.0 (X11; Linux x86_64; rv:109.0) Gecko/20100101 Firefox/116.0\r\nAccept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8\r\nAccept-Language: en-US,en;q=0.5\r\nAccept-Encoding: gzip, deflate, br\r\nDNT: 1\r\nAlt-Used: localhost:8080\r\nConnection: keep-alive\r\nUpgrade-Insecure-Requests: 1\r\nSec-Fetch-Dest: document\r\nSec-Fetch-Mode: navigate\r\nSec-Fetch-Site: cross-site\r\nPragma: no-cache\r\nCache-Control: no-cache\r\nTE: trailers\r\n\r\n"u8.ToArray();
        private static readonly HyperServer _hyperServer;

        static HyperHeaderParserTests()
        {
            _hyperServer = new(new());
            _hyperServer.Start();
        }

        public static HyperConnection CreateHyperConnection(string? headers = null)
        {
            byte[] headerBytes = headers is null ? _headers : Encoding.ASCII.GetBytes(headers);
            MemoryStream stream = new();
            stream.Write(headerBytes);
            stream.Seek(0, SeekOrigin.Begin);
            return new(stream, _hyperServer);
        }

        [TestMethod]
        public async Task SuccessAsync()
        {
            HyperConnection connection = CreateHyperConnection();
            Result<HyperContext> parseResult = await HyperHeaderParser.TryParseHeadersAsync(2048, connection);

            Assert.IsTrue(parseResult.IsSuccess);
            HyperContext context = parseResult.Value!;

            Assert.AreEqual(HttpMethod.Get, context.Method);
            Assert.AreEqual("http://localhost:8080/", context.Route.OriginalString);
            Assert.AreEqual(HttpVersion.Version11, context.Version);
            Assert.AreEqual(15, context.Headers.Count);
            Assert.AreEqual("localhost:8080", context.Headers["Host"][0]);
            Assert.AreEqual("Mozilla/5.0 (X11; Linux x86_64; rv:109.0) Gecko/20100101 Firefox/116.0", context.Headers["User-Agent"][0]);
        }

        [TestMethod]
        public async Task MaxHeaderSizeExceededAsync()
        {
            HyperConnection connection = CreateHyperConnection();
            Result<HyperContext> parseResult = await HyperHeaderParser.TryParseHeadersAsync(128, connection);

            Assert.IsFalse(parseResult.IsSuccess);
            Assert.AreEqual("Data exceeds the max header size.", parseResult.Errors[0].Message);
        }

        [TestMethod]
        public async Task InvalidStartLineAsync()
        {
            HyperConnection connection = CreateHyperConnection("INVALID_LINE\r\nHost: localhost:8080\r\n\r\n");
            Result<HyperContext> parseResult = await HyperHeaderParser.TryParseHeadersAsync(2048, connection);

            Assert.IsFalse(parseResult.IsSuccess);
            Assert.AreEqual("Invalid start line data.", parseResult.Errors[0].Message);
        }

        [TestMethod]
        public async Task InvalidHeaderLineAsync()
        {
            HyperConnection connection = CreateHyperConnection("GET / HTTP/1.1\r\nInvalidHeaderLine\r\n\r\n");
            Result<HyperContext> parseResult = await HyperHeaderParser.TryParseHeadersAsync(2048, connection);

            Assert.IsFalse(parseResult.IsSuccess);
            Assert.AreEqual("Invalid header data.", parseResult.Errors[0].Message);
        }

        [TestMethod]
        public async Task InvalidHttpMethodAsync()
        {
            HyperConnection connection = CreateHyperConnection("INVALID_METHOD / HTTP/1.1\r\nHost: localhost:8080\r\n\r\n");
            Result<HyperContext> parseResult = await HyperHeaderParser.TryParseHeadersAsync(2048, connection);

            Assert.IsFalse(parseResult.IsSuccess);
            Assert.AreEqual("Invalid HTTP method specified.", parseResult.Errors[0].Message);
        }

        [TestMethod]
        public async Task InvalidHttpVersionAsync()
        {
            HyperConnection connection = CreateHyperConnection("GET / INVALID_VERSION\r\nHost: localhost:8080\r\n\r\n");
            Result<HyperContext> parseResult = await HyperHeaderParser.TryParseHeadersAsync(2048, connection);

            Assert.IsFalse(parseResult.IsSuccess);
            Assert.AreEqual("Invalid HTTP version specified.", parseResult.Errors[0].Message);
        }

        [TestMethod]
        public async Task MissingStartLineSeparatorAsync()
        {
            HyperConnection connection = CreateHyperConnection("GET / HTTP/1.1");
            Result<HyperContext> parseResult = await HyperHeaderParser.TryParseHeadersAsync(2048, connection);

            Assert.IsFalse(parseResult.IsSuccess);
            Assert.AreEqual("Invalid data in the start line.", parseResult.Errors[0].Message);
        }

        [TestMethod]
        public async Task MissingLineSeparatorAsync()
        {
            HyperConnection connection = CreateHyperConnection("GET / HTTP/1.1\r\nHost: localhost:8080");
            Result<HyperContext> parseResult = await HyperHeaderParser.TryParseHeadersAsync(2048, connection);

            Assert.IsFalse(parseResult.IsSuccess);
            Assert.AreEqual("Invalid header data.", parseResult.Errors[0].Message);
        }

        [TestMethod]
        public async Task MissingDoubleLineSeparatorAsync()
        {
            HyperConnection connection = CreateHyperConnection("GET / HTTP/1.1\r\nHost: localhost:8080\r\n");
            Result<HyperContext> parseResult = await HyperHeaderParser.TryParseHeadersAsync(2048, connection);

            Assert.IsFalse(parseResult.IsSuccess);
            Assert.AreEqual("Invalid header data.", parseResult.Errors[0].Message);
        }
    }
}
