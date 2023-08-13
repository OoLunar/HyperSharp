using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HyperSharp.Responders;
using HyperSharp.Results;
using HyperSharp.Tests.Responders.IResponder;

namespace HyperSharp.Tests.Responders
{
    [TestClass]
    public sealed class DependentResponderTests
    {
        [TestMethod]
        public void Success()
        {
            ResponderCompiler compiler = new();
            compiler.Search(new[] { typeof(SuccessResponder) });
            ResponderDelegate<string, string> responder = compiler.CompileResponders<string, string>(Constants.ServiceProvider);

            Result<string> result = responder(string.Empty);
            Assert.IsFalse(result.Errors.Any());
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.HasValue);
            Assert.IsNull(result.Value);
        }
    }
}
