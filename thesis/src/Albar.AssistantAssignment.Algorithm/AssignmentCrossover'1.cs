using System;
using System.Collections.Generic;
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
        private readonly IReproductionSchemaResolver<IAssignmentChromosome<T>> _schema;
        private readonly IGenotypePhenotypeMapper<T> _mapper;

        public AssignmentCrossover(IReproductionSchemaResolver<IAssignmentChromosome<T>> schema,
            IGenotypePhenotypeMapper<T> mapper)
        {
            _schema = schema;
            _mapper = mapper;
        }

        public async Task<IEnumerable<IChromosome<T>>> Operate(
            IEnumerable<IChromosome<T>> chromosomes,
            PopulationCapacity capacity,
            CancellationToken token = default)
        {
            var parents = SelectParents(chromosomes.Cast<IAssignmentChromosome<T>>(), capacity);
            var combinedParents = CombineParents(parents);
            var crossoverTasks = combinedParents.Select(parent =>
                Task.Run(() => Crossover(parent.P1, parent.P2), token)
            );
            token.ThrowIfCancellationRequested();
            var result = await Task.WhenAll(crossoverTasks);
            return result.SelectMany(r => r.Select(_mapper.ToChromosome));
        }

        private static IEnumerable<IAssignmentChromosome<T>> SelectParents(
            IEnumerable<IAssignmentChromosome<T>> chromosomes,
            PopulationCapacity capacity)
        {
            var parentRequired = (int) Math.Ceiling((1 + Math.Sqrt(4 * capacity.Minimum + 1)) / 2);
            return chromosomes.OrderByDescending(c => c.Fitness).Take(parentRequired);
        }

        private static IEnumerable<(IAssignmentChromosome<T> P1, IAssignmentChromosome<T> P2)> CombineParents(
            IEnumerable<IAssignmentChromosome<T>> chromosomes)
        {
            var parents = chromosomes.ToList();
            if (parents.Count < 2) throw new Exception("2 parents minimum required to be combined");
            for (var i = 0; i < parents.Count - 1; i++)
            for (var j = i + 1; j < parents.Count; j++)
                yield return (parents[i], parents[j]);
        }

        private IEnumerable<byte[]> Crossover(
            IAssignmentChromosome<T> parent1,
            IAssignmentChromosome<T> parent2)
        {
            var schema = _schema.Resolve(parent1, parent2);
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