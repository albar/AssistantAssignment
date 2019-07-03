using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions;
using Bunnypro.GeneticAlgorithm.MultiObjective.Core;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Bunnypro.GeneticAlgorithm.MultiObjective.NSGA2
{
    public class OffspringSelection<T> : IDistinctMultiObjectiveGeneticOperation<T> where T : Enum
    {
        private readonly IParetoSort<T> _sorter = new FastNonDominatedSort<T>();

        public Task<ImmutableHashSet<IChromosome<T>>> Operate(IEnumerable<IChromosome<T>> parents,
            PopulationCapacity capacity, CancellationToken token = default)
        {
            //    TODO: Take Front by Front
            var elite = ImmutableHashSet.CreateBuilder<IChromosome<T>>();
            ImmutableArray<IChromosome<T>> lastFront;
            using (var enumerator = _sorter.Sort(parents).GetEnumerator())
            {
                lastFront = enumerator.Current;
                while (elite.Count < capacity.Minimum && elite.Count + lastFront.Length <= capacity.Maximum)
                {
                    elite.UnionWith(lastFront);
                    if (!enumerator.MoveNext()) break;
                    lastFront = enumerator.Current;
                }
            }

            if (elite.Count >= capacity.Minimum) return Task.FromResult(elite.ToImmutable());

            token.ThrowIfCancellationRequested();

            //    TODO: Use Crowding Distance Diversity Selection to The Last Over-Capacity Front
            var remains = capacity.Minimum - elite.Count;
            var crowdingDistance = lastFront.ToDictionary(c => c, c => .0);

            void AddDistance(IChromosome<T> chromosome, double value)
            {
                crowdingDistance[chromosome] =
                    value == double.MaxValue || double.MaxValue - crowdingDistance[chromosome] < value
                        ? double.MaxValue
                        : crowdingDistance[chromosome] + value;
            }

            foreach (var objective in Enum.GetValues(typeof(T)).Cast<T>())
            {
                var sorted = lastFront.OrderBy(c => c.ObjectiveValues[objective]).ToList();
                AddDistance(sorted[0], double.MaxValue);
                AddDistance(sorted[sorted.Count - 1], double.MaxValue);
                for (var i = 1; i < sorted.Count - 1; i++)
                {
                    var distance = Math.Abs(sorted[i - 1].ObjectiveValues[objective] -
                                            sorted[i + 1].ObjectiveValues[objective]);
                    AddDistance(sorted[i], distance);
                }
            }

            elite.UnionWith(crowdingDistance.OrderByDescending(d => d.Value).Select(d => d.Key).Take(remains));
            return Task.FromResult(elite.ToImmutable());
        }
    }
}