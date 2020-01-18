using AssistantAssignment.WebApp.Repositories;
using AssistantAssignment.WebApp.Services.GeneticAlgorithmRunnerService.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace AssistantAssignment.WebApp.Controllers
{
    public class CreateRunnerController
    {
        private readonly IGeneticAlgorithmRunnerQueue _queue;
        private readonly IGeneticAlgorithmTaskRepository _tasks;
        private readonly IGeneticAlgorithmRunnerBuilder _builder;

        public CreateRunnerController(IGeneticAlgorithmRunnerQueue queue,
            IGeneticAlgorithmTaskRepository tasks,
            IGeneticAlgorithmRunnerBuilder builder)
        {
            _queue = queue;
            _tasks = tasks;
            _builder = builder;
        }

        [HttpPost("/task/{id}/runner")]
        public JsonResult Create(string id, [FromBody] CreateRunnerRequestBody request)
        {
            var task = _tasks.Find(id);

            _builder.WithSize(request.Size)
                .WithMutation(request.WithMutation);
            // TODO: config runner builder by request

            var runner = _builder.Build();
            task.Runners.Add(runner.Id, runner);
            _queue.Enqueue(runner);
            return new JsonResult(new { runner.Id });
        }

        public class CreateRunnerRequestBody
        {
            public int Size { get; set; }
            public bool WithMutation { get; set; }
        }
    }
}
