using Albar.AssistantAssignment.Abstractions;

namespace Albar.AssistantAssignment.ThesisSpecificImplementation.ObjectiveEvaluators
{
    public class BelowThresholdAssessmentEvaluator : ThresholdAssessmentEvaluator
    {
        public BelowThresholdAssessmentEvaluator(IDataRepository<AssignmentObjective> repository) : base(repository)
        {
        }

        protected override bool DominationEvaluator(int domination) => domination < 0;
    }
}