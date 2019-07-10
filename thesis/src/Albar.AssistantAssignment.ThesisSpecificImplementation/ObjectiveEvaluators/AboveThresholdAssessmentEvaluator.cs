using Albar.AssistantAssignment.Abstractions;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation.ObjectiveEvaluators
{
    public class AboveThresholdAssessmentEvaluator : ThresholdAssessmentEvaluator
    {
        public AboveThresholdAssessmentEvaluator(IDataRepository<AssignmentObjective> repository) : base(repository)
        {
        }

        protected override bool DominationEvaluator(int domination) => domination >= 0;
    }
}