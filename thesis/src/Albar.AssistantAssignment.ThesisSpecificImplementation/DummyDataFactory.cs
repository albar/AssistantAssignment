using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Albar.AssistantAssignment.Algorithm.Utilities;
using Albar.AssistantAssignment.DataAbstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation
{
    public static class DummyDataFactory
    {
        public static HashSet<ISubject> CreateSubject(int count)
        {
            var byteSize = (byte) Math.Ceiling(Math.Log(count, 256));
            var assessments = Enum.GetValues(typeof(AssistantAssessment)).Cast<AssistantAssessment>().ToArray();
            var subjects = Enumerable.Range(1, count).Select(
                id => new Subject(
                    ByteConverter.GetByte(byteSize, id), 3,
                    assessments.ToDictionary(assessment => assessment, _ => 3d)
                )
            );
            return new HashSet<ISubject>(subjects);
        }

        public static HashSet<ISchedule> CreateSchedule(IEnumerable<Subject> subjects, int min = 5, int max = 10)
        {
            var randomize = new Random();
            var subjectArray = subjects as Subject[] ?? subjects.ToArray();

            var schedules = subjectArray.SelectMany(
                s => Enumerable.Range(0, randomize.Next(min, max)).Select(_ => s)
            ).Select((s, i) => new KeyValuePair<int, Subject>(i, s)).ToDictionary(v => v.Key, v => v.Value);

            var byteSize = (byte) Math.Ceiling(Math.Log(schedules.Count, 256));
            var days = Enum.GetNames(typeof(DayOfWeek)).Length;
            var sessions = Enum.GetNames(typeof(SessionOfDay)).Length;

            var result = schedules.Aggregate(new HashSet<ISchedule>(), (all, subject) =>
            {
                Schedule schedule;
                do
                {
                    schedule = new Schedule(
                        ByteConverter.GetByte(byteSize, subject.Key),
                        subject.Value.Id,
                        (DayOfWeek) randomize.Next(0, days - 1),
                        (SessionOfDay) randomize.Next(0, sessions - 1),
                        randomize.Next(1, 20)
                    );
                } while (!all.Add(schedule));

                return all;
            });

            foreach (var subject in subjectArray)
            {
                subject.Schedules = result.Where(s => s.Subject == subject.Id).Select(s => s.Id).ToImmutableArray();
            }

            return result;
        }

        public static HashSet<IAssistant> CreateAssistant(IEnumerable<Subject> subjects)
        {
            var subjectArray = subjects as Subject[] ?? subjects.ToArray();
            var assistantStorage = new List<ImmutableArray<byte[]>>();
            var subjectAssistantsCount = subjectArray.ToDictionary(s => s.Id, s => new[] {s.Schedules.Length + 3, 0});
            do
            {
                var subjectIds = subjectAssistantsCount
                    .OrderBy(s => s.Value[1])
                    .Take(2).Select(s =>
                    {
                        s.Value[1]++;
                        return s.Key;
                    }).ToImmutableArray();
                assistantStorage.Add(subjectIds);
            } while (subjectAssistantsCount.Any(s => s.Value[1] < s.Value[0]));

            var subjectStorage = subjectArray.ToDictionary(s => s, _ => new HashSet<byte[]>());
            var byteSize = (byte) Math.Ceiling(Math.Log(assistantStorage.Count, 256));
            var randomize = new Random();
            var assessments = Enum.GetValues(typeof(AssistantAssessment)).Cast<AssistantAssessment>().ToArray();
            var assistants = assistantStorage.Select((subjectIds, id) =>
            {
                var assistantId = ByteConverter.GetByte(byteSize, id);
                foreach (var subject in subjectStorage.Where(s => subjectIds.Contains(s.Key.Id)))
                {
                    subject.Value.Add(assistantId);
                }

                var assistantAssessments = subjectIds.ToImmutableDictionary(subjectId => subjectId, subjectId =>
                    assessments.ToDictionary(assessment => assessment, _ => (double) randomize.Next(2,4)));
                return new Assistant(assistantId, subjectIds, assistantAssessments);
            }).ToArray();
            foreach (var subject in subjectStorage)
            {
                subject.Key.Assistants = subject.Value.ToImmutableArray();
            }

            return new HashSet<IAssistant>(assistants);
        }
    }
}