using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Albar.AssistantAssignment.DataAbstractions;
using Bunnypro.GeneticAlgorithm.MultiObjective.Primitives;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IDataRepository<T> where T : Enum
    {
        byte GeneByteSize { get; }
        IReadOnlyDictionary<T, OptimumValue> ObjectiveOptimumValue { get; }
        IReadOnlyDictionary<T, double> ObjectiveCoefficient { get; }
        ImmutableArray<ISubject> Subjects { get; }
        ImmutableArray<ISchedule> Schedules { get; }
        ImmutableArray<IAssistant> Assistants { get; }
        ImmutableArray<IAssistantCombination> AssistantCombinations { get; }
    }
}