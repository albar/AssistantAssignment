using System.Threading;
using System.Threading.Tasks;
using Thesis.DataType;
using Thesis.WebApp.Repositories;

namespace Thesis.WebApp.Services.GeneticAlgorithmBuilderService.Abstractions
{
    public interface IGeneticAlgorithmBuilder
    {
        string Id { get; }
        int DataId { get; }

        Task<IGeneticAlgorithmTask> BuildAsync(IDataRepository repository, CancellationToken token);
        IGeneticAlgorithmBuilder WithDataId(int dataId);
    }
}
