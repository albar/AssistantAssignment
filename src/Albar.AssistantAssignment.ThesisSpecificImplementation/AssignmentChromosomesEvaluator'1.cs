using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public AssignmentChromosomesEvaluator(IReadOnlyDictionary<T, double> coefficients)
            : base(coefficients)
        {
        }

        public void Add(T objective, IObjectiveEvaluator<T> evaluator)
        {
            _evaluators.Add(objective, evaluator);
        }

        protected override async Task EvaluateObjectiveValuesAll(
            IEnumerable<IChromosome<T>> chromosomes,
            CancellationToken token = default)
        {
            if (_evaluators.Count != Enum.GetNames(typeof(T)).Length)
                throw new Exception("Some assignment objective evaluator is not implemented");
            var chromosomeEvaluationTasks = chromosomes
                .Where(chromosome => chromosome.Fitness <= 0)
                .Select(async chromosome =>
                {
                    chromosome.ObjectiveValues = await EvaluateObjectiveValues(chromosome);
                });
            await Task.WhenAll(chromosomeEvaluationTasks);
        }

        private async Task<ObjectiveValues<T>> EvaluateObjectiveValues(IChromosome<T> chromosome)
        {
            if (!(chromosome is IAssignmentChromosome<T> assignmentChromosome))
                throw new SystemException("The chromosome requires to be " + typeof(IAssignmentChromosome<T>));
            var evaluationTasks = _evaluators.Select(evaluator => Task.Run(() =>
                new KeyValuePair<T, double>(evaluator.Key, evaluator.Value.Evaluate(assignmentChromosome))
            ));
            var results = await Task.WhenAll(evaluationTasks);
            var values = results.ToDictionary(result => result.Key, result => result.Value);
            return new ObjectiveValues<T>(values);
        }

        public IEnumerator<KeyValuePair<T, IObjectiveEvaluator<T>>> GetEnumerator() => _evaluators.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}