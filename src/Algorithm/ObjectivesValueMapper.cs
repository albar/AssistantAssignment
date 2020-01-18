using EvolutionaryAlgorithm.GeneticAlgorithm.NSGA2;

namespace AssistantAssignment.Algorithm
{
    public class ObjectivesValueMapper :
        IObjectivesValueMapper<Objectives, ObjectivesValue>
    {
        public double GetValue(
            Objectives objective,
            ObjectivesValue objectivesValue)
        {
            return objectivesValue[objective];
        }
    }
}
