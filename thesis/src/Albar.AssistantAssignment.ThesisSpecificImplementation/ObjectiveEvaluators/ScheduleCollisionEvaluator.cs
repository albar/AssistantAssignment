using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation.ObjectiveEvaluators
{
    public class ScheduleCollisionEvaluator : IObjectiveEvaluator<AssignmentObjective>
    {
        public double Evaluate(IAssignmentChromosome<AssignmentObjective> chromosome)
        {
            var schedules = chromosome.Phenotype.Select(p =>
                new {Schedule = (Schedule) p.Schedule, Assistants = p.AssistantCombination}
            ).ToArray();
            return schedules.Aggregate(0, (count, schedule) =>
            {
                var isCollided = schedules.Any(other =>
                    !other.Schedule.Id.SequenceEqual(schedule.Schedule.Id) &&
                    other.Schedule.Day.Equals(schedule.Schedule.Day) &&
                    other.Schedule.Session.Equals(schedule.Schedule.Session) &&
                    other.Assistants.Assistants.Any(id => schedule.Assistants.Assistants.Any(a => a.SequenceEqual(id))));

                return isCollided ? count + 1 : count;
            });
        }
    }
}