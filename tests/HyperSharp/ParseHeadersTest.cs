using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HyperSharp.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable CS8604 // Possible null reference argument.
// We're disabling this warning because we're testing the behavior of the HyperHeaderCollection for throwing on null arguments.
namespace HyperSharp.Tests.HyperSharp
{
    [TestClass]
    public sealed class ParseHeadersTest
    {
        [TestMethod]
        public void AddWithValidKeyAndValueShouldAddToCollection()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string key = "Key1";
            string value = "Value1";

            // Act
            headerCollection.Add(key, value);

            // Assert
            Assert.AreEqual(1, headerCollection.Count);
            KeyValuePair<string, byte[]> addedHeader = headerCollection.First();
            Assert.AreEqual(key, addedHeader.Key);
            Assert.AreEqual(value, Encoding.ASCII.GetString(addedHeader.Value));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddWithNullKeyShouldThrowArgumentNullException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string? key = null;
            string value = "Value1";

            // Act
            headerCollection.Add(key, value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddWithNullValueShouldThrowArgumentNullException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string key = "Key1";
            string? value = null;

            // Act
            headerCollection.Add(key, value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddWithInvalidKeyShouldThrowArgumentException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            // Use an invalid key (e.g., contains spaces)
            string key = "Invalid Key";
            string value = "Value1";

            // Act
            headerCollection.Add(key, value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddWithInvalidValueShouldThrowArgumentException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string key = "Key1";
            // Use an invalid value (e.g., contains control characters)
            string value = "Invalid\u001FValue";

            // Act
            headerCollection.Add(key, value);
        }

        [TestMethod]
        public void UnsafeAddWithValidKeyAndValueShouldAddToCollection()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string key = "Key1";
            byte[] value = Encoding.UTF8.GetBytes("Value1");

            // Act
            headerCollection.UnsafeAdd(key, value);

            // Assert
            Assert.AreEqual(1, headerCollection.Count);
            KeyValuePair<string, byte[]> addedHeader = headerCollection.First();
            Assert.AreEqual(key, addedHeader.Key);
            CollectionAssert.AreEqual(value, addedHeader.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UnsafeAddWithNullKeyShouldThrowArgumentNullException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string? key = null;
            byte[] value = Encoding.UTF8.GetBytes("Value1");

            // Act
            headerCollection.UnsafeAdd(key, value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UnsafeAddWithInvalidKeyShouldThrowArgumentException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            // Use an invalid key (e.g., contains spaces)
            string key = "Invalid Key";
            byte[] value = Encoding.UTF8.GetBytes("Value1");

            // Act
            headerCollection.UnsafeAdd(key, value);
        }

        [TestMethod]
        public void ContainsWithExistingKeyShouldReturnTrue()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string key = "Key1";
            headerCollection.Add(key, "Value1");

            // Act
            bool containsHeader = headerCollection.Contains(key);

            // Assert
            Assert.IsTrue(containsHeader);
        }

        [TestMethod]
        public void ContainsWithNonExistingKeyShouldReturnFalse()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string key = "Key1";

            // Act
            bool containsHeader = headerCollection.Contains(key);

            // Assert
            Assert.IsFalse(containsHeader);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ContainsWithNullKeyShouldThrowArgumentNullException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string? key = null;

            // Act
            _ = headerCollection.Contains(key);
        }

        [TestMethod]
        public void GetWithExistingKeyShouldReturnValue()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToGet = "Key1";
            string valueToGet = "Value1";
            headerCollection.Add(keyToGet, valueToGet);

            // Act
            string retrievedValue = headerCollection.Get(keyToGet);

            // Assert
            Assert.AreEqual(valueToGet, retrievedValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetWithNullKeyShouldThrowArgumentNullException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string? keyToGet = null;

            // Act
            _ = headerCollection.Get(keyToGet);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetWithInvalidKeyShouldThrowArgumentException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();

            // Use an invalid key (e.g., contains spaces)
            string keyToGet = "Invalid Key";

            // Act
            _ = headerCollection.Get(keyToGet);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void GetWithNonExistingKeyShouldThrowKeyNotFoundException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToGet = "Key1";

            // Act
            _ = headerCollection.Get(keyToGet);
        }

        [TestMethod]
        public void IndexOfWithExistingKeyShouldReturnCorrectIndex()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string key = "Key1";
            headerCollection.Add(key, "Value1");

            // Act
            int index = headerCollection.IndexOf(key);

            // Assert
            Assert.AreEqual(0, index);
        }

