using System.Collections.Generic;
using System.Linq;
using Albar.AssistantAssignment.Algorithm;
using Albar.AssistantAssignment.ThesisSpecificImplementation;
using Albar.AssistantAssignment.WebApp.Services.ParallelGeneticAlgorithm;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Albar.AssistantAssignment.WebApp.Controllers
{
    [Route("api/ga/{task}/result")]
    [ApiController]
    public class GeneticAlgorithmResultController : ControllerBase
    {
        private readonly AssignmentDatabase _database;
        private readonly IGeneticAlgorithmBackgroundTaskQueue _backgroundTask;
        private readonly ILogger<GeneticAlgorithmResultController> _logger;

        public GeneticAlgorithmResultController(
            AssignmentDatabase database,
            IGeneticAlgorithmBackgroundTaskQueue backgroundTask,
            ILogger<GeneticAlgorithmResultController> logger)
        {
            _database = database;
            _backgroundTask = backgroundTask;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Chromosomes(string task)
        {
            var selectedTask = _backgroundTask.Tasks.FirstOrDefault(t => t.Id == task);
            if (selectedTask == null) return NotFound();
            var result = selectedTask.Population.Chromosomes
                .Cast<AssignmentChromosome<AssignmentObjective>>()
                .OrderByDescending(chromosome => chromosome.Fitness)
                .Select((chromosome, id) =>
                {
                    var values = new Dictionary<string, double>
                    {
                        {"Fitness", chromosome.Fitness}
                    };
                    foreach (var (objective, value) in chromosome.ObjectiveValues)
                    {
                        values.Add(objective.ToString(), value);
                    }
                    return new
                    {
                        Id = id,
                        Values = values,
                        Solutions = chromosome.Phenotype.Select(solution => new
                            {
                                solution.Schedule.Id,
                                Subject = new
                                {
                                    solution.Schedule.Subject.Id,
                                    solution.Schedule.Subject.Code
                                },
                                Schedule = new
                                {
                                    Day = solution.Schedule.Day.ToString(),
                                    Session = solution.Schedule.Session.ToString(),
                                    solution.Schedule.Lab
                                },
                                Assistants = solution.AssistantCombination.Assistants.Select(a => a.Npm).ToArray()
                            })
                            .OrderBy(solution => solution.Subject.Id)
                            .ThenBy(solution => solution.Schedule.Day)
                            .ThenBy(solution => solution.Schedule.Session)
                            .ToArray()
                    };
                }).ToArray();
            return new JsonResult(result);
        }
    }
}