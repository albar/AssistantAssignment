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
        private readonly IReadOnlyDictionary<T, double> _coefficient;

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
            IReadOnlyDictionary<T, double> coefficient)
        {
            _optimum = optimum;
            _coefficient = coefficient;
        }

        public void EvaluateAll(IEnumerable<IChromosome<T>> chromosomes)
        {
            var chromosomesArray = chromosomes.ToArray();
            EvaluateObjectiveValuesAll(chromosomesArray);
            var normalizer = Enum.GetValues(typeof(T)).Cast<T>()
                .ToDictionary<T, T, Func<double, double>>(key => key, key =>
                {
                    var ordered = chromosomesArray
                        .Select(c => c.ObjectiveValues[key])
                        .OrderBy(v => v * (_optimum[key] == OptimumValue.Maximum ? 1 : -1))
                        .ToArray();

                    var boundary = new Dictionary<OptimumValue, double>
                    {
                        {OptimumValue.Maximum, ordered.Last()},
                        {OptimumValue.Minimum, ordered.First()}
                    };

                    var range = Math.Abs(boundary[OptimumValue.Maximum] - boundary[OptimumValue.Minimum]);
                    return value =>
                    {
                        if (range <= 0) return 1;
                        return Math.Abs((value - boundary[OptimumValue.Minimum]) * _coefficient[key]) / range;
                    };
                });

            foreach (var chromosome in chromosomesArray)
            {
                chromosome.Fitness = chromosome.ObjectiveValues.Sum(objective =>
                                         normalizer[objective.Key].Invoke(objective.Value)
                                     ) / _coefficient.Sum(c => c.Value);
            }
        }

        protected virtual void EvaluateObjectiveValuesAll(IEnumerable<IChromosome<T>> chromosomes)
        {
        }
    }
}