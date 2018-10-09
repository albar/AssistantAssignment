using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bunnypro.GeneticAlgorithm.Standard;

namespace Bunnypro.GeneticAlgorithm.Core.Populations
{
    public class FixedSizePopulation<T> : Population<T> where T : IChromosome
    {
        public FixedSizePopulation(int size, IChromosomeFactory<T> chromosomeFactory)
        {
            Size = size;
            ChromosomeFactory = chromosomeFactory;
        }

        public int Size { get; }

        protected IChromosomeFactory<T> ChromosomeFactory { get; }

        protected override ImmutableHashSet<T> CreateInitialChromosomes()
        {
            return ChromosomeFactory.Create(Size).ToImmutableHashSet();
        }

        protected override ImmutableHashSet<T> FilterOffspring(IEnumerable<T> offspring)
        {
            var uniqueOffspring = new HashSet<T>(offspring.ToArray());

            if (uniqueOffspring.Count >= Size) return uniqueOffspring.Take(Size).ToImmutableHashSet();

            foreach (var parent in Chromosomes)
                if (uniqueOffspring.Add(parent) && uniqueOffspring.Count == Size)
                    break;

            if (uniqueOffspring.Count == Size) return uniqueOffspring.ToImmutableHashSet();

            throw new Exception($"Population Size is not Reached. Expected {Size}, Actual {uniqueOffspring.Count}");
        }
    }
}