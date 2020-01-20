using AssistantAssignment.Web.Api.Repositories;
using AssistantAssignment.Web.Api.Services.GeneticAlgorithmRunnerService.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace AssistantAssignment.Web.Api.Controllers
{
    public class CreateRunnerController
    {
        private readonly IGeneticAlgorithmRunnerQueue _queue;
        private readonly IGeneticAlgorithmTaskRepository _tasks;

        public CreateRunnerController(
            IGeneticAlgorithmRunnerQueue queue,
            IGeneticAlgorithmTaskRepository tasks)
        {
            _queue = queue;
            _tasks = tasks;
        }

        [HttpPost("/task/{id}/runner")]
        public JsonResult Create(string id, [FromBody] RunnerCreationOption option)
        {
            var task = _tasks.Find(id);

            var builder = task.CreateRunnerBuilder()
                .WithSize(option.Size)
                .WithMutation(option.Mutation);

            var runner = task.AddRunnerBuilder(builder);
            _queue.Enqueue(runner);
            return new JsonResult(new { runner.Id });
        }

        public class RunnerCreationOption
        {
            public int Size { get; set; }
            public bool Mutation { get; set; }
        }
    }
}
