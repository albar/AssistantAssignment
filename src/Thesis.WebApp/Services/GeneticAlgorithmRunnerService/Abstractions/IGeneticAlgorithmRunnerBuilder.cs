namespace Thesis.WebApp.Services.GeneticAlgorithmRunnerService.Abstractions
{
    public interface IGeneticAlgorithmRunnerBuilder
    {
        IGeneticAlgorithmRunner Build();
        IGeneticAlgorithmRunnerBuilder WithSize(int size);
        IGeneticAlgorithmRunnerBuilder WithMutation(bool isWithMutation = false);
    }
}
