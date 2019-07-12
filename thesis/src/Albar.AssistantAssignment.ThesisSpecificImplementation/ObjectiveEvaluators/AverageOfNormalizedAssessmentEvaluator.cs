using System;
using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation.ObjectiveEvaluators
{
    public class AverageOfNormalizedAssessmentEvaluator :
        IObjectiveEvaluator<AssignmentObjective>
    {
        private readonly DataRepository _repository;

        public AverageOfNormalizedAssessmentEvaluator(
            IDataRepository<AssignmentObjective> repository)
        {
            _repository = (DataRepository) repository;
        }

        public double Evaluate(IAssignmentChromosome<AssignmentObjective> chromosome)
        {
            var assessments = Enum.GetValues(typeof(AssistantAssessment))
                .Cast<AssistantAssessment>().ToArray();

            return chromosome.Phenotype.GroupBy(solution => solution.Schedule.Subject)
                .SelectMany(schedules =>
                {
                    var subjectCombinedAssistantAssessments = _repository.AssistantCombinations
                        .Where(combination => combination.Subject.SequenceEqual(schedules.Key))
                        .Cast<AssistantCombination>()
                        .Select(combination => combination.MaxAssessments)
                        .ToArray();

                    var assessmentNormalizer = assessments
                        .ToDictionary<AssistantAssessment, AssistantAssessment, Func<double, double>>(
                            assessment => assessment,
                            assessment =>
                            {
                                var minimumCombinedAssistantAssessment =
                                    subjectCombinedAssistantAssessments
                                        .Min(combinedAssessments => combinedAssessments[assessment]);

                                var maximumCombinedAssistantAssessment =
                                    subjectCombinedAssistantAssessments
                                        .Max(combinedAssessments => combinedAssessments[assessment]);

                                var range = Math.Abs(
                                    maximumCombinedAssistantAssessment -
                                    minimumCombinedAssistantAssessment
                                );

                                return value =>
                                {
                                    if (range <= 0) return 1d;
                                    return Math.Abs(
                                               value - minimumCombinedAssistantAssessment
                                           ) / range;
                                };
                            });

                    return schedules.Select(representation => representation.AssistantCombination)
                        .Cast<AssistantCombination>()
                        .Select(combination => combination.MaxAssessments.Average(assessment =>
                            assessmentNormalizer[assessment.Key].Invoke(assessment.Value)
                        ));
                }).Average();
        }
    }
}