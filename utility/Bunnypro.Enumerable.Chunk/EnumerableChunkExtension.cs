using System.Collections.Generic;
using System.Linq;

namespace Bunnypro.Enumerable.Chunk
{
    public static class EnumerableChunkExtension
    {   
        public static IEnumerable<EnumerableChunk<T>> Chunk<T>(this IEnumerable<T> source, int size)
        {
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext()) yield return new EnumerableChunk<T>(enumerator, size);
            }
        }

        public static IEnumerable<T[]> ToInnerArray<T>(this IEnumerable<EnumerableChunk<T>> source)
        {
            return source.Select(c => c.ToArray());
        }

        public static IEnumerable<List<T>> ToInnerList<T>(this IEnumerable<EnumerableChunk<T>> source)
        {
            return source.Select(c => c.ToList());
        }
        
        public static T[][] ToAllArray<T>(this IEnumerable<EnumerableChunk<T>> source)
        {
            return source.ToInnerArray().ToArray();
        }

        public static List<List<T>> ToAllList<T>(this IEnumerable<EnumerableChunk<T>> source)
        {
            return source.ToInnerList().ToList();
        }

        public static IEnumerable<T>[] ToArray<T>(this IEnumerable<EnumerableChunk<T>> source)
        {
            return source.Select(c => c.AsEnumerable()).ToArray();
        }

        public static List<IEnumerable<T>> ToList<T>(this IEnumerable<EnumerableChunk<T>> source)
        {
            return source.Select(c => c.AsEnumerable()).ToList();
        }
    }
}