using System;
using System.Collections.Generic;
using Thesis.WebApp.Services.GeneticAlgorithmRunnerService.Abstractions;

namespace Thesis.WebApp.Repositories
{
    public interface IGeneticAlgorithmTaskRepository
    {
        void Store(string id, IGeneticAlgorithmTask task);
        IGeneticAlgorithmTask Find(string id);
        IEnumerable<IGeneticAlgorithmTask> All();
    }
}
