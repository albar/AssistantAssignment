using System;
using System.Collections.Generic;
using System.Linq;
using Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions;

namespace Bunnypro.GeneticAlgorithm.MultiObjective.Core
{
    public class NormalizedObjectiveValuesFitnessEvaluator<T> : IChromosomeEvaluator<T> where T : Enum
    {
        private readonly IDictionary<T, Optimum> _optimum;
        private readonly IDictionary<T, double> _coefficient;


        public NormalizedObjectiveValuesFitnessEvaluator()
            : this(Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(k => k, _ => Optimum.Maximum))
        {
        }

        public NormalizedObjectiveValuesFitnessEvaluator(IDictionary<T, Optimum> optimum)
            : this(optimum, Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(k => k, _ => 1d))
        {
        }

        public NormalizedObjectiveValuesFitnessEvaluator(
            IDictionary<T, Optimum> optimum,
            IDictionary<T, double> coefficient)
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
                        .OrderBy(v => v).ToArray();

                    var boundary = new Dictionary<Optimum, double>
                    {
                        {Optimum.Maximum, ordered.First()},
                        {Optimum.Minimum, ordered.Last()}
                    };

                    var range = Math.Abs(boundary[Optimum.Maximum] - boundary[Optimum.Minimum]);
                    return value => _coefficient[key] * Math.Abs(value - boundary[_optimum[key]]) / range;
                });

            foreach (var chromosome in chromosomesArray)
            {
                chromosome.Fitness = chromosome.ObjectiveValues.Aggregate(
                    0d, (fitness, objective) => fitness + normalizer[objective.Key].Invoke(objective.Value)
                );
            }
        }

        protected virtual void EvaluateObjectiveValuesAll(IEnumerable<IChromosome<T>> chromosomes)
        {
        }

        public enum Optimum
        {
            Minimum,
            Maximum
        }
    }
}