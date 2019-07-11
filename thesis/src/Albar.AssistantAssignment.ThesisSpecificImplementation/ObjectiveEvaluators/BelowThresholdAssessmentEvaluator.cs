using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation.ObjectiveEvaluators
{
    public class BelowThresholdAssessmentEvaluator : IObjectiveEvaluator<AssignmentObjective>
    {
        private readonly IDataRepository<AssignmentObjective> _repository;

        public BelowThresholdAssessmentEvaluator(IDataRepository<AssignmentObjective> repository)
        {
            _repository = repository;
        }

        public double Evaluate(IAssignmentChromosome<AssignmentObjective> chromosome)
        {
            return chromosome.Phenotype.Cast<ScheduleSolutionRepresentation>()
                .Aggregate(0, (count, solution) =>
                {
                    var subject = (Subject) _repository.Subjects
                        .First(s => s.Id.SequenceEqual(solution.Schedule.Subject));
                    var state = subject.AssessmentThreshold.Any(threshold =>
                        solution.AssistantCombination.MaxAssessments[threshold.Key] < threshold.Value
                    );
                    return state ? count + 1 : count;
                });
        }
    }
}