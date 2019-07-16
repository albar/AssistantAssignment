using System;
using System.Collections.Generic;
using System.Linq;
using Albar.AssistantAssignment.ThesisSpecificImplementation;
using Albar.AssistantAssignment.WebApp.Services.ParallelGeneticAlgorithm;
using Bunnypro.GeneticAlgorithm.Primitives;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Albar.AssistantAssignment.WebApp.Controllers
{
    [Route("/api/ga")]
    [ApiController]
    public class GeneticAlgorithmController : ControllerBase
    {
        private readonly AssignmentDatabase _database;
        private readonly IGeneticAlgorithmBackgroundTaskQueue _queue;
        private readonly ILogger<GeneticAlgorithmController> _logger;

        public GeneticAlgorithmController(
            AssignmentDatabase database,
            IGeneticAlgorithmBackgroundTaskQueue queue,
            ILogger<GeneticAlgorithmController> logger)
        {
            _database = database;
            _queue = queue;
            _logger = logger;
        }

        [HttpGet]
        public JsonResult GetAll()
        {
            var tasks = _queue.Tasks.Select(task => new
            {
                task.Id,
                task.Group,
                State = task.State.ToString(),
                task.IsRunning,
                task.Coefficients,
                task.Capacity,
                task.EvolutionState,
                SubjectCount = task.Repository.Subjects.Length,
                ScheduleCount = task.Repository.Schedules.Length,
                AssistantCount = task.Repository.Assistants.Length
            }).ToArray();
            
            return new JsonResult(tasks);
        }

        [HttpPost]
        public IActionResult Build([FromBody] BuildBody body)
        {
            var dataGroup = _database.Groups.FirstOrDefault(g => g.Id == body.Group);
            if (dataGroup == null) return NotFound();
            var populationCapacity = new PopulationCapacity(
                Math.Min(body.Capacity.Min, body.Capacity.Max),
                Math.Max(body.Capacity.Min, body.Capacity.Max)
            );
            var task = _queue.Build(dataGroup, body.Coefficients, populationCapacity);
            var result = new
            {
                task.Id,
                task.Group,
                State = task.State.ToString(),
                task.IsRunning,
                task.Coefficients,
                task.Capacity,
                task.EvolutionState,
            };
            return new JsonResult(result);
        }
        
        public class BuildBody
        {
            public int Group { get; set; }
            public Dictionary<AssignmentObjective, double> Coefficients { get; set; }
            public Capacity Capacity { get; set; }
        }

        [HttpDelete("{task}")]
        public IActionResult Remove(string task)
        {
            var exists = _queue.Remove(task);
            if (!exists) return NotFound();
            return new JsonResult(true);
        }

        [HttpPost("{task}")]
        public IActionResult Start(string task, StartConfiguration config)
        {
            _logger.LogInformation($"Termination Kind: {config.Kind.ToString()}");
            var exists = _queue.Start(task, states =>
            {
                switch (config.Kind)
                {
                    case TerminationKind.EvolutionTimeTermination:
                        return states.EvolutionTime >= TimeSpan.FromSeconds(config.Value);
                    case TerminationKind.EvolutionCountTermination:
                        return states.EvolutionCount >= config.Value;
                    case TerminationKind.NoTermination:
                    default:
                        return false;
                }
            });

            if (!exists) return NotFound();
            return new JsonResult(true);
        }
        
        public class StartConfiguration
        {
            public TerminationKind Kind { get; set; }
            public int Value { get; set; }
        }
        
        [HttpPatch("{task}")]
        public IActionResult Stop(string task)
        {
            var exists = _queue.Stop(task);
            if (!exists) return NotFound();
            return new JsonResult(true);
        }

        public class Capacity
        {
            public int Min { get; set; }
            public int Max { get; set; }
        }

        public enum TerminationKind
        {
            NoTermination,
            EvolutionTimeTermination,
            EvolutionCountTermination
        }
    }
}