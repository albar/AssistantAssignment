using System.Collections.Generic;

namespace Bunnypro.GeneticAlgorithm.Abstractions
{
    public interface IChromosomeEvaluator
    {
        double Evaluate(IChromosome chromosome);
        void EvaluateAll(IEnumerable<IChromosome> chromosomes);
    }
}