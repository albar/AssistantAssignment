using AssistantAssignment.WebApp.Services.GeneticAlgorithmBuilderService.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace AssistantAssignment.WebApp.Controllers
{
    public class CreateTaskController
    {
        private readonly IGeneticAlgorithmBuilderQueue _queue;

        public CreateTaskController(IGeneticAlgorithmBuilderQueue queue)
        {
            _queue = queue;
        }

        [HttpPost("/task")]
        public JsonResult Create([FromBody] CreateTaskRequest request)
        {
            string id = _queue.EnqueueBuilder((IGeneticAlgorithmBuilder builder) =>
            {
                builder.WithDataId(request.DataId);
            });

            return new JsonResult(new { id });
        }

        public class CreateTaskRequest
        {
            public int DataId { get; set; }
        }
    }
}
