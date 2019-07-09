using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.Algorithm;
using Albar.AssistantAssignment.Algorithm.Utilities;
using Bunnypro.Enumerable.Chunk;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation
{
    public class GenotypePhenotypeMapper : IGenotypePhenotypeMapper<AssignmentObjective>
    {
        public IDataRepository DataRepository { get; }

        public GenotypePhenotypeMapper(IDataRepository repository)
        {
            DataRepository = repository;
        }

        public IEnumerable<IScheduleSolutionRepresentation> ToSolution(
            IAssignmentChromosome<AssignmentObjective> chromosome)
        {
            return chromosome.Phenotype ?? ToSolution(chromosome.Genotype.ToArray());
        }

        public IEnumerable<IScheduleSolutionRepresentation> ToSolution(byte[] genotype)
        {
            return genotype.Chunk(DataRepository.GeneSize).Select((gene, locus) => new ScheduleSolutionRepresentation
            {
                Schedule = DataRepository.Schedules.First(s => ByteConverter.ToInt32(s.Id) == locus),
                AssistantCombination = DataRepository.AssistantCombinations.First(c => c.Id == gene.ToArray())
            });
        }

        public IAssignmentChromosome<AssignmentObjective> ToChromosome(
            IEnumerable<IScheduleSolutionRepresentation> solution)
        {
            var scheduleSolution = solution as IScheduleSolutionRepresentation[] ?? solution.ToArray();
            var genotype = scheduleSolution.SelectMany(schedule => schedule.AssistantCombination.Id);

            return new AssignmentChromosome<AssignmentObjective>(genotype.ToImmutableArray())
            {
                Phenotype = scheduleSolution
            };
        }

        public IAssignmentChromosome<AssignmentObjective> ToChromosome(byte[] genotype)
        {
            return new AssignmentChromosome<AssignmentObjective>(genotype.ToImmutableArray());
        }
    }
}