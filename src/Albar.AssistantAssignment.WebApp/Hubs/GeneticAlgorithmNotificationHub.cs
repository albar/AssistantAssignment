using Albar.AssistantAssignment.WebApp.Services.ParallelGeneticAlgorithm;
using Microsoft.AspNetCore.SignalR;

namespace Albar.AssistantAssignment.WebApp.Hubs
{
    public class GeneticAlgorithmNotificationHub : Hub<IGeneticAlgorithmTaskListener>
    {
    }
}