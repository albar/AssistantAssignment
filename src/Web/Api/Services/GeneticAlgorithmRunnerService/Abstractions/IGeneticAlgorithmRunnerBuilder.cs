namespace AssistantAssignment.Web.Api.Services.GeneticAlgorithmRunnerService.Abstractions
{
    public interface IGeneticAlgorithmRunnerBuilder
    {
        IGeneticAlgorithmRunnerBuilder WithSize(int size);
        IGeneticAlgorithmRunnerBuilder WithMutation(bool isWithMutation = false);
        IGeneticAlgorithmRunner Build();
    }
}
