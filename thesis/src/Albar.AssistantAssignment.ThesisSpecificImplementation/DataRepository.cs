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
            AssistantCombinations = CombineAssistants();
        }

        public byte GeneSize { get; private set; }
        public IReadOnlyDictionary<AssignmentObjective, OptimumValue> ObjectiveOptimumValue { get; }
        public IReadOnlyDictionary<AssignmentObjective, double> ObjectiveCoefficient { get; }
        public ImmutableArray<ISubject> Subjects { get; }
        public ImmutableArray<ISchedule> Schedules { get; }
        public ImmutableArray<IAssistant> Assistants { get; }
        public ImmutableArray<IAssistantCombination> AssistantCombinations { get; }

        private ImmutableArray<IAssistantCombination> CombineAssistants()
        {
            var combined = Subjects.SelectMany(subject =>
                Assistants.Where(a => a.Subjects.Contains(subject.Id))
                    .Combine(subject.AssistantCountPerScheduleRequirement)
                    .Select(combination => new
                    {
                        Subject = subject.Id,
                        Assistants = combination.Cast<Assistant>()
                            .ToDictionary(a => a.Id, a => a.SubjectAssessments[subject.Id])
                    })
            ).ToArray();
            GeneSize = (byte) Math.Ceiling(Math.Log(combined.Length, 256));
            return combined.Select((combination, i) =>
            {
                var assessmentCombination = Enum.GetValues(typeof(AssistantAssessment))
                    .Cast<AssistantAssessment>()
                    .ToDictionary(assessment => assessment, assessment =>
                        combination.Assistants.Select(a => a.Value[assessment]).Max()
                    );
                return new AssistantCombination(
                    ByteConverter.GetByte(GeneSize, i),
                    combination.Subject,
                    combination.Assistants.Select(a => a.Key),
                    assessmentCombination
                );
            }).Cast<IAssistantCombination>().ToImmutableArray();
        }
    }
}