using System;
using Albar.AssistantAssignment.WebApp.Models;

namespace Albar.AssistantAssignment.WebApp.Services.GeneticAlgorithm
{
    public interface IGeneticAlgorithmTaskInfo
    {
        GeneticAlgorithmTaskState State { get; }
        Guid Id { get; }
        Group Group { get; }
    }
}