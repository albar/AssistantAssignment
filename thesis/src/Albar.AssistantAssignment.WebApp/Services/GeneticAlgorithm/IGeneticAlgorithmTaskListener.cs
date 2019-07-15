using System;
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
        Task Building(Guid id);
        Task BuildCompleted(Guid id);
        Task BuildFailed(Guid id);
        Task Starting(Guid id);
        Task Started(Guid id);
        Task EvolvedOnce(
            Guid id,
            GeneticEvolutionStates states,
            ImmutableHashSet<IChromosome> populationChromosomes);
        Task Stopping(Guid id);
        Task Finished(
            Guid id,
            GeneticEvolutionStates state,
            ImmutableHashSet<IChromosome> populationChromosomes);
    }
}