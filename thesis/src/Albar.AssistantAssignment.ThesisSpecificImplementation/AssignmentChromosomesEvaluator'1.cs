using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions;
using Bunnypro.GeneticAlgorithm.MultiObjective.Core;
using Bunnypro.GeneticAlgorithm.MultiObjective.Primitives;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation
{
    public class AssignmentChromosomesEvaluator<T> :
        NormalizedObjectiveValuesFitnessEvaluator<T>,
        IEnumerable<KeyValuePair<T, IObjectiveEvaluator<T>>>
        where T : Enum
    {
        private readonly IDictionary<T, IObjectiveEvaluator<T>> _evaluators =
            new Dictionary<T, IObjectiveEvaluator<T>>();

        public AssignmentChromosomesEvaluator(IReadOnlyDictionary<T, OptimumValue> optimum)
            : base(optimum)
        {
        }

        public AssignmentChromosomesEvaluator(
            IReadOnlyDictionary<T, OptimumValue> optimum,
            IReadOnlyDictionary<T, double> coefficient)
            : base(optimum, coefficient)
        {
        }

        public void Add(T objective, IObjectiveEvaluator<T> evaluator)
        {
            _evaluators.Add(objective, evaluator);
        }

        protected override void EvaluateObjectiveValuesAll(IEnumerable<IChromosome<T>> chromosomes)
        {
            if (_evaluators.Count != Enum.GetNames(typeof(T)).Length)
                throw new Exception("Some assignment objective evaluator is not implemented");
            foreach (var chromosome in chromosomes) chromosome.ObjectiveValues = EvaluateObjectiveValues(chromosome);
        }

        private ObjectiveValues<T> EvaluateObjectiveValues(IChromosome<T> chromosome)
        {
            if (!(chromosome is IAssignmentChromosome<T> assignmentChromosome))
                throw new SystemException("The chromosome requires to be " + typeof(IAssignmentChromosome<T>));
            var values = _evaluators.ToDictionary(e => e.Key, e => e.Value.Evaluate(assignmentChromosome));
            return new ObjectiveValues<T>(values);
        }

        public IEnumerator<KeyValuePair<T, IObjectiveEvaluator<T>>> GetEnumerator() => _evaluators.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}