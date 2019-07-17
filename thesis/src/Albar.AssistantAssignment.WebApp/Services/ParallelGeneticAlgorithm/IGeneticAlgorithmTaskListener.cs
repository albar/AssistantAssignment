using System.Threading.Tasks;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.WebApp.Services.ParallelGeneticAlgorithm
{
    public interface IGeneticAlgorithmTaskListener
    {
        Task BuildingTask(string id);
        Task BuildingRepositoryTask(string id);
        Task TaskBuildFinished(string id, object repository);
        Task TaskRemoved(string id);
        Task RunningTask(string id);
        Task EvolvedOnce(string id, object state, object fronts);
        Task StoppingTask(string id);
        Task TaskFinished(string id, object states);
        Task TaskIsRunning(string id);
    }
}