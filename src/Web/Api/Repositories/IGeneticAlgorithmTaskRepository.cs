using System.Collections.Generic;

namespace AssistantAssignment.Web.Api.Repositories
{
    public interface IGeneticAlgorithmTaskRepository
    {
        void Store(string id, IGeneticAlgorithmTask task);
        IGeneticAlgorithmTask Find(string id);
        IEnumerable<IGeneticAlgorithmTask> All();
    }
}
