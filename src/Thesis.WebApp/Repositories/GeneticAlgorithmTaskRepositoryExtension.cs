using System;
using Microsoft.Extensions.DependencyInjection;

namespace Thesis.WebApp.Repositories
{
    public static class GeneticAlgorithmTaskRepositoryExtension
    {
        public static IServiceCollection AddGeneticAlgorithmTaskRepository(
            this IServiceCollection services)
        {
            services.AddSingleton<IGeneticAlgorithmTaskRepository, GeneticAlgorithmTaskRepository>();
            return services;
        }
    }
}
