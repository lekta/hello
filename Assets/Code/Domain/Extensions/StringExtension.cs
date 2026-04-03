using System.Collections.Generic;

namespace LH.Domain {
    public static class StringExtension {
        public static string Join(this IEnumerable<string> strings, string separator) => string.Join(separator, strings);
    }
}