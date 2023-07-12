using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OoLunar.HyperSharp
{
    public sealed class HyperHeaderCollection : IReadOnlyDictionary<string, IReadOnlyList<string>>
    {
        // https://developers.cloudflare.com/rules/transform/request-header-modification/reference/header-format
        private static readonly char[] ValidHeaderNameCharacters = new char[]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            '-', '_'
        };

        private static readonly char[] ValidHeaderValueCharacters = new char[]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            '-', '_', ' ', ':', ';', '.', ',', '\\', '/', '"', '\'', '!', '?', '(', ')', '{', '}', '[', ']', '@', '<', '>', '=', '+', '*', '#', '$', '&', '`', '|', '~', '^', '%'
        };

        public IEnumerable<string> Keys => Headers.Keys;
        public IEnumerable<IReadOnlyList<string>> Values => Headers.Values;
        public int Count => Headers.Count;

        private readonly Dictionary<string, List<string>> Headers;

        public HyperHeaderCollection() => Headers = new();
        public HyperHeaderCollection(IDictionary<string, List<string>> dictionary, IEqualityComparer<string>? comparer = null) => Headers = new Dictionary<string, List<string>>(dictionary, comparer);

        public IReadOnlyList<string> this[string key]
        {
            get => Headers[key];
            set => SetHeader(key, value);
        }

        public bool ContainsKey(string key) => Headers.ContainsKey(key);
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out IReadOnlyList<string> value)
        {
            value = Headers.TryGetValue(key, out List<string>? values)
                ? values.AsReadOnly()
                : null;

            return value is not null;
        }

        public IEnumerator<KeyValuePair<string, IReadOnlyList<string>>> GetEnumerator()
        {
            foreach (KeyValuePair<string, List<string>> header in Headers)
            {
                yield return new KeyValuePair<string, IReadOnlyList<string>>(header.Key, header.Value.AsReadOnly());
            }
        }
        IEnumerator<KeyValuePair<string, IReadOnlyList<string>>> IEnumerable<KeyValuePair<string, IReadOnlyList<string>>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void AddHeaderValue(string name, string value)
        {
            if (!IsValidHeaderName(name))
            {
                throw new ArgumentException("Header name is invalid.", nameof(name));
            }
            else if (!IsValidHeaderValue(value))
            {
                throw new ArgumentException("Header value is invalid.", nameof(value));
            }

            if (!Headers.TryGetValue(name, out List<string>? values))
            {
                values = new();
                Headers.Add(name, values);
            }

            values.Add(value.Trim());
        }

        public void AddHeaderValues(string name, IEnumerable<string> values)
        {
            if (!IsValidHeaderName(name))
            {
                throw new ArgumentException("Header name is invalid.", nameof(name));
            }

            List<string> newValues = new();
            foreach (string value in values)
            {
                if (!IsValidHeaderValue(value))
                {
                    throw new ArgumentException("Header value is invalid.", nameof(values));
                }

                newValues.Add(value.Trim());
            }

            if (!Headers.TryGetValue(name, out List<string>? oldValues))
            {
                Headers[name] = newValues;
                return;
            }

            oldValues.AddRange(newValues);
        }

        public void SetHeader(string name, IEnumerable<string> values) => Headers[name] = new List<string>(values);
        public void RemoveHeader(string name) => Headers.Remove(name);

        public static bool IsValidHeaderName(string value)
        {
            if (value is null)
            {
                return false;
            }

            Span<char> validNameSpan = ValidHeaderNameCharacters.AsSpan();
            foreach (char c in value)
            {
                if (validNameSpan.IndexOf(c) == -1)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidHeaderValue(string value)
        {
            if (value is null)
            {
                return false;
            }

            Span<char> validValueSpan = ValidHeaderValueCharacters.AsSpan();
            foreach (char c in value)
            {
                if (validValueSpan.IndexOf(c) == -1)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
