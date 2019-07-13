using System;
using System.Collections.Generic;
using System.Linq;

namespace Bunnypro.Enumerable.Utility
{
    public static class EnumerableUtilityExtension
    {
        public static IEnumerable<T[]> ToInnerArray<T>(this IEnumerable<IEnumerable<T>> source)
        {
            return source.Select(c => c.ToArray());
        }

        public static IEnumerable<List<T>> ToInnerList<T>(this IEnumerable<IEnumerable<T>> source)
        {
            return source.Select(c => c.ToList());
        }
        
        public static T[][] ToAllArray<T>(this IEnumerable<IEnumerable<T>> source)
        {
            return source.ToInnerArray().ToArray();
        }

        public static List<List<T>> ToAllList<T>(this IEnumerable<IEnumerable<T>> source)
        {
            return source.ToInnerList().ToList();
        }
    }
}