        [TestMethod]
        public void IndexOfWithNonExistingKeyShouldReturnNegativeOne()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string key = "Key1";

            // Act
            int index = headerCollection.IndexOf(key);

            // Assert
            Assert.AreEqual(-1, index);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IndexOfWithNullKeyShouldThrowArgumentNullException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string? key = null;

            // Act
            _ = headerCollection.IndexOf(key);
        }

        [TestMethod]
        public void InsertWithValidIndexShouldInsertHeaderAtIndex()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new()
            {
                { "Key1", "Value1" },
                { "Key2", "Value2" }
            };

            int indexToInsert = 1;
            string keyToInsert = "NewKey";
            string valueToInsert = "NewValue";

            // Act
            headerCollection.Insert(indexToInsert, keyToInsert, valueToInsert);

            // Assert
            Assert.AreEqual(3, headerCollection.Count);
            Assert.AreEqual(keyToInsert, headerCollection[indexToInsert].Key);
            Assert.AreEqual(valueToInsert, Encoding.ASCII.GetString(headerCollection[indexToInsert].Value));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InsertWithNullKeyShouldThrowArgumentNullException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            int indexToInsert = 0;
            string? keyToInsert = null;
            string valueToInsert = "Value1";

            // Act
            headerCollection.Insert(indexToInsert, keyToInsert, valueToInsert);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InsertWithNullValueShouldThrowArgumentNullException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            int indexToInsert = 0;
            string keyToInsert = "Key1";
            string? valueToInsert = null;

            // Act
            headerCollection.Insert(indexToInsert, keyToInsert, valueToInsert);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InsertWithInvalidKeyShouldThrowArgumentException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            int indexToInsert = 0;
            // Use an invalid key (e.g., contains spaces)
            string keyToInsert = "Invalid Key";
            string valueToInsert = "Value1";

