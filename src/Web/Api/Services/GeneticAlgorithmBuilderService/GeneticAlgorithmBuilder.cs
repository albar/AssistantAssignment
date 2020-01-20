using System;
using System.Threading;
using System.Threading.Tasks;
using AssistantAssignment.Data.Abstractions;
using AssistantAssignment.Web.Api.Repositories;
using AssistantAssignment.Web.Api.Services.GeneticAlgorithmBuilderService.Abstractions;

namespace AssistantAssignment.Web.Api.Services.GeneticAlgorithmBuilderService
{
    public class GeneticAlgorithmBuilder : IGeneticAlgorithmBuilder
    {
        public int DataId { get; private set ;}
        public string Id { get; } = Guid.NewGuid().ToString();

        public Task<IGeneticAlgorithmTask> BuildAsync(
            IDataRepository repository,
            CancellationToken token)
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
