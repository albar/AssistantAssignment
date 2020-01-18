using System.Collections.Generic;

namespace AssistantAssignment.WebApp.Repositories
{
    public class GeneticAlgorithmTaskRepository : IGeneticAlgorithmTaskRepository
    {
        private readonly IDictionary<string, IGeneticAlgorithmTask> _tasks =
            new Dictionary<string, IGeneticAlgorithmTask>();

        public void Store(string id, IGeneticAlgorithmTask task)
        {
            _tasks.Add(id, task);
        }

        public IGeneticAlgorithmTask Find(string id)
        {
            return _tasks[id];
        }

        public IEnumerable<IGeneticAlgorithmTask> All()
        {
            return _tasks.Values;
        }
    }
}
