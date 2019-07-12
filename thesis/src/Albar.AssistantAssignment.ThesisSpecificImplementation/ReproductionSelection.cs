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
using Bunnypro.GeneticAlgorithm.MultiObjective.Primitives;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation
{
    public class ReproductionSelection : IReproductionSelection<AssignmentObjective>
    {
        private readonly IDataRepository<AssignmentObjective> _repository;

        public ReproductionSelection(IDataRepository<AssignmentObjective> repository)
        {
            _repository = repository;
        }

        public IEnumerable<PreparedMutationParent<AssignmentObjective>> SelectMutationParent(
            IEnumerable<IAssignmentChromosome<AssignmentObjective>> chromosomes,
            PopulationCapacity capacity)
        {
            var chromosomeArray = chromosomes.ToArray();
            var mutationCount = chromosomeArray.Length / 10;
            return chromosomeArray
                .OrderByDescending(chromosome =>
                    chromosome.ObjectiveValues[AssignmentObjective.JobCollision])
                .Take(mutationCount)
                .Select(chromosome =>
                {
                    var schedules = chromosome.Phenotype
                        .Cast<ScheduleSolutionRepresentation>()
                        .ToArray();
                    var schema = schedules.Select(schedule => schedules.Any(other =>
                        !other.Schedule.Id.SequenceEqual(schedule.Schedule.Id) &&
                        other.Schedule.Day.Equals(schedule.Schedule.Day) &&
                        other.Schedule.Session.Equals(schedule.Schedule.Session) &&
                        other.AssistantCombination.Assistants.Any(id =>
                            schedule.AssistantCombination.Assistants.Any(a => a.SequenceEqual(id)))
                    )).ToImmutableArray();
                    return new PreparedMutationParent<AssignmentObjective>(schema, chromosome);
                });
        }

        public IEnumerable<PreparedCrossoverParent<AssignmentObjective>> SelectCrossoverParent(
            IEnumerable<IAssignmentChromosome<AssignmentObjective>> chromosomes,
            PopulationCapacity capacity)
        {
            var requiredParentCount = (int) Math.Ceiling((1 + Math.Sqrt(4 * capacity.Minimum + 1)) / 2);
            var subjectsAssessmentThreshold = _repository.Subjects
                .Cast<Subject>()
                .ToDictionary(subject => subject.Id, subject => subject.AssessmentThreshold);
            var comparedObjective = new Dictionary<AssignmentObjective, OptimumValue>
            {
                {AssignmentObjective.AboveThresholdAssessment, OptimumValue.Maximum},
                {AssignmentObjective.BelowThresholdAssessment, OptimumValue.Minimum},
                {AssignmentObjective.AverageOfNormalizedAssessment, OptimumValue.Maximum}
            };
            var comparer = new NonDominatedComparer<AssignmentObjective, double>(comparedObjective);
            var ordered = chromosomes.ToList();
            ordered.Sort((first, second) =>
                comparer.Compare(second.ObjectiveValues, first.ObjectiveValues));
            return ordered.Take(requiredParentCount).Combine(2)
                .Select(parents =>
                {
                    var parentArray = parents as IAssignmentChromosome<AssignmentObjective>[] ??
                                      parents.ToArray();
                    var parentsAssessments = parentArray.Select(parent =>
                        parent.Genotype.Chunk(_repository.GeneSize).ToInnerArray()
                    ).Select(genotype => genotype.Select(gene =>
                            _repository.AssistantCombinations
                                .First(combination => combination.Id.SequenceEqual(gene))
                        ).Cast<AssistantCombination>().Select(combination =>
                            IsBelowThreshold(
                                combination.MaxAssessments,
                                subjectsAssessmentThreshold[combination.Subject])
                                ? -1
                                : 1
                        ).ToArray()
                    ).ToArray();

                    var schema = _repository.Schedules.Select((_, i) =>
                        parentsAssessments[0][i] * parentsAssessments[1][i] < 0
                    ).ToImmutableArray();
                    return new PreparedCrossoverParent<AssignmentObjective>(
                        schema,
                        parentArray[0],
                        parentArray[1]
                    );
                });
        }

        private static bool IsBelowThreshold(
            IReadOnlyDictionary<AssistantAssessment, double> assessments,
            IReadOnlyDictionary<AssistantAssessment, double> threshold)
        {
            return assessments.Any(assessment => assessment.Value < threshold[assessment.Key]);
        }
    }
}