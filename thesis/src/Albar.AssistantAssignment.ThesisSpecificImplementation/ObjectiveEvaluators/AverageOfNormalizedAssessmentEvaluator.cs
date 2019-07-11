using System;
using System.Linq;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation.ObjectiveEvaluators
{
    public class AverageOfNormalizedAssessmentEvaluator : IObjectiveEvaluator<AssignmentObjective>
    {
        private readonly DataRepository _repository;

        public AverageOfNormalizedAssessmentEvaluator(IDataRepository<AssignmentObjective> repository)
        {
            _repository = (DataRepository) repository;
        }

        public double Evaluate(IAssignmentChromosome<AssignmentObjective> chromosome)
        {
            var assessment = Enum.GetValues(typeof(AssistantAssessment)).Cast<AssistantAssessment>().ToArray();
            return chromosome.Phenotype.GroupBy(solution => solution.Schedule.Subject)
                .SelectMany(schedules =>
                {
                    var subjectCombinedAssistantAssessments = _repository.AssistantCombinations
                        .Where(c => c.Subject.SequenceEqual(schedules.Key))
                        .Cast<AssistantCombination>()
                        .Select(c => c.MaxAssessments)
                        .ToArray();

//                    var representations = schedules.Cast<ScheduleSolutionRepresentation>().ToArray();
//                    var assessments = representations.Select(r => r.AssistantCombination.MaxAssessments).ToArray();
                    var minAssessment = assessment.ToDictionary(o => o, o =>
                        subjectCombinedAssistantAssessments.Min(a => a[o])
                    );
                    var assessmentNormalizer = assessment
                        .ToDictionary<AssistantAssessment, AssistantAssessment, Func<double, double>>(o => o, o =>
                        {
                            var range = Math.Abs(subjectCombinedAssistantAssessments.Max(a => a[o]) -
                                                 minAssessment[o]);
                            return value =>
                            {
                                if (range <= 0) return 1d;
                                return Math.Abs(value - minAssessment[o]) / range;
                            };
                        });

                    return schedules.Select(representation => representation.AssistantCombination)
                        .Cast<AssistantCombination>()
                        .Select(combination => combination.MaxAssessments.Average(ass =>
                            assessmentNormalizer[ass.Key].Invoke(ass.Value)
                        ));


//                    return subjectCombinedAssistantAssessments.Select(representation =>
//                        assessment.Sum(o => Math.Abs(representation[o] - minAssessment[o]) / assessmentNormalizer[o])
//                    );
                }).Average();
        }
    }
}