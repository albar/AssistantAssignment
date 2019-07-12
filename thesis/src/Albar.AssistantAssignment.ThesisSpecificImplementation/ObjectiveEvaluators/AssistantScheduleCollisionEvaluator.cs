using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation.ObjectiveEvaluators
{
    public class AssistantScheduleCollisionEvaluator : IObjectiveEvaluator<AssignmentObjective>
    {
        public double Evaluate(IAssignmentChromosome<AssignmentObjective> chromosome)
        {
            var schedules = chromosome.Phenotype.Select(representation => new
            {
                Schedule = (Schedule) representation.Schedule,
                Combination = representation.AssistantCombination
            }).ToArray();

            return schedules.Aggregate(0, (count, schedule) =>
            {
                var isCollided = schedules.Any(other =>
                    !other.Schedule.Id.SequenceEqual(schedule.Schedule.Id) &&
                    other.Schedule.Day.Equals(schedule.Schedule.Day) &&
                    other.Schedule.Session.Equals(schedule.Schedule.Session) &&
                    other.Combination.Assistants.Any(id =>
                        schedule.Combination.Assistants
                            .Any(assistant => assistant.SequenceEqual(id)))
                );

                return isCollided ? count + 1 : count;
            });
        }
    }
}