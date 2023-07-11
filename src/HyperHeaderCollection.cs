using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace OoLunar.HyperSharp
{
    public sealed class HyperHeaderCollection : IReadOnlyDictionary<string, IReadOnlyList<string>>
    {
        private readonly Dictionary<string, List<string>> Headers;
        public IEnumerable<string> Keys => Headers.Keys;
        public IEnumerable<IReadOnlyList<string>> Values => Headers.Values;
        public int Count => Headers.Count;

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
            IEnumerator<KeyValuePair<string, List<string>>> enumerator = Headers.GetEnumerator();
            return Unsafe.As<IEnumerator<KeyValuePair<string, List<string>>>, IEnumerator<KeyValuePair<string, IReadOnlyList<string>>>>(ref enumerator);
        }
        IEnumerator<KeyValuePair<string, IReadOnlyList<string>>> IEnumerable<KeyValuePair<string, IReadOnlyList<string>>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void AddHeaderValue(string name, string value)
        {
            ArgumentException.ThrowIfNullOrEmpty(value, nameof(value));
            if (!Headers.TryGetValue(name, out List<string>? values))
            {
                values = new();
                Headers.Add(name, values);
            }

            values.Add(value.Trim());
        }

        public void AddHeaderValues(string name, IEnumerable<string> values)
        {
            List<string> newValues = new();
            foreach (string value in values)
            {
                ArgumentException.ThrowIfNullOrEmpty(value, nameof(values));
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
    }
}
