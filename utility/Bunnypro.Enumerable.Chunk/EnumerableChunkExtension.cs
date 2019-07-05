using System.Collections.Generic;

namespace Bunnypro.Enumerable.Chunk
{
    public static class EnumerableChunkExtension
    {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(
            this IEnumerable<T> source,
            int size)
        {
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext()) yield return Take(enumerator, size);
            }
        }
        
        private static IEnumerable<T> Take<T>(IEnumerator<T> enumerator, int size)
        {
            var i = 0;
            do
            {
                yield return enumerator.Current;
                i++;
            } while (i < size && enumerator.MoveNext());
        }
    }
}