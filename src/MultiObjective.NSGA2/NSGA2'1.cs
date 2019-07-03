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
        private readonly IMultiObjectiveGeneticOperation<T> _crossover;
        private readonly IMultiObjectiveGeneticOperation<T> _mutation;
        private readonly IObjectiveEvaluator<T> _objectiveValuesEvaluator;
        private readonly IDistinctMultiObjectiveGeneticOperation<T> _offspringSelection = new OffspringSelection<T>();

        public NSGA2(IMultiObjectiveGeneticOperation<T> crossover,
            IMultiObjectiveGeneticOperation<T> mutation,
            IObjectiveEvaluator<T> evaluator)
        {
            _crossover = crossover;
            _mutation = mutation;
            _objectiveValuesEvaluator = evaluator;
        }

        public override async Task<ImmutableHashSet<IChromosome<T>>> Operate(IEnumerable<IChromosome<T>> chromosomes,
            PopulationCapacity capacity,
            CancellationToken token = default)
        {
            var offspring = ImmutableHashSet.CreateBuilder<IChromosome<T>>();

            // Genetic Operations (Selection, Crossover, Mutation) <-- Parent Chromosomes with Evaluated Objective Values --> Offspring
            {
                var parents = chromosomes.ToList();

                // Crossover
                offspring.UnionWith(await _crossover.Operate(parents, capacity, token));

                // Mutation
                offspring.UnionWith(await _mutation.Operate(parents, capacity, token));

                // Evaluate Offspring Objective Values
                foreach (var child in offspring) child.ObjectiveValues = _objectiveValuesEvaluator.Evaluate(child);

                // Reinsert Parent for Elitism
                offspring.UnionWith(parents);
            }

            return await _offspringSelection.Operate(offspring.ToImmutableHashSet(), capacity, token);
        }
    }
}