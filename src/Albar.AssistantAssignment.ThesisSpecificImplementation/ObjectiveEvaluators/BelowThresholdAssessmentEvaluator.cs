using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation.ObjectiveEvaluators
{
    public class BelowThresholdAssessmentEvaluator :
        IObjectiveEvaluator<AssignmentObjective>
    {
        private readonly IDataRepository _repository;

        public BelowThresholdAssessmentEvaluator(IDataRepository repository)
        {
            _repository = repository;
        }
        
        public double Evaluate(IAssignmentChromosome<AssignmentObjective> chromosome)
        {
            return chromosome.Phenotype.Cast<ScheduleSolutionRepresentation>()
                .Aggregate(0, (count, solution) =>
                {
                    var relatedSubject = (Subject) _repository.Subjects[solution.Schedule.Subject];
                    var state = relatedSubject.AssessmentThreshold.Any(threshold =>
                        solution.AssistantCombination.MaxAssessments[threshold.Key] <
                        threshold.Value
                    );
                    return state ? count + 1 : count;
                });
        }
    }
}