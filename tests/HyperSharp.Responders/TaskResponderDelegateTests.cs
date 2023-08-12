using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoLunar.HyperSharp.Responders;
using OoLunar.HyperSharp.Results;
using OoLunar.HyperSharp.Tests.Responders.ITaskResponder;

namespace OoLunar.HyperSharp.Tests.Responders
{
    [TestClass]
    public sealed class TaskResponderDelegateTests
    {
        [TestMethod]
        public async Task AwaitSuccessAsync()
        {
            ResponderCompiler compiler = new();
            compiler.Search(new[] { typeof(AsyncSuccessTaskResponder) });
            ValueTaskResponderDelegate<string, string> responder = compiler.CompileAsyncResponders<string, string>(Constants.ServiceProvider);

            Result<string> result = await responder(string.Empty);
            Assert.IsFalse(result.Errors.Any());
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.HasValue);
            Assert.IsNull(result.Value);
        }

        [TestMethod]
        public async Task SuccessAsync()
        {
            ResponderCompiler compiler = new();
            compiler.Search(new[] { typeof(SuccessTaskResponder) });
            ValueTaskResponderDelegate<string, string> responder = compiler.CompileAsyncResponders<string, string>(Constants.ServiceProvider);

            Result<string> result = await responder(string.Empty);
            Assert.IsFalse(result.Errors.Any());
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.HasValue);
            Assert.IsNull(result.Value);
        }

        [TestMethod]
        public async Task SuccessWithValueAsync()
        {
            ResponderCompiler compiler = new();
            compiler.Search(new[] { typeof(SuccessWithValueTaskResponder) });
            ValueTaskResponderDelegate<string, string> responder = compiler.CompileAsyncResponders<string, string>(Constants.ServiceProvider);

            string responseValue = "Success with value.";
            Result<string> result = await responder(responseValue);
            Assert.IsFalse(result.Errors.Any());
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(responseValue, result.Value);
        }

        [TestMethod]
        public async Task FailureAsync()
        {
            ResponderCompiler compiler = new();
            compiler.Search(new[] { typeof(FailureTaskResponder) });
            ValueTaskResponderDelegate<string, string> responder = compiler.CompileAsyncResponders<string, string>(Constants.ServiceProvider);

            Result<string> result = await responder(string.Empty);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.IsFalse(result.IsSuccess);
            Assert.IsFalse(result.HasValue);
            Assert.IsNull(result.Value);
        }

        [TestMethod]
        public async Task FailureWithValueAsync()
        {
            ResponderCompiler compiler = new();
            compiler.Search(new[] { typeof(FailureWithValueTaskResponder) });
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
        public async Task FailureByExceptionAsync()
        {
            ResponderCompiler compiler = new();
            compiler.Search(new[] { typeof(FailureByExceptionTaskResponder) });
            ValueTaskResponderDelegate<string, string> responder = compiler.CompileAsyncResponders<string, string>(Constants.ServiceProvider);

            Result<string> result = await responder(string.Empty);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.IsFalse(result.IsSuccess);
            Assert.IsFalse(result.HasValue);
            Assert.IsNull(result.Value);
        }
    }
}
