using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Albar.AssistantAssignment.DataAbstractions;
using Bunnypro.GeneticAlgorithm.MultiObjective.Primitives;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IDataRepository<T> where T : Enum
    {
        byte GeneSize { get; }
        IReadOnlyDictionary<T, OptimumValue> OptimumValue { get; }
        ImmutableArray<ISubject> Subjects { get; }
        ImmutableArray<ISchedule> Schedules { get; }
        ImmutableArray<IAssistant> Assistants { get; }
        ImmutableArray<IAssistantCombination> AssistantCombinations { get; }
    }
}