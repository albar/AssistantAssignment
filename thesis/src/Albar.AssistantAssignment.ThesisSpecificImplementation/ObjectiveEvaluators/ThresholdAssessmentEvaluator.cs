using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;
using Bunnypro.GeneticAlgorithm.MultiObjective.NSGA2;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation.ObjectiveEvaluators
{
    public abstract class ThresholdAssessmentEvaluator : IObjectiveEvaluator<AssignmentObjective>
    {
        private readonly IDataRepository<AssignmentObjective> _repository;
        private readonly NonDominatedComparer<AssistantAssessment, double> _comparer;

        public ThresholdAssessmentEvaluator(IDataRepository<AssignmentObjective> repository)
        {
            _repository = repository;
            _comparer = new NonDominatedComparer<AssistantAssessment, double>();
        }

        public double Evaluate(IAssignmentChromosome<AssignmentObjective> chromosome)
        {
            return chromosome.Phenotype.Cast<ScheduleSolutionRepresentation>()
                .Aggregate(0, (count, solution) =>
                {
                    var subject =
                        (Subject) _repository.Subjects.First(s => s.Id.SequenceEqual(solution.Schedule.Subject));
                    var domination = _comparer.Compare(solution.AssistantCombination.MaxAssessments,
                        subject.AssessmentThreshold);
                    return DominationEvaluator(domination) ? count + 1 : count;
                });
        }

        protected abstract bool DominationEvaluator(int domination);
    }
}