using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EvolutionaryAlgorithm.Abstraction;
using Thesis.Algorithm.ObjectiveValueCalculators;

namespace Thesis.Algorithm
{
    public class ChromosomeEvaluator : IEvaluator<Chromosome>
    {
        private readonly PhenotypeResolver _resolver;
        private readonly ImmutableHashSet<ObjectiveValueCalculatorBase> _calculators;

        public ChromosomeEvaluator(PhenotypeResolver resolver,
            ImmutableHashSet<ObjectiveValueCalculatorBase> calculators)
        {
            _resolver = resolver;
            _calculators = calculators;
        }

        public async Task EvaluateAsync(IEnumerable<Chromosome> chromosomes, CancellationToken token)
        {
            await ResolvePhenotypeAsync(chromosomes, token);
            await CalculateFitnessAsync(chromosomes, token);
        }

        private async Task ResolvePhenotypeAsync(IEnumerable<Chromosome> chromosomes, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var tasks = chromosomes.Where(chromosome => chromosome.Phenotype == null)
            .Select(async chromosome =>
            {
                chromosome.Phenotype = await _resolver.ResolveAsync(chromosome.Genotype, token);
            });

            await Task.WhenAll(tasks);
        }

        private async Task CalculateFitnessAsync(IEnumerable<Chromosome> chromosomes, CancellationToken token)
        {
            await CalculateOriginalObjectivesValueAsync(chromosomes, token);
            await CalculateNormalizedObjectivesValuesAsync(chromosomes, token);
        }

        private async Task CalculateOriginalObjectivesValueAsync(
            IEnumerable<Chromosome> chromosomes, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var tasks = chromosomes
            .Where(chromosome => chromosome.OriginalObjectivesValue == null)
            .Select(async chromosome =>
            {
                chromosome.OriginalObjectivesValue = new ObjectivesValue(
                    await CalculateObjectivesValueAsync(chromosome, token));
            });

            await Task.WhenAll(tasks);
        }

        private async Task<IReadOnlyDictionary<Objectives, double>> CalculateObjectivesValueAsync(
            Chromosome chromosome, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var tasks = _calculators.Select(async calculator =>
                new KeyValuePair<Objectives, double>(calculator.Objective,
                    await calculator.CalculateAsync(chromosome, token)));

            var result = await Task.WhenAll(tasks);
            return result.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private async Task CalculateNormalizedObjectivesValuesAsync(
            IEnumerable<Chromosome> chromosomes,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var normalizersTasks = _calculators.Select(
                calculator => Task.Run(() =>
                {
                    return new KeyValuePair<ObjectiveValueCalculatorBase, Func<double, double>>(
                        calculator, GenerateObjectiveValueNormalizer(calculator, chromosomes));
                }, token));

            var normalizers = await Task.WhenAll(normalizersTasks);

            var tasks = chromosomes.Select(chromosome => Task.Run(() =>
            {
                var normalizedValues = normalizers.ToDictionary(
                    normalizer => normalizer.Key.Objective,
                    normalizer => normalizer.Value.Invoke(
                        chromosome.OriginalObjectivesValue[normalizer.Key.Objective]));

                chromosome.Fitness = new ObjectivesValue(normalizedValues);
            }, token));

            await Task.WhenAll(tasks);
        }

        private Func<double, double> GenerateObjectiveValueNormalizer(
            ObjectiveValueCalculatorBase calculator, IEnumerable<Chromosome> chromosomes)
        {
            if (!calculator.NeedToBeNormalized)
            {
                return value => value * (int)calculator.Optimum;
            }

            var ordered = chromosomes.Select(chromosome =>
                    chromosome.OriginalObjectivesValue[calculator.Objective] * (int)calculator.Optimum)
                .OrderBy(value => value)
                .ToArray();

            var lowest = ordered.First();
            var highest = ordered.Last();
            var range = Math.Abs(highest - lowest);

            if (range == 0)
            {
                return value => 1;
            }

            return value => Math.Abs((value * (int)calculator.Optimum - lowest)) / range;
        }
    }
}
