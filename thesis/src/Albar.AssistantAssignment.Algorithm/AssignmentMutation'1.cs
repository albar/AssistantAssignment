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
        private readonly ISingleParentReproductionSchemaResolver<IAssignmentChromosome<T>> _schema;
        private readonly IGenotypePhenotypeMapper<T> _mapper;

        public AssignmentMutation(
            ISingleParentReproductionSchemaResolver<IAssignmentChromosome<T>> schema,
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
            var badChromosomes = SelectChromosomes(chromosomes.Cast<IAssignmentChromosome<T>>());
            var mutationTasks = badChromosomes.Select(c => Task.Run(() => Mutate(c), token));
            token.ThrowIfCancellationRequested();
            var result = await Task.WhenAll(mutationTasks);
            return result.Select(_mapper.ToChromosome);
        }

        private static IEnumerable<IAssignmentChromosome<T>> SelectChromosomes(
            IEnumerable<IAssignmentChromosome<T>> chromosomes)
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Select(
                objective => chromosomes.OrderBy(c => c.ObjectiveValues[objective]).First()
            ).Distinct();
        }

        private byte[] Mutate(IAssignmentChromosome<T> chromosome)
        {
            var schema = _schema.Resolve(chromosome);
            return chromosome.Genotype.Chunk(_mapper.DataRepository.GeneSize).SelectMany((gene, locus) =>
            {
                if (!schema[locus]) return gene.ToArray();
                var subjectId = _mapper.DataRepository.Schedules[locus].Subject;
                return _mapper.DataRepository.AssistantCombinations
                    .Where(c => c.Subject == subjectId)
                    .OrderBy(_ => new Random().Next())
                    .First().Id;
            }).ToArray();
        }
    }
}