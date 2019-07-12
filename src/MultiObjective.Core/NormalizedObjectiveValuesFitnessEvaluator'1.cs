using System;
using System.Collections.Generic;
using System.Linq;
using Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions;
using Bunnypro.GeneticAlgorithm.MultiObjective.Primitives;

namespace Bunnypro.GeneticAlgorithm.MultiObjective.Core
{
    public class NormalizedObjectiveValuesFitnessEvaluator<T> : IChromosomeEvaluator<T> where T : Enum
    {
        private readonly IReadOnlyDictionary<T, OptimumValue> _optimum;
        private readonly IReadOnlyDictionary<T, double> _coefficients;

        public NormalizedObjectiveValuesFitnessEvaluator()
            : this(Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(k => k, _ => OptimumValue.Maximum))
        {
        }

        public NormalizedObjectiveValuesFitnessEvaluator(IReadOnlyDictionary<T, OptimumValue> optimum)
            : this(optimum, Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(k => k, _ => 1d))
        {
        }

        public NormalizedObjectiveValuesFitnessEvaluator(
            IReadOnlyDictionary<T, OptimumValue> optimum,
            IReadOnlyDictionary<T, double> coefficients)
        {
            _optimum = optimum;
            _coefficients = coefficients;
        }

        public void EvaluateAll(IEnumerable<IChromosome<T>> chromosomes)
        {
            var chromosomesArray = chromosomes.ToArray();
            EvaluateObjectiveValuesAll(chromosomesArray);
            var normalizer = Enum.GetValues(typeof(T)).Cast<T>()
                .ToDictionary<T, T, Func<double, double>>(
                    objective => objective,
                    objective =>
                    {
                        var ordered = chromosomesArray
                            .Select(chromosome => chromosome.ObjectiveValues[objective])
                            .OrderBy(value =>
                                value * (_optimum[objective] == OptimumValue.Maximum ? 1 : -1))
                            .ToArray();

                        var boundary = new Dictionary<OptimumValue, double>
                        {
                            {OptimumValue.Maximum, ordered.Last()},
                            {OptimumValue.Minimum, ordered.First()}
                        };

                        var range = Math.Abs(
                            boundary[OptimumValue.Maximum] - boundary[OptimumValue.Minimum]
                        );

                        return value =>
                        {
                            if (range <= 0) return 1;
                            return Math.Abs(
                                       (value - boundary[OptimumValue.Minimum]) *
                                       _coefficients[objective]
                                   ) / range;
                        };
                    });

            foreach (var chromosome in chromosomesArray)
            {
                chromosome.Fitness = chromosome.ObjectiveValues.Sum(objective =>
                                         normalizer[objective.Key].Invoke(objective.Value)
                                     ) / _coefficients.Sum(coefficient => coefficient.Value);
            }
        }

        protected virtual void EvaluateObjectiveValuesAll(IEnumerable<IChromosome<T>> chromosomes)
        {
        }
    }
}