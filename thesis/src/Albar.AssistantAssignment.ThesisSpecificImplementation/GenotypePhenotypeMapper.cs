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
            return genotype.Chunk(DataRepository.GeneByteSize).ToInnerArray()
                .Select((gene, locus) =>
                {
                    try
                    {
                        var schedule = (Schedule) DataRepository.Schedules[locus];
                        var combination = (AssistantCombination) DataRepository
                            .AssistantCombinations[ByteConverter.ToInt32(gene)];
                        return new ScheduleSolutionRepresentation
                        {
                            Schedule = schedule,
                            AssistantCombination = combination
                        };
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine($"gene: {string.Join("", gene)}, locus: {locus}");
                        throw;
                    }
                });
        }

        public IAssignmentChromosome<AssignmentObjective> ToChromosome(
            IEnumerable<IScheduleSolutionRepresentation> solution)
        {
            var scheduleSolution = solution as IScheduleSolutionRepresentation[] ?? solution.ToArray();
            var genotype = scheduleSolution.SelectMany(schedule => ByteConverter.GetByte(
                DataRepository.GeneByteSize,
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