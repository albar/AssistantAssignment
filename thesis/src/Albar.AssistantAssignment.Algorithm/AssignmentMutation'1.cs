using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.Algorithm.Utilities;
using Albar.AssistantAssignment.DataAbstractions;
using Bunnypro.Enumerable.Chunk;
using Bunnypro.Enumerable.Utility;
using Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.Algorithm
{
    public class AssignmentMutation<T> : IMultiObjectiveGeneticOperation<T> where T : Enum
    {
        private readonly IMutationSelection<T> _selection;
        private readonly IGenotypePhenotypeMapper<T> _mapper;
        private Dictionary<ISubject, IAssistantCombination[]> _subjectAssistantCombination;

        public AssignmentMutation(
            IMutationSelection<T> selection,
            IGenotypePhenotypeMapper<T> mapper)
        {
            _selection = selection;
            _mapper = mapper;
            _subjectAssistantCombination = _mapper.DataRepository.AssistantCombinations.GroupBy(asc => asc.Subject)
                .ToDictionary(group => group.Key, group => group.ToArray());
        }

        public async Task<IEnumerable<IChromosome<T>>> Operate(
            IEnumerable<IChromosome<T>> chromosomes,
            PopulationCapacity capacity,
            CancellationToken token = default)
        {
            var mutationTasks = _selection
                .SelectMutationParent(chromosomes.Cast<IAssignmentChromosome<T>>(), capacity)
                .Select(selected => Task.Run(() => Mutate(selected.Schema, selected.Parent), token));
            token.ThrowIfCancellationRequested();
            var result = await Task.WhenAll(mutationTasks);
            return new HashSet<IChromosome<T>>(result.Select(_mapper.ToChromosome));
        }

        private byte[] Mutate(
            ImmutableArray<bool> schema,
            IAssignmentChromosome<T> chromosome)
        {
            var random = new Random();
            return chromosome.Genotype
                .Chunk(_mapper.DataRepository.GeneByteSize)
                .ToInnerArray()
                .SelectMany((gene, locus) =>
                {
                    if (!schema[locus]) return gene;
                    var subject = _mapper.DataRepository.Schedules[locus].Subject;
                    var combinations = _subjectAssistantCombination[subject];
                    var assistantCombinationId = combinations[random.Next(0, combinations.Length - 1)].Id;
                    return ByteConverter.GetByte(
                        _mapper.DataRepository.GeneByteSize,
                        assistantCombinationId
                    );
                }).ToArray();
        }
    }
}