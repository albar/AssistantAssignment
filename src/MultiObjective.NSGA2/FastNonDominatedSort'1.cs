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
        public IEnumerable<ImmutableArray<IChromosome<T>>> Sort(IEnumerable<IChromosome<T>> chromosomes)
        {
            var dominationStates = new Dictionary<IChromosome<T>, DominationData>();
            var frontBuilder = ImmutableArray.CreateBuilder<IChromosome<T>>();
            var chromosomeList = chromosomes.ToList();
            for (var i = 0; i < chromosomeList.Count; i++)
            {
                RegisterChromosome(chromosomeList[i]);
                for (var j = i + 1; j < chromosomeList.Count; j++)
                {
                    RegisterChromosome(chromosomeList[j]);
                    var sign = Compare(chromosomeList[i].ObjectiveValues, chromosomeList[j].ObjectiveValues);
                    if (sign > 0)
                        Dominate(chromosomeList[i], chromosomeList[j]);
                    if (sign < 0)
                        Dominate(chromosomeList[j], chromosomeList[i]);
                }
                if (dominationStates[chromosomeList[i]].DominatedCount == 0)
                    frontBuilder.Add(chromosomeList[i]);
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
                        var count = dominationStates[dominatedChromosome].DominatedCount--;
                        if (count == 0) frontBuilder.Add(dominatedChromosome);
                    }
                }
            }
            
            void RegisterChromosome(IChromosome<T> chromosome)
            {
                if (dominationStates.ContainsKey(chromosome)) return;
                dominationStates.Add(chromosome, new DominationData());
            }

            void Dominate(IChromosome<T> dominating, IChromosome<T> dominated)
            {
                dominationStates[dominating].DominatedChromosomes.Add(dominated);
                dominationStates[dominated].DominatedCount++;
            }
        }

        private static int Compare(ObjectiveValues<T> values, ObjectiveValues<T> otherValues)
        {
            var domination = 0;
            foreach (var objective in Enum.GetValues(typeof(T)).Cast<T>())
            {
                var sign = Math.Sign(values[objective].CompareTo(otherValues[objective]));
                if (sign == 0 || domination == sign)
                    continue;
                if (domination != 0)
                    return 0;
                domination = sign;
            }
            return domination;
        }
        
        private class DominationData
        {
            public IList<IChromosome<T>> DominatedChromosomes { get; } = new List<IChromosome<T>>();
            public int DominatedCount { get; set; }
        }
    }
}