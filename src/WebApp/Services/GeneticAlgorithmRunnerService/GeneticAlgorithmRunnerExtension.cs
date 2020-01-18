using AssistantAssignment.WebApp.Services.GeneticAlgorithmRunnerService.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AssistantAssignment.WebApp.Services.GeneticAlgorithmRunnerService
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
