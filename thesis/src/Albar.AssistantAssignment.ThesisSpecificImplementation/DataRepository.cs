using System;
using System.Collections.Immutable;
using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.Algorithm;
using Albar.AssistantAssignment.Algorithm.Utilities;
using Albar.AssistantAssignment.DataAbstractions;
using Bunnypro.Enumerable.Combine;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation
{
    public class DataRepository : IDataRepository
    {
        public DataRepository(
            ImmutableArray<ISubject> subjects,
            ImmutableArray<ISchedule> schedules,
            ImmutableArray<IAssistant> assistants)
        {
            Subjects = subjects;
            Schedules = schedules;
            Assistants = assistants;
            AssistantCombinations = CombineAssistants(Subjects);
        }

        public byte GeneSize { get; private set; }
        public ImmutableArray<ISubject> Subjects { get; }
        public ImmutableArray<ISchedule> Schedules { get; }
        public ImmutableArray<IAssistant> Assistants { get; }
        public ImmutableArray<IAssistantCombination> AssistantCombinations { get; }

        private ImmutableArray<IAssistantCombination> CombineAssistants(ImmutableArray<ISubject> subjects)
        {
            var combined = subjects.SelectMany(subject =>
                subject.Assistants.Combine(subject.AssistantCountPerScheduleRequirement)
                    .Select(combination => new {Subject = subject.Id, Assistants = combination})
            ).ToArray();
            GeneSize = (byte) Math.Ceiling(Math.Log(combined.Length, 256));
            return combined.Select((combination, i) => new AssistantCombination(
                ByteConverter.GetByte(GeneSize, i), combination.Subject, combination.Assistants
            )).Cast<IAssistantCombination>().ToImmutableArray();
        }
    }
}