using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoLunar.HyperSharp.Responders;
using OoLunar.HyperSharp.Results;
using OoLunar.HyperSharp.Tests.Responders.IResponder;

namespace OoLunar.HyperSharp.Tests.Responders
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
