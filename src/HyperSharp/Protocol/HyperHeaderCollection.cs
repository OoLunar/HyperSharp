using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace HyperSharp.Protocol
{
    /// <summary>
    /// Represents a collection of headers with string keys and lists of string values.
    /// </summary>
    public sealed partial class HyperHeaderCollection : IList<KeyValuePair<string, byte[]>>
    {
        private readonly List<KeyValuePair<string, byte[]>> _headers;

        /// <summary>
        /// Initializes a new instance of the <see cref="HyperHeaderCollection"/> class that is empty and has the default initial capacity.
        /// </summary>
        public HyperHeaderCollection() => _headers = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="HyperHeaderCollection"/> class that is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        public HyperHeaderCollection(int capacity) => _headers = new(capacity);

        /// <summary>
        /// Initializes a new instance of the <see cref="HyperHeaderCollection"/> class that contains elements copied from the specified collection.
        /// </summary>
        /// <param name="headers">The collection whose elements are copied to the new list.</param>
        public HyperHeaderCollection(IEnumerable<KeyValuePair<string, byte[]>> headers) => _headers = new(headers);

        /// <inheritdoc />
        public KeyValuePair<string, byte[]> this[int index]
        {
            get => _headers[index];
            set => _headers[index] = value;
        }

        /// <inheritdoc />
        public int Count => _headers.Count;

        /// <inheritdoc />
        public bool IsReadOnly { get; init; }

        /// <inheritdoc />
        public void Add(KeyValuePair<string, byte[]> item) => Add(item.Key, Encoding.ASCII.GetString(item.Value));

        /// <summary>
        /// Adds the specified header and value to the collection.
        /// </summary>
        /// <param name="key">The name of the header to add.</param>
        /// <param name="value">The value of the header to add.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> or <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="key"/> or <paramref name="value"/> is invalid and against the HTTP specification.</exception>
        public void Add(string key, string value)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);
            if (!IsValidName(key))
            {
                throw new ArgumentException($"The header name is invalid: {key}", nameof(key));
            }
            else if (!IsValidValue(value))
            {
                throw new ArgumentException($"The header value is invalid: {value}", nameof(value));
            }

            key = _commonHeaderNames.GetOrAdd(NormalizeName(key));
            _headers.Add(new(key, Encoding.ASCII.GetBytes(value)));
        }

        /// <summary>
        /// Adds the specified header and value to the collection, skipping the validation of the header value.
        /// </summary>
        /// <param name="key">The name of the header to add.</param>
        /// <param name="value">The value of the header to add.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="key"/> is invalid and against the HTTP specification.</exception>
        public void UnsafeAdd(string key, byte[] value)
        {
            ArgumentNullException.ThrowIfNull(key);
            if (!IsValidName(key))
            {
                throw new ArgumentException($"The header name is invalid: {key}", nameof(key));
            }

            key = _commonHeaderNames.GetOrAdd(NormalizeName(key));
            _headers.Add(new(key, value));
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, byte[]> item) => _headers.Contains(item);

        /// <summary>
        /// Determines whether the collection contains the specified header.
        /// </summary>
        /// <param name="key">The name of the header to locate.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is <see langword="null"/>.</exception>
        /// <returns><see langword="true"/> if the collection contains the specified header; otherwise, <see langword="false"/>.</returns>
        public bool Contains(string key)
        {
            ArgumentNullException.ThrowIfNull(key);

            key = _commonHeaderNames.GetOrAdd(NormalizeName(key));
            foreach (KeyValuePair<string, byte[]> header in _headers)
            {
                if (ReferenceEquals(header.Key, key))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, byte[]>[] array, int arrayIndex) => _headers.CopyTo(array, arrayIndex);

        /// <summary>
        /// Gets the value of the first occurrence of the specified header in the collection.
        /// </summary>
        /// <param name="key">The name of the header to get.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="key"/> is invalid and against the HTTP specification.</exception>
        /// <exception cref="KeyNotFoundException">The header was not found.</exception>
        /// <returns>The value of the header.</returns>
        public string Get(string key)
        {
            ArgumentNullException.ThrowIfNull(key);
            if (!IsValidName(key))
            {
                throw new ArgumentException($"The header name is invalid: {key}", nameof(key));
            }

            key = _commonHeaderNames.GetOrAdd(NormalizeName(key));
            foreach (KeyValuePair<string, byte[]> header in _headers)
            {
                if (ReferenceEquals(header.Key, key))
                {
                    return Encoding.ASCII.GetString(header.Value);
                }
            }

            throw new KeyNotFoundException($"The header was not found: {key}");
        }

        /// <inheritdoc />
        public int IndexOf(KeyValuePair<string, byte[]> item) => _headers.IndexOf(item);

        /// <summary>
        /// Determines the index of the first occurrence of the specified header in the collection.
        /// </summary>
        /// <param name="key">The name of the header to locate.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is <see langword="null"/>.</exception>
        /// <returns>The index of the first occurrence of the specified header in the collection; otherwise, -1.</returns>
        public int IndexOf(string key)
        {
            ArgumentNullException.ThrowIfNull(key);

            key = _commonHeaderNames.GetOrAdd(NormalizeName(key));
            for (int i = 0; i < _headers.Count; i++)
            {
                if (ReferenceEquals(_headers[i].Key, key))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <inheritdoc />
        public void Insert(int index, KeyValuePair<string, byte[]> item) => Insert(index, item.Key, Encoding.ASCII.GetString(item.Value));

        /// <summary>
        /// Inserts the specified header and value into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the header should be inserted.</param>
        /// <param name="key">The name of the header to insert.</param>
        /// <param name="value">The value of the header to insert.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> or <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="key"/> or <paramref name="value"/> is invalid and against the HTTP specification.</exception>
        public void Insert(int index, string key, string value)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);
            if (!IsValidName(key))
            {
                throw new ArgumentException($"The header name is invalid: {key}", nameof(key));
            }
            else if (!IsValidValue(value))
            {
                throw new ArgumentException($"The header value is invalid: {value}", nameof(value));
            }

            key = _commonHeaderNames.GetOrAdd(NormalizeName(key));
            _headers.Insert(index, new(key, Encoding.ASCII.GetBytes(value)));
        }

        /// <summary>
        /// Inserts the specified header and value into the collection at the specified index, skipping the validation of the header value.
        /// </summary>
        /// <param name="index">The zero-based index at which the header should be inserted.</param>
        /// <param name="key">The name of the header to insert.</param>
        /// <param name="value">The value of the header to insert.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="key"/> is invalid and against the HTTP specification.</exception>
        public void UnsafeInsert(int index, string key, byte[] value)
        {
            ArgumentNullException.ThrowIfNull(key);
            if (!IsValidName(key))
            {
                throw new ArgumentException($"The header name is invalid: {key}", nameof(key));
            }

            key = _commonHeaderNames.GetOrAdd(NormalizeName(key));
            _headers.Insert(index, new(key, value));
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, byte[]> item) => _headers.Remove(item);

        /// <summary>
        /// Removes the first occurrence of the specified header from the collection.
        /// </summary>
        /// <param name="key">The name of the header to remove.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is <see langword="null"/>.</exception>
        /// <returns><see langword="true"/> if the header was removed; otherwise, <see langword="false"/>.</returns>
        public bool Remove(string key)
        {
            ArgumentNullException.ThrowIfNull(key);

            key = _commonHeaderNames.GetOrAdd(NormalizeName(key));
            for (int i = 0; i < _headers.Count; i++)
            {
                if (ReferenceEquals(_headers[i].Key, key))
                {
                    _headers.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes the first <paramref name="n"/> occurrences of the specified header from the collection.
        /// </summary>
        /// <param name="key">The name of the header to remove.</param>
        /// <param name="n">The number of headers to remove.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is <see langword="null"/>.</exception>
        /// <returns>The number of headers removed.</returns>
        public int RemoveN(string key, int n)
        {
            ArgumentNullException.ThrowIfNull(key);
            if (n == 0)
            {
                return 0;
            }

            key = _commonHeaderNames.GetOrAdd(NormalizeName(key));
            int removed = 0;
            for (int i = 0; i < _headers.Count; i++)
            {
                if (ReferenceEquals(_headers[i].Key, key))
                {
                    _headers.RemoveAt(i);
                    i--; // We removed an item, so we need to go back one.
                    removed++;
                    if (removed == n)
                    {
                        return removed;
                    }
                }
            }

            return removed;
        }

        /// <summary>
        /// Removes all occurrences of the specified header from the collection.
        /// </summary>
        /// <param name="key">The name of the header to remove.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is <see langword="null"/>.</exception>
        /// <returns>The number of headers removed.</returns>
        public int RemoveAll(string key)
        {
            ArgumentNullException.ThrowIfNull(key);

            key = _commonHeaderNames.GetOrAdd(NormalizeName(key));
            int count = 0;
            for (int i = 0; i < _headers.Count; i++)
            {
                if (ReferenceEquals(_headers[i].Key, key))
                {
                    _headers.RemoveAt(i);
                    i--; // We removed an item, so we need to go back one.
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Removes all pre-existing headers with the specified name and adds the specified header and value to the collection.
        /// </summary>
        /// <param name="key">The name of the header to add or replace.</param>
        /// <param name="value">The value of the header to add or replace.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> or <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="key"/> or <paramref name="value"/> is invalid and against the HTTP specification.</exception>
        public void Set(string key, string value)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);
            if (!IsValidName(key))
            {
                throw new ArgumentException($"The header name is invalid: {key}", nameof(key));
            }
            else if (!IsValidValue(value))
            {
                throw new ArgumentException($"The header value is invalid: {value}", nameof(value));
            }

            key = _commonHeaderNames.GetOrAdd(NormalizeName(key));
            _headers.RemoveAll(x => ReferenceEquals(x.Key, key));
            _headers.Add(new(key, Encoding.ASCII.GetBytes(value)));
        }

        /// <summary>
        /// Removes all pre-existing headers with the specified name and adds the specified header and value to the collection, skipping the validation of the header value.
        /// </summary>
        /// <param name="key">The name of the header to add or replace.</param>
        /// <param name="value">The value of the header to add or replace.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="key"/> is invalid and against the HTTP specification.</exception>
        public void UnsafeSet(string key, byte[] value)
        {
            ArgumentNullException.ThrowIfNull(key);
            if (!IsValidName(key))
            {
                throw new ArgumentException($"The header name is invalid: {key}", nameof(key));
            }

            key = _commonHeaderNames.GetOrAdd(NormalizeName(key));
            _headers.RemoveAll(x => ReferenceEquals(x.Key, key));
            _headers.Add(new(key, value));
        }

        /// <summary>
        /// Adds the specified header and value to the collection if the header does not already exist.
        /// </summary>
        /// <param name="key">The name of the header to add.</param>
        /// <param name="value">The value of the header to add.</param>
        /// <returns><see langword="true"/> if the header was added; otherwise, <see langword="false"/>. Additionally returns <see langword="false"/> if the <paramref name="key"/> or <paramref name="value"/> is either <see langword="null"/> or invalid and against the HTTP specification.</returns>
        public bool TryAdd(string key, string value)
        {
            if (key is null || !IsValidName(key) || value is null || !IsValidValue(value))
            {
                return false;
            }

            key = _commonHeaderNames.GetOrAdd(NormalizeName(key));
            if (Contains(key))
            {
                return false;
            }

            _headers.Add(new(key, Encoding.ASCII.GetBytes(value)));
            return true;
        }

        /// <summary>
        /// Adds the specified header and value to the collection if the header does not already exist, skipping the validation of the header value.
        /// </summary>
        /// <param name="key">The name of the header to add.</param>
        /// <param name="value">The value of the header to add.</param>
        /// <returns><see langword="true"/> if the header was added; otherwise, <see langword="false"/>. Additionally returns <see langword="false"/> if the <paramref name="key"/> is either <see langword="null"/> or invalid and against the HTTP specification.</returns>
        public bool UnsafeTryAdd(string key, byte[] value)
        {
            if (key is null || !IsValidName(key))
            {
                return false;
            }

            key = _commonHeaderNames.GetOrAdd(NormalizeName(key));
            if (Contains(key))
            {
                return false;
            }

            _headers.Add(new(key, value));
            return true;
        }

        /// <summary>
        /// Attempts to get the value of the specified header.
        /// </summary>
        /// <param name="key">The name of the header to get.</param>
        /// <param name="value">The value of the header if it exists; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the header exists; otherwise, <see langword="false"/>. Additionally returns <see langword="false"/> if the <paramref name="key"/> is either <see langword="null"/> or invalid and against the HTTP specification.</returns>
        public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
        {
            if (key is null || !IsValidName(key))
            {
                value = null;
                return false;
            }

            key = _commonHeaderNames.GetOrAdd(NormalizeName(key));
            foreach (KeyValuePair<string, byte[]> header in _headers)
            {
                if (ReferenceEquals(header.Key, key))
                {
                    value = Encoding.ASCII.GetString(header.Value);
                    return true;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Attempts to get the raw value of the specified header.
        /// </summary>
        /// <param name="key">The name of the header to get.</param>
        /// <param name="value">The value of the header if it exists; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the header exists; otherwise, <see langword="false"/>. Additionally returns <see langword="false"/> if the <paramref name="key"/> is either <see langword="null"/> or invalid and against the HTTP specification.</returns>
        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value)
        {
            if (key is null || !IsValidName(key))
            {
                value = null;
                return false;
            }

            key = _commonHeaderNames.GetOrAdd(NormalizeName(key));
            foreach (KeyValuePair<string, byte[]> header in _headers)
            {
                if (ReferenceEquals(header.Key, key))
                {
                    value = header.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Attempts to get the values of the specified header.
        /// </summary>
        /// <param name="key">The name of the header to get.</param>
        /// <param name="values">The values of the header if it exists; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the header exists; otherwise, <see langword="false"/>. Additionally returns <see langword="false"/> if the <paramref name="key"/> is either <see langword="null"/> or invalid and against the HTTP specification.</returns>
        public bool TryGetValues(string key, [NotNullWhen(true)] out List<string>? values)
        {
            if (key is null || !IsValidName(key))
            {
                values = null;
                return false;
            }

            values = null;
            key = _commonHeaderNames.GetOrAdd(NormalizeName(key));
            foreach (KeyValuePair<string, byte[]> header in _headers)
            {
                if (ReferenceEquals(header.Key, key))
                {
                    values ??= new();
                    values.Add(Encoding.ASCII.GetString(header.Value));
                }
            }

            return values is not null;
        }

        /// <summary>
        /// Attempts to get the raw values of the specified header.
        /// </summary>
        /// <param name="key">The name of the header to get.</param>
        /// <param name="values">The values of the header if it exists; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the header exists; otherwise, <see langword="false"/>. Additionally returns <see langword="false"/> if the <paramref name="key"/> is either <see langword="null"/> or invalid and against the HTTP specification.</returns>
        public bool TryGetValues(string key, [NotNullWhen(true)] out List<byte[]>? values)
        {
            if (key is null || !IsValidName(key))
            {
                values = null;
                return false;
            }

            key = _commonHeaderNames.GetOrAdd(NormalizeName(key));
            values = null;
            foreach (KeyValuePair<string, byte[]> header in _headers)
            {
                if (ReferenceEquals(header.Key, key))
                {
                    values ??= new();
                    values.Add(header.Value);
                }
            }

            return values is not null;
        }

        /// <inheritdoc />
        public void Clear() => _headers.Clear();

        /// <inheritdoc />
        public void RemoveAt(int index) => _headers.RemoveAt(index);

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, byte[]>> GetEnumerator() => _headers.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _headers.GetEnumerator();
    }
}
