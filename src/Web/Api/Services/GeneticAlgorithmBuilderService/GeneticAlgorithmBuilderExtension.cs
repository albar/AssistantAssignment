using AssistantAssignment.Web.Api.Services.GeneticAlgorithmBuilderService.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AssistantAssignment.Web.Api.Services.GeneticAlgorithmBuilderService
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
