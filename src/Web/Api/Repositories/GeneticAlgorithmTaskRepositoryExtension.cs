using Microsoft.Extensions.DependencyInjection;

namespace AssistantAssignment.Web.Api.Repositories
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
