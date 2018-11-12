using System.Collections.Generic;
using System.Collections.Immutable;
using Bunnypro.GeneticAlgorithm.Core.Populations;
using Bunnypro.GeneticAlgorithm.Core.Strategies;
using Bunnypro.GeneticAlgorithm.Standard;
using Moq;
using Moq.Protected;

namespace Bunnypro.GeneticAlgorithm.Core.Test
{
    public static class MockObject
    {
        public static Population Population<T>(HashSet<T> chromosomes) where T : IChromosome
        {
            var population = new Mock<Population> { CallBase = true };
            population.Protected().Setup<ImmutableHashSet<T>>("CreateInitialChromosomes").Returns(chromosomes.ToImmutableHashSet());

            population.Protected()
                .Setup<ImmutableHashSet<T>>("FilterOffspring", chromosomes)
                .Returns((IEnumerable<T> offspring) => offspring.ToImmutableHashSet());

            return population.Object;
        }

        public static IEvolutionStrategy EvolutionStrategy()
        {
            var evolutionStrategy = new Mock<IEvolutionStrategy>();
            evolutionStrategy.Setup(e => e.Prepare(It.IsAny<IEnumerable<IChromosome>>()));
            evolutionStrategy.Setup(e => e.GenerateOffspring(It.IsAny<IEnumerable<IChromosome>>())).Returns((IEnumerable<IChromosome> parent) => parent);
            return evolutionStrategy.Object;
        }
    }
}
