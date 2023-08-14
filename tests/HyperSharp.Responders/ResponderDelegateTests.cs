using System.Linq;
using HyperSharp.Responders;
using HyperSharp.Results;
using HyperSharp.Tests.Responders.IResponder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HyperSharp.Tests.Responders
{
    [TestClass]
    public sealed class ResponderTests
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

        [TestMethod]
        public void SuccessWithValue()
        {
            ResponderCompiler compiler = new();
            compiler.Search(new[] { typeof(SuccessWithValueResponder) });
            ResponderDelegate<string, string> responder = compiler.CompileResponders<string, string>(Constants.ServiceProvider);

            string responseValue = "Success with value.";
            Result<string> result = responder(responseValue);
            Assert.IsFalse(result.Errors.Any());
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(responseValue, result.Value);
        }

        [TestMethod]
        public void Failure()
        {
            ResponderCompiler compiler = new();
            compiler.Search(new[] { typeof(FailureResponder) });
            ResponderDelegate<string, string> responder = compiler.CompileResponders<string, string>(Constants.ServiceProvider);

            Result<string> result = responder(string.Empty);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsFalse(result.HasValue);
            Assert.IsNull(result.Value);
        }

        [TestMethod]
        public void FailureWithValue()
        {
            ResponderCompiler compiler = new();
            compiler.Search(new[] { typeof(FailureWithValueResponder) });
            ResponderDelegate<string, string> responder = compiler.CompileResponders<string, string>(Constants.ServiceProvider);

            string responseValue = "Failure with value.";
            Result<string> result = responder(responseValue);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(responseValue, result.Value);
        }

        [TestMethod]
        public void FailureByException()
        {
            ResponderCompiler compiler = new();
            compiler.Search(new[] { typeof(FailureByExceptionResponder) });
            ResponderDelegate<string, string> responder = compiler.CompileResponders<string, string>(Constants.ServiceProvider);

            Result<string> result = responder(string.Empty);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsFalse(result.HasValue);
            Assert.IsNull(result.Value);
        }
    }
}
