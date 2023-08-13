using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HyperSharp.Responders;
using HyperSharp.Results;
using HyperSharp.Tests.Responders.IValueTaskResponder;

namespace HyperSharp.Tests.Responders
{
    [TestClass]
    public sealed class ValueTaskResponderDelegateTests
    {
        [TestMethod]
        public async Task AwaitValueSuccessAsync()
        {
            ResponderCompiler compiler = new();
            compiler.Search(new[] { typeof(AsyncSuccessValueTaskResponder) });
            ValueTaskResponderDelegate<string, string> responder = compiler.CompileAsyncResponders<string, string>(Constants.ServiceProvider);

            Result<string> result = await responder(string.Empty);
            Assert.IsFalse(result.Errors.Any());
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.HasValue);
            Assert.IsNull(result.Value);
        }

        [TestMethod]
        public async Task ValueSuccessAsync()
        {
            ResponderCompiler compiler = new();
            compiler.Search(new[] { typeof(SuccessValueTaskResponder) });
            ValueTaskResponderDelegate<string, string> responder = compiler.CompileAsyncResponders<string, string>(Constants.ServiceProvider);

            Result<string> result = await responder(string.Empty);
            Assert.IsFalse(result.Errors.Any());
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.HasValue);
            Assert.IsNull(result.Value);
        }

        [TestMethod]
        public async Task ValueSuccessWithValueAsync()
        {
            ResponderCompiler compiler = new();
            compiler.Search(new[] { typeof(SuccessWithValueValueTaskResponder) });
            ValueTaskResponderDelegate<string, string> responder = compiler.CompileAsyncResponders<string, string>(Constants.ServiceProvider);

            string responseValue = "Success with value.";
            Result<string> result = await responder(responseValue);
            Assert.IsFalse(result.Errors.Any());
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(responseValue, result.Value);
        }

        [TestMethod]
        public async Task ValueFailureAsync()
        {
            ResponderCompiler compiler = new();
            compiler.Search(new[] { typeof(FailureValueTaskResponder) });
            ValueTaskResponderDelegate<string, string> responder = compiler.CompileAsyncResponders<string, string>(Constants.ServiceProvider);

            Result<string> result = await responder(string.Empty);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.IsFalse(result.IsSuccess);
            Assert.IsFalse(result.HasValue);
            Assert.IsNull(result.Value);
        }

        [TestMethod]
        public async Task ValueFailureWithValueAsync()
        {
            ResponderCompiler compiler = new();
            compiler.Search(new[] { typeof(FailureWithValueValueTaskResponder) });
            ValueTaskResponderDelegate<string, string> responder = compiler.CompileAsyncResponders<string, string>(Constants.ServiceProvider);

            string responseValue = "Failure with value.";
            Result<string> result = await responder(responseValue);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual("Failure Error", result.Errors.First().Message);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(responseValue, result.Value);
        }

        [TestMethod]
        public async Task ValueFailureByExceptionAsync()
        {
            ResponderCompiler compiler = new();
            compiler.Search(new[] { typeof(FailureByExceptionValueTaskResponder) });
            ValueTaskResponderDelegate<string, string> responder = compiler.CompileAsyncResponders<string, string>(Constants.ServiceProvider);

            Result<string> result = await responder(string.Empty);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.IsFalse(result.IsSuccess);
            Assert.IsFalse(result.HasValue);
            Assert.IsNull(result.Value);
        }
    }
}
