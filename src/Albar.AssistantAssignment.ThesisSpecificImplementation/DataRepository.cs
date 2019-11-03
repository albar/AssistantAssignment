using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.DataAbstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;
using Bunnypro.Enumerable.Combine;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation
{
    public class DataRepository : IDataRepository
    {
        public DataRepository(
            ImmutableDictionary<int, ISubject> subjects,
            ImmutableDictionary<int, ISchedule> schedules,
            ImmutableDictionary<int, IAssistant> assistants
        )
        {
            Subjects = subjects;
            Schedules = schedules;
            Assistants = assistants;
            AssistantCombinations = CombineAssistants();
        }

        public byte GeneByteSize { get; private set; }
        public ImmutableDictionary<int, ISubject> Subjects { get; }
        public ImmutableDictionary<int, ISchedule> Schedules { get; }
        public ImmutableDictionary<int, IAssistant> Assistants { get; }
        public ImmutableDictionary<int, IAssistantCombination> AssistantCombinations { get; }

        private ImmutableDictionary<int, IAssistantCombination> CombineAssistants()
        {
            var combined = Subjects.Values.SelectMany(subject =>
                Assistants.Values.Where(assistant => assistant.Subjects.Contains(subject.Id))
                    .Combine(subject.AssistantCountPerScheduleRequirement)
                    .Select(combination => new
                    {
                        Subject = subject.Id,
                        AssistantsAssessments = combination
                            .Cast<Assistant>()
                            .ToDictionary(
                                assistant => assistant.Id,
                                assistant => assistant.SubjectAssessments[subject.Id]
                            )
                    })
            ).ToArray();
            GeneByteSize = (byte) Math.Ceiling(Math.Log(combined.Length, 256));
            return combined.Select((combination, i) =>
            {
                var assessmentCombination = Enum.GetValues(typeof(AssistantAssessment))
                    .Cast<AssistantAssessment>()
                    .ToDictionary(assessment => assessment, assessment =>
                        combination.AssistantsAssessments
                            .Select(assessments => assessments.Value[assessment])
                            .Max()
                    );
                return new AssistantCombination(
                    i,
                    combination.Subject,
                    combination.AssistantsAssessments.Select(assistant => assistant.Key),
                    assessmentCombination
                );
            }).Cast<IAssistantCombination>().ToImmutableDictionary(c => c.Id, c => c);
        }
    }
}