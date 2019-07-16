using System;
using System.Collections.Generic;
using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.DataAbstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation.ObjectiveEvaluators
{
    public class AverageOfNormalizedAssessmentEvaluator :
        IObjectiveEvaluator<AssignmentObjective>
    {
        private readonly Dictionary<ISubject, Dictionary<AssistantAssessment, Func<double, double>>>
            _subjectAssessmentNormalizer;

        public AverageOfNormalizedAssessmentEvaluator(
            IDataRepository<AssignmentObjective> repository)
        {
            var assessments = Enum.GetValues(typeof(AssistantAssessment))
                .Cast<AssistantAssessment>().ToArray();

            _subjectAssessmentNormalizer = repository.Subjects.ToDictionary(subject => subject, subject =>
            {
                var subjectCombinedAssistantAssessments = repository.AssistantCombinations
                    .Where(combination => combination.Subject.Equals(subject))
                    .Cast<AssistantCombination>()
                    .Select(combination => combination.MaxAssessments)
                    .ToArray();

                return assessments.ToDictionary<AssistantAssessment, AssistantAssessment, Func<double, double>>(
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
            });
        }

        public double Evaluate(IAssignmentChromosome<AssignmentObjective> chromosome)
        {
            return chromosome.Phenotype.GroupBy(solution => solution.Schedule.Subject)
                .SelectMany(groupedSchedules =>
                {
                    var assessmentNormalizer = _subjectAssessmentNormalizer[groupedSchedules.Key];
                    return groupedSchedules.Select(representation => representation.AssistantCombination)
                        .Cast<AssistantCombination>()
                        .Select(combination => combination.MaxAssessments.Average(assessment =>
                            assessmentNormalizer[assessment.Key].Invoke(assessment.Value)
                        ));
                }).Average();
        }
    }
}