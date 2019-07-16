namespace Albar.AssistantAssignment.WebApp.Services.ParallelGeneticAlgorithm
{
    public enum GeneticAlgorithmTaskState
    {
        Unknown,
        BuildingTask,
        BuildingRepositoryTask,
        TaskBuildFinished,
        TaskRemoved,
        RunningTask,
        TaskIsRunning,
        EvolvedOnce,
        StoppingTask,
        TaskFinished,
    }
}