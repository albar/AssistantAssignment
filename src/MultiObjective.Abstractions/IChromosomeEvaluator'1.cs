using System;
using System.Collections.Generic;

namespace Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions
{
    public interface IChromosomeEvaluator<T> where T : Enum
    {
        void EvaluateAll(IEnumerable<IChromosome<T>> chromosomes);
    }
}