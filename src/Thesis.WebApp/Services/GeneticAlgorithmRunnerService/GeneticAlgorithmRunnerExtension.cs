using Microsoft.Extensions.DependencyInjection;
using Thesis.WebApp.Services.GeneticAlgorithmRunnerService.Abstractions;

namespace Thesis.WebApp.Services.GeneticAlgorithmRunnerService
{
    public static class GeneticAlgorithmRunnerExtension
    {
        public static IServiceCollection AddGeneticAlgorithmRunnerService(
            this IServiceCollection services)
        {
            services.AddSingleton<IGeneticAlgorithmRunnerBuilder, GeneticAlgorithmRunnerBuilder>();
            services.AddHostedService<HostedRunnerService>();
            return services;
        }
    }
}
