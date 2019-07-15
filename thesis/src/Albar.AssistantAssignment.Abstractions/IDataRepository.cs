using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Albar.AssistantAssignment.DataAbstractions;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IDataRepository<T> where T : Enum
    {
        byte GeneByteSize { get; }
//        IReadOnlyDictionary<T, double> ObjectiveCoefficients { get; }
        ImmutableArray<ISubject> Subjects { get; }
        ImmutableArray<ISchedule> Schedules { get; }
        ImmutableArray<IAssistant> Assistants { get; }
        ImmutableArray<IAssistantCombination> AssistantCombinations { get; }
    }
}