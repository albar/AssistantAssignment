using System.Collections.Generic;

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
        
        {
        }
    }
}