using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.Algorithm.Utilities;
using Albar.AssistantAssignment.DataAbstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;
using Bunnypro.Enumerable.Combine;
using Bunnypro.GeneticAlgorithm.MultiObjective.Primitives;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation
{
    public class DataRepository : IDataRepository<AssignmentObjective>
    {
        public DataRepository(
            ImmutableArray<ISubject> subjects,
            ImmutableArray<ISchedule> schedules,
            ImmutableArray<IAssistant> assistants,
            IReadOnlyDictionary<AssignmentObjective, OptimumValue> optimumValue,
            IReadOnlyDictionary<AssignmentObjective, double> coefficient)
        {
            Subjects = subjects;
            Schedules = schedules;
            Assistants = assistants;
            ObjectiveOptimumValue = optimumValue;
            ObjectiveCoefficient = coefficient;
            CalculateByteSize();
            AssistantCombinations = CombineAssistants();
        }

        private void CalculateByteSize()
        {
            byte CalculateByteSize(int length) => (byte) Math.Ceiling(Math.Log(length, 256));
            SubjectIdByteSize = CalculateByteSize(Subjects.Length);
            ScheduleIdByteSize = CalculateByteSize(Schedules.Length);
            AssistantIdByteSize = CalculateByteSize(Assistants.Length);
        }

        public byte SubjectIdByteSize { get; private set; }
        public byte ScheduleIdByteSize { get; private set; }
        public byte AssistantIdByteSize { get; private set; }
        public byte AssistantCombinationIdByteSize { get; private set; }
        public IReadOnlyDictionary<AssignmentObjective, OptimumValue> ObjectiveOptimumValue { get; }
        public IReadOnlyDictionary<AssignmentObjective, double> ObjectiveCoefficient { get; }
        public ImmutableArray<ISubject> Subjects { get; }
        public ImmutableArray<ISchedule> Schedules { get; }
        public ImmutableArray<IAssistant> Assistants { get; }
        public ImmutableArray<IAssistantCombination> AssistantCombinations { get; }

        private ImmutableArray<IAssistantCombination> CombineAssistants()
        {
            var combined = Subjects.SelectMany(subject =>
                Assistants.Where(assistant => assistant.Subjects.Contains(subject))
                    .Combine(subject.AssistantCountPerScheduleRequirement)
                    .Select(combination => new
                    {
                        Subject = subject,
                        AssistantsAssessments = combination.Cast<Assistant>()
                            .ToDictionary(
                                assistant => assistant,
                                assistant => assistant.SubjectAssessments[subject]
                            )
                    })
            ).ToArray();
            AssistantCombinationIdByteSize = (byte) Math.Ceiling(Math.Log(combined.Length, 256));
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
            }).Cast<IAssistantCombination>().ToImmutableArray();
        }
    }
}