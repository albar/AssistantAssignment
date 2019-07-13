using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation.ObjectiveEvaluators
{
    public class AboveThresholdAssessmentEvaluator :
        IObjectiveEvaluator<AssignmentObjective>
    {
        public double Evaluate(IAssignmentChromosome<AssignmentObjective> chromosome)
        {
            return chromosome.Phenotype.Cast<ScheduleSolutionRepresentation>()
                .Aggregate(0, (count, solution) =>
                {
                    var relatedSubject = (Subject) solution.Schedule.Subject;
                    var state = relatedSubject.AssessmentThreshold.All(threshold =>
                        solution.AssistantCombination.MaxAssessments[threshold.Key] >=
                        threshold.Value
                    );
                    return state ? count + 1 : count;
                });
        }
    }
}