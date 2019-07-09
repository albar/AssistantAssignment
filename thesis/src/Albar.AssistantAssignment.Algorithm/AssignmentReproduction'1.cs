using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Albar.AssistantAssignment.Abstractions;
using Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.Algorithm
{
    public class AssignmentReproduction<T> : IMultiObjectiveGeneticOperation<T> where T : Enum
    {
        private readonly IEnumerable<IMultiObjectiveGeneticOperation<T>> _operations;

        public AssignmentReproduction(
            IGenotypePhenotypeMapper<T> mapper,
            IReproductionSelection<T> parentSelection)
            : this(mapper, parentSelection, parentSelection)
        {
        }

        public AssignmentReproduction(
            IGenotypePhenotypeMapper<T> mapper,
            ICrossoverSelection<T> crossoverSelection,
            IMutationSelection<T> mutationSelection)
        {
            _operations = new IMultiObjectiveGeneticOperation<T>[]
            {
                new AssignmentCrossover<T>(crossoverSelection, mapper),
                new AssignmentMutation<T>(mutationSelection, mapper)
            };
        }

        public async Task<IEnumerable<IChromosome<T>>> Operate(
            IEnumerable<IChromosome<T>> chromosomes,
            PopulationCapacity capacity,
            CancellationToken token = default)
        {
            var operations = _operations.Select(operation => operation.Operate(chromosomes, capacity, token));
            var result = await Task.WhenAll(operations);
            return result.SelectMany(r => r);
        }
    }
}