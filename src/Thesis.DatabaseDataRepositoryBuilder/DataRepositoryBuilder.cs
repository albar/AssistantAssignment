using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thesis.DataType;
using Thesis.Database;
using System;

namespace Thesis.DatabaseDataRepositoryBuilder
{
    public class DataRepositoryBuilder
    {
        public static async Task<IDataRepository> CreateDataRepositoryAsync(
            DatabaseContext database, int id,
            CancellationToken token)
        {
            var schedulesBuilder = ImmutableArray.CreateBuilder<Schedule>();
            var coursesBuilder = ImmutableArray.CreateBuilder<Course>();
            var assistantsBuilder = ImmutableArray.CreateBuilder<Assistant>();

            await database.Subjects.Where(subject => subject.Group.Id == id)
                .Include(subject => subject.Schedules)
                .Include(subject => subject.AssistantSubjects)
                .ForEachAsync(subject =>
                {
                    var schedules = subject.Schedules.Select(schedule =>
                        new Schedule(
                            subject.Id,
                            subject.AssistantPerScheduleCount,
                            schedule.Day,
                            schedule.Session));

                    schedulesBuilder.AddRange(schedules);

                    var course = new Course(subject.Id, subject.AssistantSubjects
                            .Select(assistantSubject => assistantSubject.AssistantId)
                            .ToImmutableHashSet(),
                        subject.AssessmentsThreshold.ToImmutableDictionary(
                            assesment => assesment.Key,
                            assesment => assesment.Value
                        ));

                    coursesBuilder.Add(course);
                }, token);

            await database.Assistants.Where(assistant => assistant.Group.Id == id)
                .Include(assistant => assistant.AssistantSubjects)
                .ForEachAsync(assistant =>
                {
                    var result = new Assistant(assistant.Id,
                        assistant.AssistantSubjects.ToImmutableDictionary(
                            assistantSubject => assistantSubject.SubjectId,
                            assistantSubject => assistantSubject.Assessments
                                .ToImmutableDictionary(kv => kv.Key, kv => kv.Value)
                        ));

                    assistantsBuilder.Add(result);
                }, token);

            var schedules = schedulesBuilder.ToImmutable();
            var courses = coursesBuilder.ToImmutable();
            var assistants = assistantsBuilder.ToImmutable();

            return new DataRepository(id, schedules, assistants, courses);
        }
    }
}
