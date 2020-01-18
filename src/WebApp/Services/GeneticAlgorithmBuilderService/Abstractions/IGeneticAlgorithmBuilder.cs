using System.Threading;
using System.Threading.Tasks;
using AssistantAssignment.Data.Abstractions;
using AssistantAssignment.WebApp.Repositories;

namespace AssistantAssignment.WebApp.Services.GeneticAlgorithmBuilderService.Abstractions
{
    public interface IGeneticAlgorithmBuilder
    {
        string Id { get; }
        int DataId { get; }

        Task<IGeneticAlgorithmTask> BuildAsync(IDataRepository repository, CancellationToken token);
        IGeneticAlgorithmBuilder WithDataId(int dataId);
    }
}
