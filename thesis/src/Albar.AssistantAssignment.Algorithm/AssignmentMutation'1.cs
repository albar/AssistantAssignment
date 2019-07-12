using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Albar.AssistantAssignment.Abstractions;
using Bunnypro.Enumerable.Chunk;
using Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.Algorithm
{
    public class AssignmentMutation<T> : IMultiObjectiveGeneticOperation<T> where T : Enum
    {
        private readonly IMutationSelection<T> _selection;
        private readonly IGenotypePhenotypeMapper<T> _mapper;

        public AssignmentMutation(
            IMutationSelection<T> selection,
            IGenotypePhenotypeMapper<T> mapper)
        {
            _selection = selection;
            _mapper = mapper;
        }

        public async Task<IEnumerable<IChromosome<T>>> Operate(
            IEnumerable<IChromosome<T>> chromosomes,
            PopulationCapacity capacity,
            CancellationToken token = default)
        {
            var mutationTasks = _selection
                .SelectMutationParent(chromosomes.Cast<IAssignmentChromosome<T>>(), capacity)
                .Select(selection => Task.Run(() => Mutate(selection.Schema, selection.Parent), token));
            token.ThrowIfCancellationRequested();
            var result = await Task.WhenAll(mutationTasks);
            return new HashSet<IChromosome<T>>(result.Select(_mapper.ToChromosome));
        }

        private byte[] Mutate(
            ImmutableArray<bool> schema,
            IAssignmentChromosome<T> chromosome)
        {
            return chromosome.Genotype
                .Chunk(_mapper.DataRepository.GeneSize)
                .ToInnerArray()
                .SelectMany((gene, locus) =>
                {
                    if (!schema[locus]) return gene;
                    var subjectId = _mapper.DataRepository.Schedules[locus].Subject;
                    return _mapper.DataRepository.AssistantCombinations
                        .Where(c => c.Subject == subjectId)
                        .OrderBy(_ => new Random().Next())
                        .First().Id;
                }).ToArray();
        }
    }
}