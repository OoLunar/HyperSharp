using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace OoLunar.HyperSharp.Parsing
{
    public sealed class HyperHeaderCollection : IReadOnlyDictionary<string, IReadOnlyList<string>>
    {
        // https://developers.cloudflare.com/rules/transform/request-header-modification/reference/header-format
        private static readonly char[] _validHeaderNameCharacters = new char[]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            '-', '_'
        };

        private static readonly char[] _validHeaderValueCharacters = new char[]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            '-', '_', ' ', ':', ';', '.', ',', '\\', '/', '"', '\'', '!', '?', '(', ')', '{', '}', '[', ']', '@', '<', '>', '=', '+', '*', '#', '$', '&', '`', '|', '~', '^', '%'
        };

        private readonly Dictionary<string, List<byte[]>> _headers = new();

        public int Count => _headers.Count;
        public IEnumerable<string> Keys => _headers.Keys;
        public IEnumerable<IReadOnlyList<string>> Values
        {
            get
            {
                foreach (List<byte[]> values in _headers.Values)
                {
                    yield return values.Select(Encoding.ASCII.GetString).ToList();
                }
            }
        }

        public bool ContainsKey(string key) => _headers.ContainsKey(key);
        public IReadOnlyList<string> this[string key] => _headers[key].Select(Encoding.ASCII.GetString).ToList();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<KeyValuePair<string, IReadOnlyList<string>>> GetEnumerator()
        {
            foreach (KeyValuePair<string, List<byte[]>> header in _headers)
            {
                yield return new KeyValuePair<string, IReadOnlyList<string>>(header.Key, header.Value.Select(Encoding.ASCII.GetString).ToList());
            }
        }

        public void Add(string name, string value)
        {
            ValidateArgumentParameters(ref name, value);
            if (_headers.TryGetValue(name, out List<byte[]>? values))
            {
                values.Add(Encoding.ASCII.GetBytes(value));
            }
            else
            {
                _headers.Add(name, new List<byte[]> { Encoding.ASCII.GetBytes(value) });
            }
        }

        public void Add(string name, IEnumerable<string> values)
        {
            ValidateArgumentParameters(ref name, values);
            if (!_headers.TryGetValue(name, out List<byte[]>? existingValues))
            {
                existingValues = new List<byte[]>();
                _headers.Add(name, existingValues);
            }

            foreach (string value in values)
            {
                existingValues.Add(Encoding.ASCII.GetBytes(value));
            }
        }

        public bool TryAdd(string name, string value)
        {
            ValidateArgumentParameters(ref name, value);
            if (_headers.ContainsKey(name))
            {
                return false;
            }

            _headers.Add(name, new List<byte[]> { Encoding.ASCII.GetBytes(value) });
            return true;
        }

        public bool TryAdd(string name, IEnumerable<string> values)
        {
            ValidateArgumentParameters(ref name, values);
            if (_headers.ContainsKey(name))
            {
                return false;
            }

            List<byte[]> existingValues = new();
            foreach (string value in values)
            {
                existingValues.Add(Encoding.ASCII.GetBytes(value));
            }

            _headers.Add(name, existingValues);
            return true;
        }

        public void Set(string name, string value)
        {
            ValidateArgumentParameters(ref name, value);
            if (!_headers.TryGetValue(name, out List<byte[]>? values))
            {
                _headers.Add(name, new List<byte[]> { Encoding.ASCII.GetBytes(value) });
                return;
            }

            values.Clear();
            values.Add(Encoding.ASCII.GetBytes(value));
        }

        public void Set(string name, IEnumerable<string> values)
        {
            ValidateArgumentParameters(ref name, values);
            if (!_headers.TryGetValue(name, out List<byte[]>? existingValues))
            {
                existingValues = new List<byte[]>();
                _headers.Add(name, existingValues);
                return;
            }

            existingValues.Clear();
            foreach (string value in values)
            {
                existingValues.Add(Encoding.ASCII.GetBytes(value));
            }
        }

        public bool Remove(string name)
        {
            ValidateArgumentParameters(ref name, string.Empty);
            return _headers.Remove(name);
        }

        public bool TryGetValue(string name, [MaybeNullWhen(false)] out string value)
        {
            ValidateArgumentParameters(ref name, string.Empty);
            if (!_headers.TryGetValue(name, out List<byte[]>? existingValues))
            {
                value = null;
                return false;
            }

            value = Encoding.ASCII.GetString(existingValues[0]);
            return true;
        }

        public bool TryGetValue(string name, [MaybeNullWhen(false)] out IReadOnlyList<string> values)
        {
            ValidateArgumentParameters(ref name, string.Empty);
            if (!_headers.TryGetValue(name, out List<byte[]>? existingValues))
            {
                values = null;
                return false;
            }

            values = existingValues.Select(Encoding.ASCII.GetString).ToList();
            return true;
        }

        public bool TryGetValue(string name, [MaybeNullWhen(false)] out byte[] value)
        {
            ValidateArgumentParameters(ref name, string.Empty);
            if (!_headers.TryGetValue(name, out List<byte[]>? existingValues))
            {
                value = null;
                return false;
            }

            value = existingValues[0];
            return true;
        }

        public bool TryGetValue(string name, [MaybeNullWhen(false)] out IReadOnlyList<byte[]> values)
        {
            ValidateArgumentParameters(ref name, string.Empty);
            if (!_headers.TryGetValue(name, out List<byte[]>? existingValues))
            {
                values = null;
                return false;
            }

            values = existingValues;
            return true;
        }

        private static void ValidateArgumentParameters(ref string name, string value)
        {
            ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            if (!IsValidName(name))
            {
                throw new ArgumentException($"Invalid header name: {name}", nameof(name));
            }

            if (!IsValidValue(value))
            {
                throw new ArgumentException($"Invalid header value: {value}", nameof(value));
            }

            name = NormalizeHeaderName(name);
        }

        private static void ValidateArgumentParameters(ref string name, IEnumerable<string> value)
        {
            ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            if (!IsValidName(name))
            {
                throw new ArgumentException($"Invalid header name: {name}", nameof(name));
            }

            foreach (string item in value)
            {
                if (!IsValidValue(item))
                {
                    throw new ArgumentException($"Invalid header value: {item}", nameof(value));
                }
            }

            name = NormalizeHeaderName(name);
        }

        public static bool IsValidName(string value)
        {
            Span<char> validNameSpan = _validHeaderNameCharacters.AsSpan();
            foreach (char character in value)
            {
                if (validNameSpan.IndexOf(character) == -1)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidValue(ReadOnlySpan<char> value)
        {
            Span<char> validValueSpan = _validHeaderValueCharacters.AsSpan();
            foreach (char character in value)
            {
                if (validValueSpan.IndexOf(character) == -1)
                {
                    return false;
                }
            }

            return true;
        }

        public static string NormalizeHeaderName(ReadOnlySpan<char> value)
        {
            if (value.IsEmpty)
            {
                return string.Empty;
            }

            Span<char> characters = value.ToArray();
            characters[0] = char.ToUpperInvariant(value[0]);
            for (int i = 1; i < value.Length; i++)
            {
                if (value[i] == '-' || value[i] == '_')
                {
                    characters[i + 1] = char.ToUpperInvariant(value[i + 1]);
                }
            }

            return characters.ToString();
        }
    }
}
