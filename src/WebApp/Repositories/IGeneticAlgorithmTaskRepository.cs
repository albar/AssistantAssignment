using System.Collections.Generic;

namespace AssistantAssignment.WebApp.Repositories
{
    public interface IGeneticAlgorithmTaskRepository
    {
        void Store(string id, IGeneticAlgorithmTask task);
        IGeneticAlgorithmTask Find(string id);
        IEnumerable<IGeneticAlgorithmTask> All();
    }
}
