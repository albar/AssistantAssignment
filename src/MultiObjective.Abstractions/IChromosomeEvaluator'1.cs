using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions
{
    public interface IChromosomeEvaluator<T> where T : Enum
    {
        Task EvaluateAll(IEnumerable<IChromosome<T>> chromosomes, CancellationToken token = default);
    }
}