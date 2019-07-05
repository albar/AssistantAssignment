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
            var count = Factorial(items.Length) / (Factorial(size) * Factorial(items.Length - size));
            for (var i = 0; i < count - 1; i++)
            {
                yield return state.Select(index => items[index]);
                state = UpgradeState(state.ToList(), items.Length - 1).ToArray();
            }
            yield return state.Select(index => items[index]);
        }

        private static int Factorial(int n)
        {
            if (n <= 1) return 1;
            return n * Factorial(n - 1);
        }

        private static List<int> UpgradeState(List<int> state, int max)
        {
            if (state[state.Count - 1] < max)
            {
                state[state.Count - 1]++;
                return state;
            }

            var upgraded = UpgradeState(state.Take(state.Count - 1).ToList(), max - 1);
            upgraded.Add(upgraded[upgraded.Count - 1] + 1);
            return upgraded;
        }
    }
}