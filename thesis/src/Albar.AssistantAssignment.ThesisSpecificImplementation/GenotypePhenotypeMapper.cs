using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.Algorithm;
using Albar.AssistantAssignment.Algorithm.Utilities;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;
using Bunnypro.Enumerable.Chunk;
using Bunnypro.Enumerable.Utility;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation
{
    public class GenotypePhenotypeMapper : IGenotypePhenotypeMapper<AssignmentObjective>
    {
        public IDataRepository<AssignmentObjective> DataRepository { get; }

        public GenotypePhenotypeMapper(IDataRepository<AssignmentObjective> repository)
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
            return genotype.Chunk(DataRepository.AssistantCombinationIdByteSize).ToInnerArray()
                .Select((gene, locus) =>
                {
                    return new ScheduleSolutionRepresentation
                    {
                        Schedule = (Schedule) DataRepository.Schedules
                            .First(schedule => schedule.Id == locus),
                        AssistantCombination = (AssistantCombination) DataRepository
                            .AssistantCombinations
                            .First(combination => combination.Id == ByteConverter.ToInt32(gene))
                    };
                });
        }

        public IAssignmentChromosome<AssignmentObjective> ToChromosome(
            IEnumerable<IScheduleSolutionRepresentation> solution)
        {
            var scheduleSolution = solution as IScheduleSolutionRepresentation[] ?? solution.ToArray();
            var genotype = scheduleSolution.SelectMany(schedule => ByteConverter.GetByte(
                DataRepository.AssistantCombinationIdByteSize,
                schedule.AssistantCombination.Id
            ));

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