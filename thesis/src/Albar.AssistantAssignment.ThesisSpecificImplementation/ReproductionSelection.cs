using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.Abstractions.Primitives;
using Albar.AssistantAssignment.Algorithm;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;
using Bunnypro.Enumerable.Chunk;
using Bunnypro.Enumerable.Combine;
using Bunnypro.GeneticAlgorithm.MultiObjective.NSGA2;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation
{
    public class ReproductionSelection : IReproductionSelection<AssignmentObjective>
    {
        private readonly IGenotypePhenotypeMapper<AssignmentObjective> _mapper;

        private NonDominatedComparer<AssistantAssessment, double> _comparer =
            new NonDominatedComparer<AssistantAssessment, double>();

        public ReproductionSelection(IGenotypePhenotypeMapper<AssignmentObjective> mapper)
        {
            _mapper = mapper;
        }

        public IEnumerable<PreparedMutationParent<AssignmentObjective>> SelectMutationParent(
            IEnumerable<IAssignmentChromosome<AssignmentObjective>> chromosomes, PopulationCapacity capacity)
        {
            var chromosomeArray = chromosomes.ToArray();
            var mutationCount = chromosomeArray.Length / 10;
            return chromosomeArray
                .OrderBy(c => c.ObjectiveValues[AssignmentObjective.ScheduleCollision] * -1)
                .Take(mutationCount)
                .Cast<AssignmentChromosome<AssignmentObjective>>()
                .Select(chromosome =>
                {
                    var schedules = chromosome.Phenotype.Cast<ScheduleSolutionRepresentation>().ToArray();
                    var schema = schedules.Select(schedule => schedules.Any(other =>
                        !other.Schedule.Id.SequenceEqual(schedule.Schedule.Id) &&
                        other.Schedule.Day.Equals(schedule.Schedule.Day) &&
                        other.Schedule.Session.Equals(schedule.Schedule.Session) &&
                        other.AssistantCombination.Assistants.Any(id => schedule.AssistantCombination.Assistants.Any(a => a.SequenceEqual(id)))
                    )).ToImmutableArray();
                    return new PreparedMutationParent<AssignmentObjective>(schema, chromosome);
                });
        }

        public IEnumerable<PreparedCrossoverParent<AssignmentObjective>> SelectCrossoverParent(
            IEnumerable<IAssignmentChromosome<AssignmentObjective>> chromosomes, PopulationCapacity capacity)
        {
            var requiredParentCount = (int) Math.Ceiling((1 + Math.Sqrt(4 * capacity.Minimum + 1)) / 2);
            return chromosomes.OrderByDescending(c => c.Fitness)
                .Take(requiredParentCount).Combine(2)
                .Select(parents =>
                {
                    var parentArray = parents as IAssignmentChromosome<AssignmentObjective>[] ?? parents.ToArray();
                    var parentsAssessments = parentArray.Select(parent =>
                        parent.Genotype.Chunk(_mapper.DataRepository.GeneSize).ToInnerArray()
                    ).Select(genotype => genotype.Select(gene =>
                            _mapper.DataRepository.AssistantCombinations.First(a => a.Id.SequenceEqual(gene))
                        ).Cast<AssistantCombination>().Select(combination => combination.MaxAssessments).ToArray()
                    ).ToArray();

                    var schema = _mapper.DataRepository.Schedules.Select((_, i) =>
                        _comparer.Compare(parentsAssessments[0][i], parentsAssessments[1][i]) < 0
                    ).ToImmutableArray();
                    return new PreparedCrossoverParent<AssignmentObjective>(schema, parentArray[0], parentArray[1]);
                });
        }
    }
}