namespace Bunnypro.GeneticAlgorithm.Abstractions
{
    public interface IFitnessEvaluator
    {
        double Evaluate(IChromosome chromosome);
    }
}