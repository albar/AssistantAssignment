using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Albar.AssistantAssignment.DataAbstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation
{
    public static class DummyDataFactory
    {
        public static HashSet<ISubject> CreateSubject(int count)
        {
            var assessments = Enum.GetValues(typeof(AssistantAssessment)).Cast<AssistantAssessment>().ToArray();
            var subjects = Enumerable.Range(0, count).Select(
                id => new Subject(id, 3, assessments.ToDictionary(assessment => assessment, _ => 8d))
            );
            return new HashSet<ISubject>(subjects);
        }

        public static HashSet<ISchedule> CreateSchedule(IEnumerable<Subject> subjects, int min = 5, int max = 10)
        {
            var randomize = new Random();
            var subjectArray = subjects as Subject[] ?? subjects.ToArray();

            var schedules = subjectArray.SelectMany(
                subject => Enumerable.Range(0, randomize.Next(min, max)).Select(_ => subject)
            ).Select((subject, i) => new KeyValuePair<int, Subject>(i, subject)).ToDictionary(v => v.Key, v => v.Value);

            var days = Enum.GetNames(typeof(DayOfWeek)).Length;
            var sessions = Enum.GetNames(typeof(SessionOfDay)).Length;

            var result = schedules.Aggregate(new HashSet<ISchedule>(), (all, schedule) =>
            {
                Schedule newSchedule;
                do
                {
                    newSchedule = new Schedule(
                        schedule.Key,
                        schedule.Value,
                        (DayOfWeek) randomize.Next(0, days - 1),
                        (SessionOfDay) randomize.Next(0, sessions - 1),
                        randomize.Next(1, 20)
                    );
                } while (!all.Add(newSchedule));

                return all;
            });

            foreach (var subject in subjectArray)
            {
                subject.Schedules = result.Where(s => s.Subject.Equals(subject)).ToImmutableArray();
            }

            return result;
        }

        public static HashSet<IAssistant> CreateAssistant(IEnumerable<Subject> subjects)
        {
            var subjectArray = subjects as Subject[] ?? subjects.ToArray();
            var assistantStorage = new List<ImmutableArray<ISubject>>();
            var subjectAssistantsCount = subjectArray.ToDictionary(
                subject => subject, subject => new[] {subject.Schedules.Length + 3, 0}
            );
            do
            {
                var subjectIds = subjectAssistantsCount
                    .OrderBy(s => s.Value[1])
                    .Take(2).Select(s =>
                    {
                        s.Value[1]++;
                        return s.Key;
                    })
                    .Cast<ISubject>()
                    .ToImmutableArray();
                assistantStorage.Add(subjectIds);
            } while (subjectAssistantsCount.Any(s => s.Value[1] < s.Value[0]));

            var subjectStorage = subjectArray.ToDictionary(subject => subject, _ => new HashSet<int>());
            var randomize = new Random();
            var assessments = Enum.GetValues(typeof(AssistantAssessment)).Cast<AssistantAssessment>().ToArray();
            var assistants = assistantStorage.Select((assistantSubjects, id) =>
            {
                foreach (var subject in subjectStorage.Where(subject => assistantSubjects.Contains(subject.Key)))
                {
                    subject.Value.Add(id);
                }

                var assistantAssessments = assistantSubjects.ToImmutableDictionary(subject => subject, subjectId =>
                    assessments.ToDictionary(assessment => assessment, _ => (double) randomize.Next(6, 9)));
                return new Assistant(id, assistantSubjects, assistantAssessments);
            }).ToArray();
            foreach (var subject in subjectStorage)
            {
                subject.Key.Assistants = assistants
                    .Where(assistant => subject.Value.Contains(assistant.Id))
                    .Cast<IAssistant>()
                    .ToImmutableArray();
            }

            return new HashSet<IAssistant>(assistants);
        }
    }
}