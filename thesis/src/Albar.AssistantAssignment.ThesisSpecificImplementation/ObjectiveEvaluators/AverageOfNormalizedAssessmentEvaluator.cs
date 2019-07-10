using System;
using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation.ObjectiveEvaluators
{
    public class AverageOfNormalizedAssessmentEvaluator : IObjectiveEvaluator<AssignmentObjective>
    {
        public double Evaluate(IAssignmentChromosome<AssignmentObjective> chromosome)
        {
            var assessment = Enum.GetValues(typeof(AssistantAssessment)).Cast<AssistantAssessment>().ToArray();
            return chromosome.Phenotype.GroupBy(solution => solution.Schedule.Subject)
                .SelectMany(schedules =>
                {
                    var representations = schedules.Cast<ScheduleSolutionRepresentation>().ToArray();
                    var assessments = representations.Select(r => r.AssistantCombination.MaxAssessments).ToArray();
                    var minAssessment = assessment.ToDictionary(o => o, o => assessments.Min(a => a[o]));
                    var rangeAssessment = assessment.ToDictionary(o => o, o =>
                    {
                        var range = Math.Abs(assessments.Max(a => a[o]) - minAssessment[o]);
                        return range <= 0 ? 1 : range;
                    });
                    return assessments.Select(representation =>
                        assessment.Sum(o => Math.Abs(representation[o] - minAssessment[o])/ rangeAssessment[o])
                    );
                }).Average();
        }
    }
}