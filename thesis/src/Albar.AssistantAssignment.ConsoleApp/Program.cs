using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Albar.AssistantAssignment.Abstractions;
using Bunnypro.GeneticAlgorithm.Core;
using Bunnypro.GeneticAlgorithm.MultiObjective.NSGA2;
using Albar.AssistantAssignment.Algorithm;
using Albar.AssistantAssignment.Algorithm.Utilities;
using Albar.AssistantAssignment.ThesisSpecificImplementation;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Factories;
using Albar.AssistantAssignment.ThesisSpecificImplementation.ObjectiveEvaluators;
using Bunnypro.Enumerable.Chunk;
using Bunnypro.GeneticAlgorithm.MultiObjective.Abstractions;
using Bunnypro.GeneticAlgorithm.MultiObjective.Primitives;
using Bunnypro.GeneticAlgorithm.Primitives;
using Microsoft.Extensions.Configuration;

namespace Albar.AssistantAssignment.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
//            Test();
//            return;
            var configuration = BuildConfiguration(args);
            var (capacity, termination) = ExtractValuesConfiguration(configuration);
            var repository = ExtractDataConfiguration();

            // Declaration
            var genotypePhenotypeMapper = new GenotypePhenotypeMapper(repository);
            var reproduction = new AssignmentReproduction<AssignmentObjective>(
                genotypePhenotypeMapper,
                new ReproductionSelection(repository)
            );
            var coefficients = new Dictionary<AssignmentObjective, double>
            {
                {AssignmentObjective.AssistantScheduleCollision, -10d},
                {AssignmentObjective.BelowThresholdAssessment, -1d},
                {AssignmentObjective.AboveThresholdAssessment, 3d},
                {AssignmentObjective.AverageOfNormalizedAssessment, 1d}
            };

            var objectiveEvaluator =
                new AssignmentChromosomesEvaluator<AssignmentObjective>(coefficients)
                {
                    {AssignmentObjective.AssistantScheduleCollision, new AssistantScheduleCollisionEvaluator()},
                    {AssignmentObjective.AboveThresholdAssessment, new AboveThresholdAssessmentEvaluator(repository)},
                    {AssignmentObjective.BelowThresholdAssessment, new BelowThresholdAssessmentEvaluator(repository)},
                    {
                        AssignmentObjective.AverageOfNormalizedAssessment,
                        new AverageOfNormalizedAssessmentEvaluator(repository)
                    }
                };
            var nsga = new NSGA2<AssignmentObjective>(reproduction, objectiveEvaluator, coefficients);
            var ga = new GeneticAlgorithm(nsga);

            // Initialization
            var population = new PopulationFactory<AssignmentObjective>(genotypePhenotypeMapper, objectiveEvaluator)
                .Create(capacity);
//            await objectiveEvaluator.EvaluateAll(population.Chromosomes.Cast<IChromosome<AssignmentObjective>>());

            var result = await ga.EvolveUntil(population, state =>
            {
                if (population.Chromosomes.Cast<AssignmentChromosome<AssignmentObjective>>()
                    .SelectMany(chromosome => chromosome.Phenotype)
                    .Select(representation =>
                        representation.AssistantCombination.Assistants.All(assistant =>
                            repository.Subjects[representation.Schedule.Subject].Assistants.Contains(assistant))).Any(isValid => !isValid))
                {
                    throw new Exception("Not Valid");
                }

//                return population.Chromosomes.Any(c => c.Fitness >= 1);

                return termination(state);
            });
