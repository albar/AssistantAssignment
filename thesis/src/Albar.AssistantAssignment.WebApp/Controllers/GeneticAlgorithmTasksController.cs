using System;
using System.Collections.Generic;
using System.Linq;
using Albar.AssistantAssignment.ThesisSpecificImplementation;
using Albar.AssistantAssignment.WebApp.Services;
using Albar.AssistantAssignment.WebApp.Services.GeneticAlgorithm;
using Bunnypro.GeneticAlgorithm.Primitives;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Albar.AssistantAssignment.WebApp.Controllers
{
    [Route("api/ga")]
    [ApiController]
    public class GeneticAlgorithmTasksController : ControllerBase
    {
        private readonly IGeneticAlgorithmBackgroundTaskQueue _backgroundTaskQueue;
        private readonly AssignmentDatabase _database;
        private readonly ILogger<GeneticAlgorithmTasksController> _logger;

        public GeneticAlgorithmTasksController(
            IGeneticAlgorithmBackgroundTaskQueue backgroundTaskQueue,
            AssignmentDatabase database,
            ILogger<GeneticAlgorithmTasksController> logger)
        {
            _backgroundTaskQueue = backgroundTaskQueue;
            _database = database;
            _logger = logger;
        }

        [HttpGet]
        public JsonResult GetAll()
        {
            return new JsonResult(_backgroundTaskQueue.TaskInfos.Select(info => new
            {
                info.Id,
                info.Group,
                info.Coefficients,
                info.Capacity,
                State = info.State.ToString()
            }));
        }

        [HttpPost]
        public IActionResult Register([FromBody] GeneticAlgorithmConfiguration config)
        {
//            _logger.LogInformation($"Group: {config.Group}");
//            _logger.LogInformation($"Coefficient: {string.Join(";", config.Coefficients.Select(c => $"{c.Key}({c.Value})"))}");
//            _logger.LogInformation($"Capacity: Min ({config.Min}), Max ({config.Max})");

            var capacity = new PopulationCapacity(config.Min, config.Max);
            var group = _database.Groups.Find(config.Group);
            if (group == null) return NotFound();
            var info = _backgroundTaskQueue.Register(group, config.Coefficients, capacity);
            return new JsonResult(info);
        }

        [HttpDelete("{task}")]
        public JsonResult Remove(string task)
        {
            return new JsonResult(_backgroundTaskQueue.Remove(task));
        }

        [HttpPut("{task}")]
        public JsonResult Build(string task)
        {
            return new JsonResult(_backgroundTaskQueue.Build(task));
        }

        [HttpPost("{task}")]
        public JsonResult Start(string task, TerminationKind kind, int value)
        {
            return new JsonResult(_backgroundTaskQueue.Start(task, kind, value));
        }

        [HttpPatch("{task}")]
        public JsonResult Stop(string task)
        {
            return new JsonResult(_backgroundTaskQueue.Stop(task));
        }

        public class GeneticAlgorithmConfiguration
        {
            public int Group { get; set; }
            public Dictionary<AssignmentObjective, double> Coefficients { get; set; }
            public int Min { get; set; }
            public int Max { get; set; }
        }
    }
}