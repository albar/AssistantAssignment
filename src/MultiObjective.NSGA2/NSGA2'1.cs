using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions;
using Bunnypro.GeneticAlgorithm.MultiObjective.Core;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Bunnypro.GeneticAlgorithm.MultiObjective.NSGA2
{
    public class NSGA2<T> : DistinctMultiObjectiveGeneticOperation<T> where T : Enum
    {
        private readonly IMultiObjectiveGeneticOperation<T> _reproduction;
        private readonly IObjectiveValuesEvaluator<T> _evaluator;
        private readonly IDistinctMultiObjectiveGeneticOperation<T> _offspringSelection = new OffspringSelection<T>();

        public NSGA2(IMultiObjectiveGeneticOperation<T> reproduction, IObjectiveValuesEvaluator<T> evaluator)
        {
            _reproduction = reproduction;
            _evaluator = evaluator;
        }

        public override async Task<ImmutableHashSet<IChromosome<T>>> Operate(
            IEnumerable<IChromosome<T>> chromosomes,
            PopulationCapacity capacity,
            CancellationToken token = default)
        {
            var offspring = ImmutableHashSet.CreateBuilder<IChromosome<T>>();

            // Genetic Operations (Selection, Crossover, Mutation) <-- Parent Chromosomes with Evaluated Objective Values --> Offspring
            {
                var parents = chromosomes.ToList();

                // Reproduction
                offspring.UnionWith(await _reproduction.Operate(parents, capacity, token));

                // Evaluate Offspring Objective Values
                foreach (var child in offspring) child.ObjectiveValues = _evaluator.Evaluate(child);

                // Reinsert Parent for Elitism
                offspring.UnionWith(parents);
            }

            return await _offspringSelection.Operate(offspring.ToImmutableHashSet(), capacity, token);
        }
    }
}