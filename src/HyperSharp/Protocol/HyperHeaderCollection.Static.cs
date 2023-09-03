using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace HyperSharp.Protocol
{
    public sealed partial class HyperHeaderCollection : IEnumerable<KeyValuePair<string, byte[]>>
    {
        private static readonly StringPool _commonHeaderNames;
        static HyperHeaderCollection()
        {
            // HyperHeaderName is an enum source generated from the Microsoft.Net.Http.Headers.HeaderNames class.
            FieldInfo[] headerNames = typeof(HyperHeaderName).GetFields(BindingFlags.Public | BindingFlags.Static);

            _commonHeaderNames = new StringPool(headerNames.Length);
            foreach (FieldInfo headerName in headerNames)
            {
                _commonHeaderNames.Add(headerName.Name);
            }
        }

        // https://developers.cloudflare.com/rules/transform/request-header-modification/reference/header-format
        private static readonly char[] _validHeaderNameCharacters = new char[]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '_'
        };

        private static readonly char[] _validHeaderNameValues = new char[] {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '_',
            ' ', ':', ';', '.', ',', '\\', '/', '"', '\'', '!', '?', '(', ')', '{', '}', '[', ']', '@', '<', '>', '=', '+', '*', '#', '$', '&', '`', '|', '~', '^', '%'
        };

        /// <summary>
        /// Validates that the header name does not contain invalid characters.
        /// </summary>
        /// <param name="name">The header name to validate.</param>
        /// <returns><see langword="true"/> if the header name is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValidName(ReadOnlySpan<char> name)
        {
            if (name.IsEmpty)
            {
                return false;
            }

            ReadOnlySpan<char> validHeaderNameCharacters = _validHeaderNameCharacters.AsSpan();
            return name.IndexOfAnyExcept(validHeaderNameCharacters) < 0;
        }

        /// <summary>
        /// Validates that the header value does not contain invalid characters.
        /// </summary>
        /// <param name="name">The header value to validate.</param>
        /// <returns><see langword="true"/> if the header value is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValidName(ReadOnlySpan<byte> name)
        {
            if (name.IsEmpty)
            {
                return false;
            }

            ReadOnlySpan<byte> validHeaderNameCharacters = MemoryMarshal.AsBytes(_validHeaderNameCharacters.AsSpan());
            return name.IndexOfAnyExcept(validHeaderNameCharacters) < 0;
        }

        /// <summary>
        /// Validates that the header value does not contain invalid characters.
        /// </summary>
        /// <param name="value">The header value to validate.</param>
        /// <returns><see langword="true"/> if the header value is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValidValue(ReadOnlySpan<char> value)
        {
            if (value.IsEmpty)
            {
                return false;
            }

            ReadOnlySpan<char> validHeaderValueCharacters = _validHeaderNameValues.AsSpan();
            return value.IndexOfAnyExcept(validHeaderValueCharacters) < 0;
        }

        /// <summary>
        /// Validates that the header value does not contain invalid characters.
        /// </summary>
        /// <param name="value">The header value to validate.</param>
        /// <returns><see langword="true"/> if the header value is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValidValue(ReadOnlySpan<byte> value)
        {
            if (value.IsEmpty)
            {
                return false;
            }

            ReadOnlySpan<byte> validHeaderValueCharacters = MemoryMarshal.AsBytes(_validHeaderNameValues.AsSpan());
            return value.IndexOfAnyExcept(validHeaderValueCharacters) < 0;
        }

        /// <summary>
        /// Normalizes the header name from x-Header-name to X-Header-Name format.
        /// </summary>
        /// <param name="value">The header name to normalize.</param>
        /// <returns>The normalized header name.</returns>
        public static string NormalizeName(ReadOnlySpan<char> value)
        {
            if (value.IsEmpty)
            {
                return string.Empty;
            }

            Span<char> characters = value.ToArray();
            characters[0] = char.ToUpperInvariant(value[0]);
            for (int i = 1; i < value.Length; i++)
            {
                if (value[i] is '-' or '_')
                {
                    characters[i + 1] = char.ToUpperInvariant(value[i + 1]);
                }
            }

            return characters.ToString();
        }
    }
}
