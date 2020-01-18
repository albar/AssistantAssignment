using System;
using System.Collections.Generic;
using AssistantAssignment.WebApp.Services.GeneticAlgorithmRunnerService.Abstractions;

namespace AssistantAssignment.WebApp.Repositories
{
    public interface IGeneticAlgorithmTaskRepository
    {
        void Store(string id, IGeneticAlgorithmTask task);
        IGeneticAlgorithmTask Find(string id);
        IEnumerable<IGeneticAlgorithmTask> All();
    }
}
