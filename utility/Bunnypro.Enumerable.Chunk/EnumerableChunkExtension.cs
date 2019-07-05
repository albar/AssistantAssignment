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
        
        public static T[][] ToArray<T>(this IEnumerable<EnumerableChunk<T>> source)
        {
            return source.Select(c => c.ToArray()).ToArray();
        }

        public static List<List<T>> ToList<T>(this IEnumerable<EnumerableChunk<T>> source)
        {
            return source.Select(c => c.ToList()).ToList();
        }
    }
}