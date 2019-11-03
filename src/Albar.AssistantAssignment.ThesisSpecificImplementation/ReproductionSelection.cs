using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.Abstractions.Primitives;
using Albar.AssistantAssignment.Algorithm;
using Albar.AssistantAssignment.Algorithm.Utilities;
using Albar.AssistantAssignment.DataAbstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;
using Bunnypro.Enumerable.Chunk;
using Bunnypro.Enumerable.Combine;
using Bunnypro.Enumerable.Utility;
using Bunnypro.GeneticAlgorithm.MultiObjective.NSGA2;
using Bunnypro.GeneticAlgorithm.MultiObjective.Primitives;
using Bunnypro.GeneticAlgorithm.Primitives;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation
{
    public class ReproductionSelection : IReproductionSelection<AssignmentObjective>
    {
        private readonly IDataRepository _repository;

        public ReproductionSelection(IDataRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<PreparedMutationParent<AssignmentObjective>> SelectMutationParent(
            IEnumerable<IAssignmentChromosome<AssignmentObjective>> chromosomes,
            PopulationCapacity capacity)
        {
            var chromosomeArray = chromosomes.ToArray();
            return chromosomeArray
                .Where(chromosome =>
                    (int) chromosome.ObjectiveValues[AssignmentObjective.AssistantScheduleCollision] > 0
                )
                .Select(chromosome =>
                {
                    var representations = chromosome.Phenotype
                        .Cast<ScheduleSolutionRepresentation>()
                        .ToArray();
                    var collidedRepresentations = representations.ToDictionary(
                        representation => representation.Schedule.Id, _ => false
                    );
                    foreach (var representation in representations)
                    {
                        if (collidedRepresentations[representation.Schedule.Id]) continue;
                        var currentCollidedRepresentations = representations.Where(other =>
                            other.Schedule.Id != representation.Schedule.Id &&
                            other.Schedule.Day.Equals(representation.Schedule.Day) &&
                            other.Schedule.Session.Equals(representation.Schedule.Session) &&
                            representation.AssistantCombination.Assistants.Any(assistant =>
                                other.AssistantCombination.Assistants.Contains(assistant)
                            )
                        ).Select(r => r.Schedule.Id).ToArray();
                        if (currentCollidedRepresentations.Length > 0)
                            collidedRepresentations[representation.Schedule.Id] = true;

                        foreach (var collided in currentCollidedRepresentations)
                        {
                            if (collidedRepresentations[collided]) continue;
                            collidedRepresentations[collided] = true;
                        }
                    }

                    var schema = collidedRepresentations
                        .OrderBy(collision => collision.Key)
                        .Select(collision => collision.Value)
                        .ToImmutableArray();
                    return new PreparedMutationParent<AssignmentObjective>(schema, chromosome);
                });
        }

        public IEnumerable<PreparedCrossoverParent<AssignmentObjective>> SelectCrossoverParent(
            IEnumerable<IAssignmentChromosome<AssignmentObjective>> chromosomes,
            PopulationCapacity capacity)
        {
            var requiredParentCount = (int) Math.Ceiling((1 + Math.Sqrt(4 * capacity.Minimum + 1)) / 2);
            var subjectsAssessmentThreshold = _repository.Subjects
                .ToDictionary(subject => subject.Key, subject => ((Subject) subject.Value).AssessmentThreshold);
            return chromosomes.OrderByDescending(chromosome => chromosome.Fitness)
                .Take(requiredParentCount).Combine(2).ToInnerArray()
                .Select(parents =>
                {
                    var parentsAssessments = parents.Select(parent =>
                        parent.Genotype.Chunk(_repository.GeneByteSize).ToInnerArray()
                    ).Select(genotype => genotype
                        .Select(gene => _repository.AssistantCombinations[ByteConverter.ToInt32(gene)])
                        .Cast<AssistantCombination>().Select(combination =>
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
                        parents[0],
                        parents[1]
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