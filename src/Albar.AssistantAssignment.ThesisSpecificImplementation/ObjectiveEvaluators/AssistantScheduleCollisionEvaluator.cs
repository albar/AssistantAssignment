using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation.ObjectiveEvaluators
{
    public class AssistantScheduleCollisionEvaluator : IObjectiveEvaluator<AssignmentObjective>
    {
        public double Evaluate(IAssignmentChromosome<AssignmentObjective> chromosome)
        {
            var representations = chromosome.Phenotype.Select(representation => new
            {
                Schedule = (Schedule) representation.Schedule,
                Combination = representation.AssistantCombination
            }).ToArray();
            return representations.Aggregate(0, (count, schedule) =>
            {
                var isCollided = representations.Any(other =>
                    other.Schedule.Id != schedule.Schedule.Id &&
                    other.Schedule.Day.Equals(schedule.Schedule.Day) &&
                    other.Schedule.Session.Equals(schedule.Schedule.Session) &&
                    schedule.Combination.Assistants.Any(
                        assistantId => other.Combination.Assistants.Contains(assistantId)
                    )
                );
                return isCollided ? count + 1 : count;
            });
        }
    }
}