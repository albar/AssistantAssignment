using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.Algorithm;
using Albar.AssistantAssignment.Algorithm.Utilities;
using Albar.AssistantAssignment.ThesisSpecificImplementation;
using Albar.AssistantAssignment.WebApp.PopulationTracker.Model;
using Albar.AssistantAssignment.WebApp.Services.GenericBackgroundTask;
using Bunnypro.GeneticAlgorithm.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Albar.AssistantAssignment.WebApp.PopulationTracker
{
    public class EvolutionTracker : IPopulationEventHandler
    {
        private readonly string _id;
        private readonly IGenericBackgroundTaskQueue _queue;

        public EvolutionTracker(string id, IGenericBackgroundTaskQueue queue)
        {
            _id = id;
            _queue = queue;
            CreateRunningTask();
        }

        private void CreateRunningTask()
        {
            DatabaseAction(async db =>
            {
                await db.RunningTasks.AddAsync(new RunningTask
                {
                    TaskId = _id
                });
            });
        }

        public void OnChromosomesUpdated(ImmutableHashSet<IChromosome> chromosomes, int generationNumber)
        {
            DatabaseAction(async db =>
            {
                var keyedChromosomes = chromosomes.Cast<AssignmentChromosome<AssignmentObjective>>().ToDictionary(
                    chromosome => ByteConverter.ToString(chromosome.Genotype.ToArray()),
                    chromosome => chromosome
                );

                var keys = keyedChromosomes.Keys;

                var runningTask = await db.RunningTasks
                    .Include(rt => rt.Chromosomes)
                    .Include(rt => rt.Generations)
                    .FirstOrDefaultAsync(task => task.TaskId == _id);
                var existed = runningTask.Chromosomes.Select(chromosome => chromosome.Genotype)
                    .Where(genotype => keys.Contains(genotype))
                    .ToArray();
                var generation = new Generation
                {
                    Number = generationNumber
                };
                generation.GenerationChromosomes = keyedChromosomes.Where(kc => !existed.Contains(kc.Key))
                    .Select(kc => new GenerationChromosome
                    {
                        Chromosome = new Chromosome
                        {
                            RunningTask = runningTask,
                            Genotype = kc.Key,
                            Fitness = kc.Value.Fitness,
                            ObjectiveValues = kc.Value.ObjectiveValues
                        },
                        Generation = generation
                    }).ToList();
                runningTask.Generations.Add(generation);
            });
        }

        private void DatabaseAction(Func<EvolutionTrackerDatabase, Task> action)
        {
            _queue.Enqueue(async (provider, token) =>
            {
                await using var database = provider.GetRequiredService<EvolutionTrackerDatabase>();
                await action.Invoke(database);
                await database.SaveChangesAsync(token);
            });
        }
    }
}