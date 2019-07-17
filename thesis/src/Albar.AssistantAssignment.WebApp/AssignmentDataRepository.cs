using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Albar.AssistantAssignment.DataAbstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation;
using Albar.AssistantAssignment.WebApp.Models;
using Microsoft.EntityFrameworkCore;
using Assistant = Albar.AssistantAssignment.ThesisSpecificImplementation.Data.Assistant;
using Schedule = Albar.AssistantAssignment.ThesisSpecificImplementation.Data.Schedule;
using Subject = Albar.AssistantAssignment.ThesisSpecificImplementation.Data.Subject;

namespace Albar.AssistantAssignment.WebApp
{
    public class AssignmentDataRepository : DataRepository, IEquatable<AssignmentDataRepository>
    {
        public AssignmentDataRepository(
            Group @group,
            ImmutableArray<ISubject> subjects,
            ImmutableArray<ISchedule> schedules,
            ImmutableArray<IAssistant> assistants) :
            base(subjects, schedules, assistants)
        {
            Group = group;
        }

        public Group Group { get; }

        public static async Task<AssignmentDataRepository> BuildAsync(
            AssignmentDatabase database,
            Group @group,
            CancellationToken token)
        {
            var allSubjects = await database.Subjects.Where(subject => subject.Group.Id == group.Id)
                .Include(subject => subject.Schedules)
                .ToListAsync(token);

            var subjects = allSubjects.Select(subject =>
            {
                var subjectData = new Subject(
                    subject.Id,
                    subject.AssistantPerScheduleCount,
                    subject.AssessmentsThreshold
                )
                {
                    Code = subject.Code
                };

                subjectData.Schedules = subject.Schedules.Select(schedule => (ISchedule) new Schedule(
                    schedule.Id,
                    subjectData,
                    schedule.Day,
                    schedule.Session,
                    schedule.Lab
                )).ToImmutableArray();

                return (ISubject) subjectData;
            }).ToImmutableArray();

            var assistants = database.Assistants.Where(assistant => assistant.Group.Id == group.Id)
                .Include(assistant => assistant.AssistantSubjects)
                .ToList()
                .Select(assistant =>
                {
                    var assistantSubjects = subjects.Where(subject =>
                        assistant.AssistantSubjects.Any(ass => subject.Id == ass.SubjectId)
                    ).ToImmutableArray();

                    var assistantAssessments = assistantSubjects
                        .ToImmutableDictionary(
                            subject => subject,
                            subject => assistant.AssistantSubjects
                                .First(ass => ass.SubjectId == subject.Id)
                                .Assessments
                        );

                    return (IAssistant) new Assistant(
                        assistant.Id, assistantSubjects, assistantAssessments
                    )
                    {
                        Npm = assistant.Npm.ToString()
                    };
                }).ToImmutableArray();

            foreach (var subject1 in subjects)
            {
                var subject = (Subject) subject1;
                subject.Assistants = assistants
                    .Where(assistant => assistant.Subjects.Contains(subject))
                    .ToImmutableArray();
            }

            var schedules = subjects.SelectMany(subject => subject.Schedules).ToImmutableArray();

            return new AssignmentDataRepository(group, subjects, schedules, assistants);
        }

        public bool Equals(AssignmentDataRepository other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || Group.Id == other.Group.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((AssignmentDataRepository) obj);
        }

        public override int GetHashCode()
        {
            return Group != null ? Group.Id : 0;
        }
    }
}