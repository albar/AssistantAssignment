using System;
using Bunnypro.GeneticAlgorithm.MultiObjective.Primitives;

namespace Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions
{
    public interface IObjectiveEvaluator<T> where T : Enum
    {
        ObjectiveValues<T> Evaluate(IChromosome<T> chromosome);
    }
}