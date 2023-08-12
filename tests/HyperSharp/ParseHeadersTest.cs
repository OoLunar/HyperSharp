using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoLunar.HyperSharp.Protocol;

namespace OoLunar.HyperSharp.Tests.HyperSharp
{
    [TestClass]
    public sealed class ParseHeadersTest
    {
        /// <summary>
        /// Tests adding a valid single header value.
        /// </summary>
        [TestMethod]
        public void TestAddSingleHeaderValue()
        {
            HyperHeaderCollection headerCollection = new() { { "Content-Type", "application/json" } };

            Assert.IsTrue(headerCollection.ContainsKey("Content-Type"));
            Assert.IsTrue(headerCollection.TryGetValue("Content-Type", out IReadOnlyList<string>? contentTypeValue));
            Assert.AreEqual("application/json", contentTypeValue[0]);
        }

        /// <summary>
        /// Tests adding multiple header values to a single header.
        /// </summary>
        [TestMethod]
        public void TestAddMultipleHeaderValues()
        {
            HyperHeaderCollection headerCollection = new() { { "Accept", new List<string> { "application/json", "text/plain" } } };

            Assert.IsTrue(headerCollection.ContainsKey("Accept"));
            Assert.IsTrue(headerCollection.TryGetValue("Accept", out IReadOnlyList<string>? acceptValues));
            CollectionAssert.AreEqual(new List<string> { "application/json", "text/plain" }, acceptValues.ToList());
        }

        /// <summary>
        /// Tests setting a valid single header value.
        /// </summary>
        [TestMethod]
        public void TestSetSingleHeaderValue()
        {
            HyperHeaderCollection headerCollection = new();
            headerCollection.Set("User-Agent", "MyApp/1.0");

            Assert.IsTrue(headerCollection.ContainsKey("User-Agent"));
            Assert.IsTrue(headerCollection.TryGetValue("User-Agent", out IReadOnlyList<string>? userAgentValue));
            Assert.AreEqual("MyApp/1.0", userAgentValue[0]);
        }

        /// <summary>
        /// Tests setting multiple header values to a single header.
        /// </summary>
        [TestMethod]
        public void TestSetMultipleHeaderValues()
        {
            HyperHeaderCollection headerCollection = new();
            headerCollection.Set("Accept", new List<string> { "application/json", "text/plain" });

            Assert.IsTrue(headerCollection.ContainsKey("Accept"));
            Assert.IsTrue(headerCollection.TryGetValue("Accept", out IReadOnlyList<string>? acceptValues));
            CollectionAssert.AreEqual(new List<string> { "application/json", "text/plain" }, acceptValues.ToList());
        }

        /// <summary>
        /// Tests removing a header.
        /// </summary>
        [TestMethod]
        public void TestRemoveHeader()
        {
            HyperHeaderCollection headerCollection = new() { { "Authorization", "Bearer token" } };
            Assert.IsTrue(headerCollection.ContainsKey("Authorization"));

            headerCollection.Remove("Authorization");
            Assert.IsFalse(headerCollection.ContainsKey("Authorization"));
        }

        /// <summary>
        /// Tests adding an invalid header name.
        /// </summary>
        [TestMethod]
        public void TestInvalidHeaderNameThrowsException()
        {
            HyperHeaderCollection headerCollection = new();
            Assert.ThrowsException<ArgumentException>(() => headerCollection.Add("Invalid Name", "value"));
        }

        /// <summary>
        /// Tests adding an invalid header value.
        /// </summary>
        [TestMethod]
        public void TestInvalidHeaderValueThrowsException()
        {
            HyperHeaderCollection headerCollection = new();
            Assert.ThrowsException<ArgumentException>(() => headerCollection.Add("Header", "Invalid Value 🖕"));
        }

        /// <summary>
        /// Tests normalizing a header name.
        /// </summary>
        [TestMethod]
        public void TestNormalizeHeaderName()
        {
            string originalName = "content-type";
            string normalized = HyperHeaderCollection.NormalizeHeaderName(originalName);
            Assert.AreEqual("Content-Type", normalized);
        }

        /// <summary>
        /// Tests indexing a header name, which skips header name normalization.
        /// </summary>
        [TestMethod]
        public void TestNormalizeHeaderNameIndex()
        {
            string originalName = "content-type";
            HyperHeaderCollection headerCollection = new() { { originalName, "application/json" } };
            Assert.ThrowsException<KeyNotFoundException>(() => headerCollection[originalName]);
        }

        /// <summary>
        /// Tests validating a valid header.
        /// </summary>
        [TestMethod]
        public void TestIsValidHeader() => Assert.IsTrue(HyperHeaderCollection.IsValidHeader("Accept: application/json"));

        /// <summary>
        /// Tests validating an invalid header.
        /// </summary>
        [TestMethod]
        public void TestIsValidHeaderInvalid() => Assert.IsFalse(HyperHeaderCollection.IsValidHeader(": invalid header"));
    }
}
