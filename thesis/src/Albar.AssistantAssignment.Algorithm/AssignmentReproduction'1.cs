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
        private readonly IGenotypePhenotypeMapper<T> _mapper;
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
            _mapper = mapper;
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
            return new HashSet<IChromosome<T>>(result.SelectMany(r => r)).Cast<AssignmentChromosome<T>>().Select(ResolvePhenotype);
        }

        private IChromosome<T> ResolvePhenotype(AssignmentChromosome<T> chromosome)
        {
            chromosome.Phenotype = _mapper.ToSolution(chromosome);
            return chromosome;
        }
    }
}