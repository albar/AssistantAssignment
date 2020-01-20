using AssistantAssignment.Web.Api.Services.GeneticAlgorithmRunnerService.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AssistantAssignment.Web.Api.Services.GeneticAlgorithmRunnerService
{
    public static class GeneticAlgorithmRunnerExtension
    {
        public static IServiceCollection AddGeneticAlgorithmRunnerService(
            this IServiceCollection services)
        {
            services.AddHostedService<HostedRunnerService>();
            return services;
        }
    }
}
