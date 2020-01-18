using System;
using System.Threading;
using System.Threading.Tasks;
using AssistantAssignment.Data.Abstractions;
using AssistantAssignment.WebApp.Repositories;
using AssistantAssignment.WebApp.Services.GeneticAlgorithmBuilderService.Abstractions;

namespace AssistantAssignment.WebApp.Services.GeneticAlgorithmBuilderService
{
    public class GeneticAlgorithmBuilder : IGeneticAlgorithmBuilder
    {
        public int DataId { get; private set ;}
        public string Id { get; } = Guid.NewGuid().ToString();

        public Task<IGeneticAlgorithmTask> BuildAsync(IDataRepository repository, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public IGeneticAlgorithmBuilder WithDataId(int dataId)
        {
            DataId = dataId;
            return this;
        }
    }
}
