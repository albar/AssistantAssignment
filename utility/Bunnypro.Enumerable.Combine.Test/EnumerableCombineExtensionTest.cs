using System;
using System.Linq;
using Xunit;

namespace Bunnypro.Enumerable.Combine.Test
{
    public class EnumerableCombineExtensionTest
    {
        [Fact]
        public void Test1()
        {
            var values = new[] {1, 2, 3, 4, 5};
            const int combination = 3;
            var actual = values.Combine(combination).Select(c => c.ToArray()).ToArray();
            var expected = new[]
            {
                new[] {1, 2, 3},
                new[] {1, 2, 4},
                new[] {1, 2, 5},
                new[] {1, 3, 4},
                new[] {1, 3, 5},
                new[] {1, 4, 5},
                new[] {2, 3, 4},
                new[] {2, 3, 5},
                new[] {2, 4, 5},
                new[] {3, 4, 5}
            };
            Assert.Equal(expected, actual);
        }
    }
}
