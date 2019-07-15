using System.Collections.Immutable;
using System.Threading.Tasks;
using Bunnypro.GeneticAlgorithm.Abstractions;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.WebApp.Services.GeneticAlgorithm
{
    public interface IGeneticAlgorithmTaskListener
    {
        Task Registered(IGeneticAlgorithmTaskInfo info);
        Task Removing(IGeneticAlgorithmTaskInfo taskInfo);
        Task Removed(IGeneticAlgorithmTaskInfo info);
        Task Building(string id);
        Task BuildCompleted(string id);
        Task BuildFailed(string id);
        Task Starting(string id);
        Task Started(string id);
        Task EvolvedOnce(
            string id,
            GeneticEvolutionStates states,
            ImmutableHashSet<IChromosome> populationChromosomes);
        Task Stopping(string id);
        Task Finished(
            string id,
            GeneticEvolutionStates state,
            ImmutableHashSet<IChromosome> populationChromosomes);
    }
}