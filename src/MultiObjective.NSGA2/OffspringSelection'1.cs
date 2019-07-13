using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Bunnypro.GeneticAlgorithm.MultiObjective.NSGA2
{
    public class OffspringSelection<T> : IDistinctMultiObjectiveGeneticOperation<T> where T : Enum
    {
        private readonly IParetoSort<T> _sorter;

        public OffspringSelection(IReadOnlyDictionary<T, double> coefficients)
        {
            _sorter = new FastNonDominatedSort<T>(coefficients);
        }

        public Task<ImmutableHashSet<IChromosome<T>>> Operate(IEnumerable<IChromosome<T>> parents,
            PopulationCapacity capacity, CancellationToken token = default)
        {
            var elite = ImmutableHashSet.CreateBuilder<IChromosome<T>>();
            ImmutableArray<IChromosome<T>> lastFront;
            var fronts = _sorter.Sort(parents).Select(f => f.ToArray()).ToArray();
            foreach (var front in fronts)
            {
                if (elite.Count + front.Length > capacity.Maximum)
                {
                    lastFront = front.ToImmutableArray();
                    break;
                }
                elite.UnionWith(front);
                if (elite.Count > capacity.Minimum) break;
            }

            if (elite.Count >= capacity.Minimum) return Task.FromResult(elite.ToImmutable());

            token.ThrowIfCancellationRequested();

            var remains = capacity.Minimum - elite.Count;
            var crowdingDistance = lastFront.ToDictionary(c => c, c => 0d);

            foreach (var objective in Enum.GetValues(typeof(T)).Cast<T>())
            {
                var sorted = lastFront.OrderBy(c => c.ObjectiveValues[objective]).ToArray();
                for (var i = 0; i < sorted.Length; i++)
                {
                    var lowerValue = i == 0 ? 0 : sorted[i - 1].ObjectiveValues[objective];
                    var upperValue = i == sorted.Length - 1 ? 0 : sorted[i + 1].ObjectiveValues[objective];
                    var distance = Math.Abs(upperValue - lowerValue);
                    crowdingDistance[sorted[i]] += distance;
                }
            }

            elite.UnionWith(crowdingDistance.OrderByDescending(d => d.Value).Select(d => d.Key).Take(remains));
            return Task.FromResult(elite.ToImmutable());
        }
    }
}