//            var below = repository.Schedules.Length * 25 / 100;
//            var result = await ga.EvolveUntil(population, states =>
//                termination(states) &&
//                population.Chromosomes
//                    .Cast<IAssignmentChromosome<AssignmentObjective>>()
////                    .OrderBy(c => c.Fitness).Last().ObjectiveValues[AssignmentObjective.AssistantScheduleCollision] == 0
//                    .Any(c =>
//                        (int) c.ObjectiveValues[AssignmentObjective.AssistantScheduleCollision] == 0
//                    )
//                || population.Chromosomes
//                    .Cast<IAssignmentChromosome<AssignmentObjective>>()
//                    .Any(c =>
//                        (int) c.ObjectiveValues[AssignmentObjective.AssistantScheduleCollision] == 0
//                        && (int) c.ObjectiveValues[AssignmentObjective.BelowThresholdAssessment] < below
//                    )
//            );

            Console.WriteLine("Evolution Count: {0}", result.EvolutionCount);
            Console.WriteLine("Evolution Time: {0}", result.EvolutionTime);
            Console.WriteLine("Schedule Count {0}", repository.Schedules.Count);
            Console.WriteLine("Chromosomes Count {0}", population.Chromosomes.Count);

            var fronts = new FastNonDominatedSort<AssignmentObjective>(coefficients)
                .Sort(population.Chromosomes.Cast<IChromosome<AssignmentObjective>>())
                .Select(f =>
                    f.Cast<AssignmentChromosome<AssignmentObjective>>().OrderByDescending(s => s.Fitness).ToArray()
                ).ToArray();
            Console.WriteLine("Front Count {0}", fronts.Length);

            var chromosomes = population.Chromosomes.Cast<AssignmentChromosome<AssignmentObjective>>().ToArray();
            var ranges = Enum.GetValues(typeof(AssignmentObjective))
                .Cast<AssignmentObjective>()
                .Select(a =>
                {
                    var sign = coefficients[a];
                    var ordered = chromosomes.Select(c => c.ObjectiveValues[a])
                        .OrderBy(v => v * sign)
                        .ToArray();
                    return $"{a}. Max: {ordered.Last()}. Min: {ordered.First()}";
                });
            foreach (var range in ranges)
            {
                Console.WriteLine(range);
            }

            foreach (var front in fronts)
            {
                Console.WriteLine("Front. Solution Count: {0}", front.Length);
                foreach (var solution in front.OrderByDescending(c => c.Fitness))
                {
                    Console.WriteLine(
                        "\tFitness: {0:0.000}, Objectives: {1}",
                        solution.Fitness,
                        string.Join(", ", solution.ObjectiveValues.Select(o =>
                            $"{o.Value:00.000} ({coefficients[o.Key]})")
                        )
                    );
                }

                Console.WriteLine();
            }

            var best = (AssignmentChromosome<AssignmentObjective>) population.Chromosomes
                .OrderByDescending(c => c.Fitness).First();

            foreach (var assistant in repository.Assistants.Values)
            {
                Console.WriteLine(
                    "Assistant: {0}, Subjects: {1}, Schedules: {2}",
                    assistant.Id,
                    string.Join(",", assistant.Subjects),
                    best.Phenotype.Count(solution =>
                        solution.AssistantCombination.Assistants.Contains(assistant.Id)
                    )
                );
            }

            var solutions = best.Phenotype.Select(s => (ScheduleSolutionRepresentation) s)
                .OrderBy(s => s.Schedule.Day)
                .ThenBy(s => s.Schedule.Session)
                .ThenBy(s => s.Schedule.Lab)
                .ToArray();
            foreach (var solution in solutions)
            {
                Console.WriteLine("Schedule: {0}", solution.Schedule.Id);
                Console.WriteLine("\tSubject: {0}", solution.Schedule.Subject);
                Console.WriteLine("\tDay: {0} Session: {1}", solution.Schedule.Day, solution.Schedule.Session);
                Console.WriteLine("\tAssistants: {0}",
                    string.Join(", ", solution.AssistantCombination.Assistants));
            }
            
            Console.WriteLine("All Valid");
        }

        private static IConfiguration BuildConfiguration(string[] args)
        {
            var map = new Dictionary<string, string>
            {
                {"-h", "schedules"},
                {"-a", "assistants"},
                {"-c", "capacity"},
                {"-t", "time"},
                {"-e", "evolution"}
            };

            return new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddCommandLine(args, map)
                .Build();
        }

        private static (PopulationCapacity, Func<GeneticEvolutionStates, bool>) ExtractValuesConfiguration(
            IConfiguration configuration)
        {
            var capacity = configuration["capacity"].Split(",").Take(2).Select(int.Parse).OrderBy(v => v).ToArray();
            Func<GeneticEvolutionStates, bool> termination = _ => true;
            if (configuration["time"] != null)
            {
                var time = int.Parse(configuration["time"]);
                termination = states => states.EvolutionTime >= TimeSpan.FromSeconds(time);
            }
            else if (configuration["evolution"] != null)
            {
                var evolution = int.Parse(configuration["evolution"]);
                termination = states => states.EvolutionCount >= evolution;
            }


            var populationCapacity = new PopulationCapacity(
                capacity[0], capacity.Length > 1 ? capacity[1] : capacity[0]
            );

            return (populationCapacity, termination);
        }

        private static IDataRepository ExtractDataConfiguration()
        {
            var subjects = DummyDataFactory.CreateSubject(5);
            var schedules = DummyDataFactory.CreateSchedule(subjects.Values.Cast<Subject>());
            var assistants = DummyDataFactory.CreateAssistant(subjects.Values.Cast<Subject>());
            return new DataRepository(
                subjects,
                schedules,
                assistants
            );
        }
    }
}