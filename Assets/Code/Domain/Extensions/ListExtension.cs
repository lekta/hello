using System.Collections.Generic;
using JetBrains.Annotations;

namespace LH.Domain {
    public static class ListExtension {
        [ContractAnnotation("list:null => false")]
        public static bool IsFilled<T>(this List<T> list) => list != null && list.Count > 0;

        [ContractAnnotation("list:null => true")]
        public static bool IsEmpty<T>(this List<T> list) => list == null || list.Count == 0;

        [ContractAnnotation("list:null => false")]
        public static bool IsFilled<T>(this IReadOnlyList<T> list) => list != null && list.Count > 0;

        [ContractAnnotation("list:null => true")]
        public static bool IsEmpty<T>(this IReadOnlyList<T> list) => list == null || list.Count == 0;
    }
}