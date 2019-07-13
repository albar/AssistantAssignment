using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions;

namespace Bunnypro.GeneticAlgorithm.MultiObjective.Core
{
    public abstract class NormalizedObjectiveValuesFitnessEvaluator<T> : IChromosomeEvaluator<T> where T : Enum
    {
        private readonly IReadOnlyDictionary<T, double> _coefficients;

        public NormalizedObjectiveValuesFitnessEvaluator() :
            this(Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(k => k, _ => 1d))
        {
        }

        public NormalizedObjectiveValuesFitnessEvaluator(IReadOnlyDictionary<T, double> coefficients)
        {
            _coefficients = coefficients;
        }

        public async Task EvaluateAll(IEnumerable<IChromosome<T>> chromosomes, CancellationToken token = default)
        {
            var chromosomesArray = chromosomes.ToArray();
            var fitnessEvaluableChromosomes = await EvaluateObjectiveValuesAll(chromosomesArray, token);
            await EvaluateFitnessValueAll(fitnessEvaluableChromosomes);
        }

        private async Task EvaluateFitnessValueAll(IEnumerable<IChromosome<T>> chromosomes)
        {
            var chromosomeArray = chromosomes.ToArray();
            var objectivesNormalizer = Enum.GetValues(typeof(T)).Cast<T>()
                .ToDictionary<T, T, Func<double, double>>(
                    objective => objective,
                    objective =>
                    {
                        var ordered = chromosomeArray
                            .Select(chromosome => chromosome.ObjectiveValues[objective])
                            .OrderBy(value => value * _coefficients[objective])
                            .ToArray();

                        var boundary = new
                        {
                            Max = ordered.Last(),
                            Min = ordered.First()
                        };

                        var range = Math.Abs(boundary.Max - boundary.Min);

                        return value =>
                        {
                            if (range <= 0) return 1;
                            return Math.Abs(
                                       (value - boundary.Min) * _coefficients[objective]
                                   ) / range;
                        };
                    });

            var coefficientSum = _coefficients.Sum(coefficient => Math.Abs(coefficient.Value));
            var fitnessEvaluationTasks = chromosomeArray.Select(chromosome => Task.Run(() =>
            {
                chromosome.Fitness = chromosome.ObjectiveValues.Sum(objective =>
                                         objectivesNormalizer[objective.Key].Invoke(objective.Value)
                                     ) / coefficientSum;
            }));

            await Task.WhenAll(fitnessEvaluationTasks);
        }

        protected abstract Task<IEnumerable<IChromosome<T>>> EvaluateObjectiveValuesAll(
            IEnumerable<IChromosome<T>> chromosomes,
            CancellationToken token = default);
    }
}