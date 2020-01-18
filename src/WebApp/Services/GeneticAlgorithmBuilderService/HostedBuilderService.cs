using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AssistantAssignment.Data.Repository;
using AssistantAssignment.Data.Types;
using AssistantAssignment.Data.Database;
using AssistantAssignment.WebApp.Repositories;
using AssistantAssignment.WebApp.Services.GeneticAlgorithmBuilderService.Abstractions;

namespace AssistantAssignment.WebApp.Services.GeneticAlgorithmBuilderService
{
    public class HostedBuilderService : BackgroundService
    {
        private IServiceProvider _provider;
        private readonly IGeneticAlgorithmBuilderQueue _queue;
        private readonly IGeneticAlgorithmTaskRepository _repository;
        private readonly IDictionary<int, IDataRepository> _cachedDataRepositories
            = new Dictionary<int, IDataRepository>();

        public HostedBuilderService(IServiceProvider provider,
            IGeneticAlgorithmBuilderQueue queue,
            IGeneticAlgorithmTaskRepository repository)
        {
            _provider = provider;
            _queue = queue;
            _repository = repository;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var builder = await _queue.DequeueAsync(token);
                var repository = await FindOrCreateDataRepositoryAsync(builder.DataId, token);
                var task = await builder.BuildAsync(repository, token);
                _repository.Store(builder.Id, task);
            }
        }

        private async Task<IDataRepository> FindOrCreateDataRepositoryAsync(int dataId,
            CancellationToken token)
        {
            if (_cachedDataRepositories.ContainsKey(dataId))
                return _cachedDataRepositories[dataId];

            await using var database = _provider.CreateScope().ServiceProvider
                .GetRequiredService<DatabaseContext>();
            var repository = await Data.Repository.DataRepositoryBuilder.CreateDataRepositoryAsync(database, dataId, token);
            _cachedDataRepositories.Add(repository.Id, repository);
            return repository;
        }
    }
}
