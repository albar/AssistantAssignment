using System;
using System.Collections.Generic;
using System.Linq;

namespace Bunnypro.Enumerable.Combine
{
    public static class EnumerableCombineExtension
    {
        public static IEnumerable<IEnumerable<T>> Combine<T>(this IEnumerable<T> source, int size)
        {
            var state = System.Linq.Enumerable.Range(0, size).Select(i => i).ToArray();
            var items = source.ToArray();
            if (items.Length < size) throw new Exception("Items length is less than combination size");
            var count = Combination(items.Length, size);
            for (var i = 0; i < count - 1; i++)
            {
                yield return state.Select(index => items[index]).ToArray();
                state = UpgradeState(state, items.Length - 1);
            }
            yield return state.Select(index => items[index]).ToArray();
        }

        private static int Combination(int of, int by)
        {
            if (by > of) return 1;
            var diff = of - by;
            var max = Math.Max(by, diff);
            var min = Math.Min(by, diff);
            var result = 1;
            for (var i = of; i > max; i--)
            {
                result *= i;
            }

            for (var i = min; i > 1; i--)
            {
                result /= i;
            }

            return result;
        }

        private static int[] UpgradeState(int[] state, int max, int pos = 1)
        {
            var realPos = state.Length - pos;
            if (state[realPos] < max)
            {
                state[realPos]++;
                return state;
            }

            var upgraded = UpgradeState(state, max - 1, pos + 1);
            upgraded[realPos] = upgraded[realPos - 1] + 1;
            return upgraded;
        }
    }
}