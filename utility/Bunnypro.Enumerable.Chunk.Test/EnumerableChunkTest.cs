using Xunit;

namespace Bunnypro.Enumerable.Chunk.Test
{
    public class EnumerableChunkTest
    {
        [Fact]
        public void Test1()
        {
            var items = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9};
            const int size = 2;
            
            var expected = new[]
            {
                new[] {1, 2},
                new[] {3, 4},
                new[] {5, 6},
                new[] {7, 8},
                new[] {9}
            };

            var actual = items.Chunk(size).ToAllArray();
            Assert.Equal(expected, actual);
        }
    }
}
