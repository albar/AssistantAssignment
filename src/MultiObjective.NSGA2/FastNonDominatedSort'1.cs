using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions;
using Bunnypro.GeneticAlgorithm.MultiObjective.Primitives;

namespace Bunnypro.GeneticAlgorithm.MultiObjective.NSGA2
{
    public class FastNonDominatedSort<T> : IParetoSort<T> where T : Enum
    {
        private readonly IComparer<IReadOnlyDictionary<T, double>> _comparer;

        public FastNonDominatedSort(IReadOnlyDictionary<T, OptimumValue> optimum)
        {
            _comparer = new NonDominatedComparer<T, double>(optimum);
        }

        public IEnumerable<ImmutableArray<IChromosome<T>>> Sort(IEnumerable<IChromosome<T>> chromosomes)
        {
            var chromosomeArray = chromosomes.ToArray();
            var dominationStates = chromosomeArray.ToDictionary(c => c, _ => new DominationData());
            var frontBuilder = ImmutableArray.CreateBuilder<IChromosome<T>>();
            for (var i = 0; i < chromosomeArray.Length; i++)
            {
                for (var j = i + 1; j < chromosomeArray.Length; j++)
                {
                    var sign = _comparer.Compare(chromosomeArray[i].ObjectiveValues, chromosomeArray[j].ObjectiveValues);
                    if (sign > 0)
                        Dominate(chromosomeArray[i], chromosomeArray[j]);
                    if (sign < 0)
                        Dominate(chromosomeArray[j], chromosomeArray[i]);
                }
                if (dominationStates[chromosomeArray[i]].DominatedCount == 0)
                    frontBuilder.Add(chromosomeArray[i]);
            }

            while (frontBuilder.Count > 0)
            {
                var previousFront = frontBuilder.ToImmutable();
                yield return previousFront;
                frontBuilder.Clear();
                foreach (var dominatingChromosome in previousFront)
                {
                    foreach (var dominatedChromosome in dominationStates[dominatingChromosome].DominatedChromosomes)
                    {
                        var count = dominationStates[dominatedChromosome].DominatedCount - 1;
                        if (count == 0) frontBuilder.Add(dominatedChromosome);
                        dominationStates[dominatedChromosome].DominatedCount = count;
                    }
                }
            }

            void Dominate(IChromosome<T> dominating, IChromosome<T> dominated)
            {
                dominationStates[dominating].DominatedChromosomes.Add(dominated);
                dominationStates[dominated].DominatedCount++;
            }
        }
        
        private class DominationData
        {
            public IList<IChromosome<T>> DominatedChromosomes { get; } = new List<IChromosome<T>>();
            public int DominatedCount { get; set; }
        }
    }
}