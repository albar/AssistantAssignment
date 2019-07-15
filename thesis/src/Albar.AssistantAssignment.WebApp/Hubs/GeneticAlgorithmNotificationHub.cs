using Albar.AssistantAssignment.WebApp.Services;
using Albar.AssistantAssignment.WebApp.Services.GeneticAlgorithm;
using Microsoft.AspNetCore.SignalR;

namespace Albar.AssistantAssignment.WebApp.Hubs
{
    public class GeneticAlgorithmNotificationHub : Hub<IGeneticAlgorithmTaskListener>
    {
    }
}