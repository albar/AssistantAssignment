namespace Albar.AssistantAssignment.WebApp.Services.GeneticAlgorithm
{
    public enum GeneticAlgorithmTaskState
    {
        Registered,
        Building,
        BuildFailed,
        BuildCompleted,
        Starting,
        Finished,
        Running,
        Stopping,
        Stopped
    }
}