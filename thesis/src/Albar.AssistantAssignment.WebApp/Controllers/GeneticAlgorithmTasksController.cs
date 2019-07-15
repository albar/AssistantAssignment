using System;
using System.Collections.Generic;
using Albar.AssistantAssignment.ThesisSpecificImplementation;
using Albar.AssistantAssignment.WebApp.Services;
using Albar.AssistantAssignment.WebApp.Services.GeneticAlgorithm;
using Bunnypro.GeneticAlgorithm.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace Albar.AssistantAssignment.WebApp.Controllers
{
    [Route("api/ga")]
    [ApiController]
    public class GeneticAlgorithmTasksController : ControllerBase
    {
        private readonly IGeneticAlgorithmBackgroundTaskQueue _backgroundTaskQueue;
        private readonly AssignmentDatabase _database;

        public GeneticAlgorithmTasksController(
            IGeneticAlgorithmBackgroundTaskQueue backgroundTaskQueue,
            AssignmentDatabase database)
        {
            _backgroundTaskQueue = backgroundTaskQueue;
            _database = database;
        }

        [HttpGet]
        public JsonResult GetAll()
        {
            return new JsonResult(_backgroundTaskQueue.TaskInfos);
        }

        [HttpPost]
        public JsonResult Register([FromBody] GeneticAlgorithmConfiguration config)
        {
            var group = _database.Groups.Find(config.Group);
            if (group == null) return new JsonResult(NotFound());
            var info = _backgroundTaskQueue.Register(group, config.Coefficients, config.Capacity);
            return new JsonResult(info);
        }

        [HttpDelete("{task}")]
        public JsonResult Remove(Guid task)
        {
            return new JsonResult(_backgroundTaskQueue.Remove(task));
        }

        [HttpPut("{task}")]
        public JsonResult Build([FromBody] Guid task)
        {
            return new JsonResult(_backgroundTaskQueue.Build(task));
        }

        [HttpPost("{task}")]
        public JsonResult Start(Guid task, TerminationKind kind, int value)
        {
            return new JsonResult(_backgroundTaskQueue.Start(task, kind, value));
        }

        [HttpPatch("{task}")]
        public JsonResult Stop([FromBody] Guid task)
        {
            return new JsonResult(_backgroundTaskQueue.Stop(task));
        }

        public class GeneticAlgorithmConfiguration
        {
            public int Group { get; set; }
            public Dictionary<AssignmentObjective, double> Coefficients { get; set; }
            public PopulationCapacity Capacity { get; set; }
        }
    }
}