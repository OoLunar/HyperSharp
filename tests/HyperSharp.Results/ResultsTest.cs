using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Tests.Results
{
    [TestClass]
    public sealed class ResultsTest
    {
        [TestMethod]
        public void Success()
        {
            Result result = Result.Success();
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Errors.Any());
            Assert.IsFalse(result.HasValue);
            Assert.IsNull(result.Value);
        }

        [TestMethod]
        public void SuccessWithValue()
        {
            object someValue = "Hello, world!";
            Result result = Result.Success(someValue);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Errors.Any());
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(someValue, result.Value);
        }

        [TestMethod]
        public void FailureWithError()
        {
            string errorMessage = "An error occurred.";
            Result result = Result.Failure(errorMessage);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.IsFalse(result.HasValue);
            Assert.IsNull(result.Value);
            Assert.AreEqual(errorMessage, result.Errors.First().Message);
        }

        [TestMethod]
        public void FailureWithSingleErrorObject()
        {
            Error error = new("Another error.");
            Result result = Result.Failure(error);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.IsFalse(result.HasValue);
            Assert.IsNull(result.Value);
            Assert.AreEqual(error.Message, result.Errors.First().Message);
        }

        [TestMethod]
        public void FailureWithMultipleErrors()
        {
            Error[] errors = new[]
            {
                new Error("Error 1"),
                new Error("Error 2"),
                new Error("Error 3")
            };

            Result result = Result.Failure(errors);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(errors.Length, result.Errors.Count());
            Assert.IsFalse(result.HasValue);
            Assert.IsNull(result.Value);
            CollectionAssert.AreEqual(errors.Select(e => e.Message).ToArray(), result.Errors.Select(e => e.Message).ToArray());
        }

        [TestMethod]
        public void FailureWithValueAndError()
        {
            string someValue = "Value with error.";
            Error error = new("Value error.");

            Result result = Result.Failure(someValue, error);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(someValue, result.Value);
            Assert.AreEqual(error.Message, result.Errors.First().Message);
        }

        [TestMethod]
        public void FailureWithValueAndErrors()
        {
            string someValue = "Value with errors.";
            Error[] errors = new[]
            {
                new Error("Error 1"),
                new Error("Error 2")
            };

            Result result = Result.Failure(someValue, errors);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(errors.Length, result.Errors.Count());
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(someValue, result.Value);
            CollectionAssert.AreEqual(errors.Select(e => e.Message).ToArray(), result.Errors.Select(e => e.Message).ToArray());
        }
    }
}
