using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace LH.Domain {
    public static class EnumerableExtension {
        [ContractAnnotation("list:null => false")]
        public static bool IsFilled<T>(this List<T> list) => list != null && list.Count > 0;

        [ContractAnnotation("list:null => true")]
        public static bool IsEmpty<T>(this List<T> list) => list == null || list.Count == 0;

        [ContractAnnotation("list:null => false")]
        public static bool IsFilled<T>(this IReadOnlyList<T> list) => list != null && list.Count > 0;

        [ContractAnnotation("list:null => true")]
        public static bool IsEmpty<T>(this IReadOnlyList<T> list) => list == null || list.Count == 0;


        public static T GetAtOrDefault<T>(this IReadOnlyList<T> list, int index) {
            return list == null || index >= list.Count || index < 0 ? default : list[index];
        }
        
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> enumeration) => enumeration
            .Where(i => i is UnityEngine.Object obj ? obj != null : i != null);
    }
}