            // Act
            headerCollection.Insert(indexToInsert, keyToInsert, valueToInsert);
        }

        [TestMethod]
        public void UnsafeInsertWithValidIndexShouldInsertHeaderAtIndex()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new()
            {
                { "Key1", "Value1" },
                { "Key2", "Value2" }
            };

            int indexToInsert = 1;
            string keyToInsert = "NewKey";
            byte[] valueToInsert = Encoding.UTF8.GetBytes("NewValue");

            // Act
            headerCollection.UnsafeInsert(indexToInsert, keyToInsert, valueToInsert);

            // Assert
            Assert.AreEqual(3, headerCollection.Count);
            Assert.AreEqual(keyToInsert, headerCollection[indexToInsert].Key);
            CollectionAssert.AreEqual(valueToInsert, headerCollection[indexToInsert].Value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UnsafeInsertWithNullKeyShouldThrowArgumentNullException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            int indexToInsert = 0;
            string? keyToInsert = null;
            byte[] valueToInsert = Encoding.UTF8.GetBytes("Value1");

            // Act
            headerCollection.UnsafeInsert(indexToInsert, keyToInsert, valueToInsert);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UnsafeInsertWithInvalidKeyShouldThrowArgumentException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            int indexToInsert = 0;
            // Use an invalid key (e.g., contains spaces)
            string keyToInsert = "Invalid Key";
            byte[] valueToInsert = Encoding.UTF8.GetBytes("Value1");

            // Act
            headerCollection.UnsafeInsert(indexToInsert, keyToInsert, valueToInsert);
        }

        [TestMethod]
        public void RemoveWithExistingKeyShouldRemoveHeaderAndReturnTrue()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToRemove = "Key1";
            headerCollection.Add(keyToRemove, "Value1");
            headerCollection.Add("Key2", "Value2");

            // Act
            bool removed = headerCollection.Remove(keyToRemove);

            // Assert
            Assert.IsTrue(removed);
            Assert.AreEqual(1, headerCollection.Count);
            Assert.IsFalse(headerCollection.Contains(keyToRemove));
        }

        [TestMethod]
        public void RemoveWithNonExistingKeyShouldReturnFalse()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToRemove = "Key1";
            headerCollection.Add("Key2", "Value2");

            // Act
            bool removed = headerCollection.Remove(keyToRemove);

            // Assert
            Assert.IsFalse(removed);
            Assert.AreEqual(1, headerCollection.Count); // Count should remain unchanged
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveWithNullKeyShouldThrowArgumentNullException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string? keyToRemove = null;

            // Act
            _ = headerCollection.Remove(keyToRemove);
        }

        [TestMethod]
        public void RemoveNWithExistingKeyShouldRemoveNHeadersAndReturnCount()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToRemove = "Key1";
            headerCollection.Add(keyToRemove, "Value1");
            headerCollection.Add(keyToRemove, "Value2");
            headerCollection.Add("Key2", "Value3");

            // Act
            int removedCount = headerCollection.RemoveN(keyToRemove, 2);

            // Assert
            Assert.AreEqual(2, removedCount);
            Assert.AreEqual(1, headerCollection.Count);
            Assert.IsFalse(headerCollection.Contains(keyToRemove));
        }

        [TestMethod]
        public void RemoveNWithNonExistingKeyShouldReturnZero()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToRemove = "Key1";
            headerCollection.Add("Key2", "Value1");

            // Act
            int removedCount = headerCollection.RemoveN(keyToRemove, 1);

            // Assert
            Assert.AreEqual(0, removedCount);
            Assert.AreEqual(1, headerCollection.Count); // Count should remain unchanged
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveNWithNullKeyShouldThrowArgumentNullException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string? keyToRemove = null;

            // Act
            _ = headerCollection.RemoveN(keyToRemove, 1);
        }

        [TestMethod]
        public void RemoveAllWithExistingKeyShouldRemoveAllHeadersAndReturnCount()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToRemove = "Key1";
            headerCollection.Add(keyToRemove, "Value1");
            headerCollection.Add(keyToRemove, "Value2");
            headerCollection.Add("Key2", "Value3");

            // Act
            int removedCount = headerCollection.RemoveAll(keyToRemove);

            // Assert
            Assert.AreEqual(2, removedCount);
            Assert.AreEqual(1, headerCollection.Count);
            Assert.IsFalse(headerCollection.Contains(keyToRemove));
        }

        [TestMethod]
        public void RemoveAllWithNonExistingKeyShouldReturnZero()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToRemove = "Key1";
            headerCollection.Add("Key2", "Value1");

            // Act
            int removedCount = headerCollection.RemoveAll(keyToRemove);

            // Assert
            Assert.AreEqual(0, removedCount);
            Assert.AreEqual(1, headerCollection.Count); // Count should remain unchanged
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveAllWithNullKeyShouldThrowArgumentNullException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string? keyToRemove = null;

            // Act
            _ = headerCollection.RemoveAll(keyToRemove);
        }

        [TestMethod]
        public void SetWithExistingKeyShouldRemoveAllAndAddNewHeader()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToRemove = "Key1";
            headerCollection.Add(keyToRemove, "Value1");
            headerCollection.Add(keyToRemove, "Value2");

            string keyToSet = "Key1";
            string valueToSet = "NewValue";

            // Act
            headerCollection.Set(keyToSet, valueToSet);

            // Assert
            Assert.AreEqual(1, headerCollection.Count);
            Assert.AreEqual(keyToSet, headerCollection[0].Key);
            Assert.AreEqual(valueToSet, Encoding.ASCII.GetString(headerCollection[0].Value));
        }

        [TestMethod]
        public void SetWithNonExistingKeyShouldAddNewHeader()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToSet = "Key1";
            string valueToSet = "NewValue";

            // Act
            headerCollection.Set(keyToSet, valueToSet);

            // Assert
            Assert.AreEqual(1, headerCollection.Count);
            Assert.AreEqual(keyToSet, headerCollection[0].Key);
            Assert.AreEqual(valueToSet, Encoding.ASCII.GetString(headerCollection[0].Value));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetWithNullKeyShouldThrowArgumentNullException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string? keyToSet = null;
            string valueToSet = "Value1";

            // Act
            headerCollection.Set(keyToSet, valueToSet);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetWithNullValueShouldThrowArgumentNullException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToSet = "Key1";
            string? valueToSet = null;

            // Act
            headerCollection.Set(keyToSet, valueToSet);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetWithInvalidKeyShouldThrowArgumentException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            // Use an invalid key (e.g., contains spaces)
            string keyToSet = "Invalid Key";
            string valueToSet = "Value1";

            // Act
            headerCollection.Set(keyToSet, valueToSet);
        }

        [TestMethod]
        public void UnsafeSetWithExistingKeyShouldRemoveAllAndAddNewHeader()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToRemove = "Key1";
            headerCollection.Add(keyToRemove, "Value1");
            headerCollection.Add(keyToRemove, "Value2");

            string keyToSet = "Key1";
            byte[] valueToSet = Encoding.UTF8.GetBytes("NewValue");

            // Act
            headerCollection.UnsafeSet(keyToSet, valueToSet);

            // Assert
            Assert.AreEqual(1, headerCollection.Count);
            Assert.AreEqual(keyToSet, headerCollection[0].Key);
            CollectionAssert.AreEqual(valueToSet, headerCollection[0].Value);
        }

        [TestMethod]
        public void UnsafeSetWithNonExistingKeyShouldAddNewHeader()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToSet = "Key1";
            byte[] valueToSet = Encoding.UTF8.GetBytes("NewValue");

            // Act
            headerCollection.UnsafeSet(keyToSet, valueToSet);

            // Assert
            Assert.AreEqual(1, headerCollection.Count);
            Assert.AreEqual(keyToSet, headerCollection[0].Key);
            CollectionAssert.AreEqual(valueToSet, headerCollection[0].Value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UnsafeSetWithNullKeyShouldThrowArgumentNullException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string? keyToSet = null;
            byte[] valueToSet = Encoding.UTF8.GetBytes("Value1");

            // Act
            headerCollection.UnsafeSet(keyToSet, valueToSet);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UnsafeSetWithInvalidKeyShouldThrowArgumentException()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            // Use an invalid key (e.g., contains spaces)
            string keyToSet = "Invalid Key";
            byte[] valueToSet = Encoding.UTF8.GetBytes("Value1");

            // Act
            headerCollection.UnsafeSet(keyToSet, valueToSet);
        }

        [TestMethod]
        public void TryAddWithNonExistingKeyShouldAddHeaderAndReturnTrue()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToAdd = "Key1";
            string valueToAdd = "Value1";

            // Act
            bool added = headerCollection.TryAdd(keyToAdd, valueToAdd);

            // Assert
            Assert.IsTrue(added);
            Assert.AreEqual(1, headerCollection.Count);
            Assert.IsTrue(headerCollection.Contains(keyToAdd));
            Assert.AreEqual(valueToAdd, Encoding.ASCII.GetString(headerCollection[headerCollection.IndexOf(keyToAdd)].Value));
        }

        [TestMethod]
        public void TryAddWithExistingKeyShouldReturnFalseAndNotAddHeader()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToAdd = "Key1";
            headerCollection.Add(keyToAdd, "Value1");

            string valueToAdd = "NewValue";

            // Act
            bool added = headerCollection.TryAdd(keyToAdd, valueToAdd);

            // Assert
            Assert.IsFalse(added);
            Assert.AreEqual(1, headerCollection.Count); // Count should remain unchanged
            Assert.AreEqual("Value1", Encoding.ASCII.GetString(headerCollection[headerCollection.IndexOf(keyToAdd)].Value)); // Value should not change
        }

        [TestMethod]
        public void TryAddWithNullKeyShouldReturnFalseAndNotAddHeader()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string? keyToAdd = null;
            string valueToAdd = "Value1";

            // Act
            bool added = headerCollection.TryAdd(keyToAdd, valueToAdd);

            // Assert
            Assert.IsFalse(added);
            Assert.AreEqual(0, headerCollection.Count); // Count should remain unchanged
        }

        [TestMethod]
        public void TryAddWithInvalidKeyShouldReturnFalseAndNotAddHeader()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            // Use an invalid key (e.g., contains spaces)
            string keyToAdd = "Invalid Key";
            string valueToAdd = "Value1";

            // Act
            bool added = headerCollection.TryAdd(keyToAdd, valueToAdd);

            // Assert
            Assert.IsFalse(added);
            Assert.AreEqual(0, headerCollection.Count); // Count should remain unchanged
        }

        [TestMethod]
        public void UnsafeTryAddWithNonExistingKeyShouldAddHeaderAndReturnTrue()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToAdd = "Key1";
            byte[] valueToAdd = Encoding.UTF8.GetBytes("Value1");

            // Act
            bool added = headerCollection.UnsafeTryAdd(keyToAdd, valueToAdd);

            // Assert
            Assert.IsTrue(added);
            Assert.AreEqual(1, headerCollection.Count);
            Assert.IsTrue(headerCollection.Contains(keyToAdd));
            CollectionAssert.AreEqual(valueToAdd, headerCollection[headerCollection.IndexOf(keyToAdd)].Value);
        }

        [TestMethod]
        public void UnsafeTryAddWithExistingKeyShouldReturnFalseAndNotAddHeader()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToAdd = "Key1";
            headerCollection.Add(keyToAdd, "Value1");

            byte[] valueToAdd = Encoding.UTF8.GetBytes("NewValue");

            // Act
            bool added = headerCollection.UnsafeTryAdd(keyToAdd, valueToAdd);

            // Assert
            Assert.IsFalse(added);
            Assert.AreEqual(1, headerCollection.Count); // Count should remain unchanged
            Assert.AreEqual("Value1", Encoding.ASCII.GetString(headerCollection[headerCollection.IndexOf(keyToAdd)].Value)); // Value should not change
        }

        [TestMethod]
        public void TryGetValueWithExistingKeyShouldReturnTrueAndValue()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToGet = "Key1";
            string valueToGet = "Value1";
            headerCollection.Add(keyToGet, valueToGet);

            // Act
            bool result = headerCollection.TryGetValue(keyToGet, out string? retrievedValue);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(valueToGet, retrievedValue);
        }

        [TestMethod]
        public void TryGetValueWithNonExistingKeyShouldReturnFalseAndNullValue()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToGet = "Key1";

            // Act
            bool result = headerCollection.TryGetValue(keyToGet, out string? retrievedValue);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(retrievedValue);
        }

        [TestMethod]
        public void TryGetValueByteWithExistingKeyShouldReturnTrueAndValue()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToGet = "Key1";
            byte[] valueToGet = Encoding.UTF8.GetBytes("Value1");
            headerCollection.UnsafeAdd(keyToGet, valueToGet);

            // Act
            bool result = headerCollection.TryGetValue(keyToGet, out byte[]? retrievedValue);

            // Assert
            Assert.IsTrue(result);
            CollectionAssert.AreEqual(valueToGet, retrievedValue);
        }

        [TestMethod]
        public void TryGetValueByteWithNonExistingKeyShouldReturnFalseAndNullValue()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToGet = "Key1";

            // Act
            bool result = headerCollection.TryGetValue(keyToGet, out byte[]? retrievedValue);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(retrievedValue);
        }

        [TestMethod]
        public void TryGetValuesWithExistingKeyShouldReturnTrueAndValues()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToGet = "Key1";
            List<string> valuesToGet = new() { "Value1", "Value2" };
            foreach (string value in valuesToGet)
            {
                headerCollection.Add(keyToGet, value);
            }

            // Act
            bool result = headerCollection.TryGetValues(keyToGet, out List<string>? retrievedValues);

            // Assert
            Assert.IsTrue(result);
            CollectionAssert.AreEqual(valuesToGet, retrievedValues);
        }

        [TestMethod]
        public void TryGetValuesWithNonExistingKeyShouldReturnFalseAndNullValues()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToGet = "Key1";

            // Act
            bool result = headerCollection.TryGetValues(keyToGet, out List<string>? retrievedValues);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(retrievedValues);
        }

        [TestMethod]
        public void TryGetValuesByteWithExistingKeyShouldReturnTrueAndValues()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToGet = "Key1";
            List<byte[]> valuesToGet = new()
            {
                Encoding.UTF8.GetBytes("Value1"),
                Encoding.UTF8.GetBytes("Value2")
            };

            foreach (byte[] value in valuesToGet)
            {
                headerCollection.UnsafeAdd(keyToGet, value);
            }

            // Act
            bool result = headerCollection.TryGetValues(keyToGet, out List<byte[]>? retrievedValues);

            // Assert
            Assert.IsTrue(result);
            CollectionAssert.AreEqual(valuesToGet, retrievedValues, new ByteArrayComparer());
        }

        [TestMethod]
        public void TryGetValuesByteWithNonExistingKeyShouldReturnFalseAndNullValues()
        {
            // Arrange
            HyperHeaderCollection headerCollection = new();
            string keyToGet = "Key1";

            // Act
            bool result = headerCollection.TryGetValues(keyToGet, out List<byte[]>? retrievedValues);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(retrievedValues);
        }

        // Custom comparer for comparing byte arrays
        private class ByteArrayComparer : IComparer
        {
            public int Compare(object? x, object? y)
            {
                if (x is byte[] arr1 && y is byte[] arr2)
                {
                    if (arr1.Length != arr2.Length)
                    {
                        return -1;
                    }

                    for (int i = 0; i < arr1.Length; i++)
                    {
                        if (arr1[i] != arr2[i])
                        {
                            return -1;
                        }
                    }

                    return 0;
                }

                if (x is null && y is null)
                {
                    return 0; // Both are null, consider them equal
                }

                return -1; // One of them is null, consider them not equal
            }
        }
    }
}
