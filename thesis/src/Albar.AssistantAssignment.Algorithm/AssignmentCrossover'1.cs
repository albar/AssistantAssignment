using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.Algorithm.Utilities;
using Bunnypro.Enumerable.Chunk;
using Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.Algorithm
{
    public class AssignmentCrossover<T> : IMultiObjectiveGeneticOperation<T> where T : Enum
    {
        private readonly ICrossoverSelection<T> _selection;
        private readonly IGenotypePhenotypeMapper<T> _mapper;

        public AssignmentCrossover(ICrossoverSelection<T> selection, IGenotypePhenotypeMapper<T> mapper)
        {
            _selection = selection;
            _mapper = mapper;
        }

        public async Task<IEnumerable<IChromosome<T>>> Operate(
            IEnumerable<IChromosome<T>> chromosomes,
            PopulationCapacity capacity,
            CancellationToken token = default)
        {
            var crossoverTasks = _selection
                .SelectCrossoverParent(chromosomes.Cast<IAssignmentChromosome<T>>(), capacity)
                .Select(selection => Task.Run(() =>
                        Crossover(selection.Schema, selection.Parent1, selection.Parent2), token));
            token.ThrowIfCancellationRequested();
            var result = await Task.WhenAll(crossoverTasks);
            return result.SelectMany(r => r.Select(_mapper.ToChromosome));
        }

        private IEnumerable<byte[]> Crossover(
            ImmutableArray<bool> schema,
            IAssignmentChromosome<T> parent1,
            IAssignmentChromosome<T> parent2)
        {
            var p1 = parent1.Genotype.Chunk(_mapper.DataRepository.GeneSize).ToAllArray();
            var p2 = parent2.Genotype.Chunk(_mapper.DataRepository.GeneSize).ToAllArray();
            return schema.Select((isCrossover, locus) =>
                isCrossover ? (p2[locus], p1[locus]) : (p1[locus], p2[locus])
            ).Aggregate(new List<byte>[2], (offspring, gene) =>
            {
                var (g1, g2) = gene;
                offspring[1].AddRange(g1);
                offspring[2].AddRange(g2);
                return offspring;
            }).Select(off => off.ToArray());
        }
    }
}