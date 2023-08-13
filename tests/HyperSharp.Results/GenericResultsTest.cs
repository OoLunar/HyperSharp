using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HyperSharp.Results;

namespace HyperSharp.Tests.Results
{
    [TestClass]
    public sealed class GenericResultsTest
    {
        [TestMethod]
        public void SuccessWithoutValue()
        {
            Result<int> result = Result.Success<int>();
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Errors.Any());
            Assert.IsFalse(result.HasValue);
            Assert.AreEqual(default, result.Value);
        }

        [TestMethod]
        public void SuccessWithValue()
        {
            int someValue = 42;
            Result<int> result = Result.Success(someValue);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Errors.Any());
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(someValue, result.Value);
        }

        [TestMethod]
        public void FailureWithError()
        {
            string errorMessage = "An error occurred.";
            Result<int> result = Result.Failure<int>(errorMessage);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.IsFalse(result.HasValue);
            Assert.AreEqual(default, result.Value);
            Assert.AreEqual(errorMessage, result.Errors.First().Message);
        }

        [TestMethod]
        public void FailureWithSingleErrorObject()
        {
            Error error = new("Another error.");
            Result<int> result = Result.Failure<int>(error);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.IsFalse(result.HasValue);
            Assert.AreEqual(default, result.Value);
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

            Result<int> result = Result.Failure<int>(errors);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(errors.Length, result.Errors.Count());
            Assert.IsFalse(result.HasValue);
            Assert.AreEqual(default, result.Value);
            CollectionAssert.AreEqual(errors.Select(e => e.Message).ToArray(), result.Errors.Select(e => e.Message).ToArray());
        }

        [TestMethod]
        public void FailureWithValueAndError()
        {
            int someValue = 42;
            Error error = new("Value error.");

            Result<int> result = Result.Failure(someValue, error);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(someValue, result.Value);
            Assert.AreEqual(error.Message, result.Errors.First().Message);
        }

        [TestMethod]
        public void FailureWithValueAndErrors()
        {
            int someValue = 42;
            Error[] errors = new[]
            {
                new Error("Error 1"),
                new Error("Error 2")
            };

            Result<int> result = Result.Failure(someValue, errors);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(errors.Length, result.Errors.Count());
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(someValue, result.Value);
            CollectionAssert.AreEqual(errors.Select(e => e.Message).ToArray(), result.Errors.Select(e => e.Message).ToArray());
        }
    }
}
