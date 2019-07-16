using System.Threading.Tasks;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.WebApp.Services.ParallelGeneticAlgorithm
{
    public interface IGeneticAlgorithmTaskListener
    {
        Task BuildingTask(string id);
        Task BuildingRepositoryTask(string id);
        Task TaskBuildFinished(string id);
        Task TaskRemoved(string id);
        Task RunningTask(string id);
        Task EvolvedOnce(string id, GeneticEvolutionStates state, object fronts);
        Task StoppingTask(string id);
        Task TaskFinished(string id, GeneticEvolutionStates states);
        Task TaskIsRunning(string id);
    }
}