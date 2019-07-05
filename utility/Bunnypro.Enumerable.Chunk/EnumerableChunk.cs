using System.Collections;
using System.Collections.Generic;

namespace Bunnypro.Enumerable.Chunk
{
    public class EnumerableChunk<T> : IEnumerable<T>
    {
        private readonly IEnumerator<T> _enumerator;
        private readonly int _size;

        public EnumerableChunk(IEnumerator<T> enumerator, int size)
        {
            _enumerator = enumerator;
            _size = size;
        }

        private IEnumerable<T> AsEnumerable()
        {
            var i = 0;
            do
            {
                yield return _enumerator.Current;
                i++;
            } while (i < _size && _enumerator.MoveNext());
        }

        public IEnumerator<T> GetEnumerator() => AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}