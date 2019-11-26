using Microsoft.Extensions.DependencyInjection;
using Thesis.WebApp.Services.GeneticAlgorithmBuilderService.Abstractions;

namespace Thesis.WebApp.Services.GeneticAlgorithmBuilderService
{
    public static class GeneticAlgorithmBuilderExtension
    {
        public static IServiceCollection AddGeneticAlgorithmBuilderService(
            this IServiceCollection services)
        {
            services.AddSingleton<IGeneticAlgorithmBuilderQueue, GeneticAlgorithmBuilderQueue>();
            services.AddHostedService<HostedBuilderService>();
            return services;
        }
    }
}
