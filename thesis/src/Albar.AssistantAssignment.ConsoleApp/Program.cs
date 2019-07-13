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

            var objectiveEvaluator =
                new AssignmentChromosomesEvaluator<AssignmentObjective>(repository.ObjectiveCoefficients)
                {
                    {AssignmentObjective.AssistantScheduleCollision, new AssistantScheduleCollisionEvaluator()},
                    {AssignmentObjective.AboveThresholdAssessment, new AboveThresholdAssessmentEvaluator()},
                    {AssignmentObjective.BelowThresholdAssessment, new BelowThresholdAssessmentEvaluator()},
                    {
                        AssignmentObjective.AverageOfNormalizedAssessment,
                        new AverageOfNormalizedAssessmentEvaluator(repository)
                    }
                };
            var nsga = new NSGA2<AssignmentObjective>(reproduction, objectiveEvaluator,
                repository.ObjectiveCoefficients);
            var ga = new GeneticAlgorithm(nsga);

            // Initialization
            var population = new PopulationFactory<AssignmentObjective>(genotypePhenotypeMapper, objectiveEvaluator)
                .Create(capacity);
            await objectiveEvaluator.EvaluateAll(population.Chromosomes.Cast<IChromosome<AssignmentObjective>>());

//            Print(repository);

//            Console.WriteLine("{0}, {1}", capacity.Minimum, capacity.Maximum);
//            // GA Iteration

            var result = await ga.EvolveUntil(population, termination);
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

//            foreach (var chromosome in population.Chromosomes)
//            {
//                var ch = (AssignmentChromosome<AssignmentObjective>) chromosome;
//                Console.WriteLine(string.Join(":", ch.Genotype.Select(g => new[] {g}).Select(ByteConverter.ToString)));
//                Console.WriteLine("\tFitness: {0}", ch.Fitness);
//                Console.WriteLine("\tObjective: {0}", string.Join(", ", ch.ObjectiveValues.Select(a => a.Value)));
//            }

//            var bestChromosome = (AssignmentChromosome<AssignmentObjective>) population.Chromosomes
//                .OrderByDescending(c => c.Fitness).First();
//            var bestSolution = genotypePhenotypeMapper.ToSolution(bestChromosome).ToArray();

            Console.WriteLine("Evolution Count: {0}", result.EvolutionCount);
            Console.WriteLine("Evolution Time: {0}", result.EvolutionTime);
            Console.WriteLine("Schedule Count {0}", repository.Schedules.Length);
            Console.WriteLine("Chromosomes Count {0}", population.Chromosomes.Count);
//            foreach (var (objective, value) in bestChromosome.ObjectiveValues)
//            {
//                Console.WriteLine("{0}: {1}", objective.ToString(), value);
//            }

//            objectiveEvaluator.EvaluateAll(population.Chromosomes.Cast<IChromosome<AssignmentObjective>>());
            var fronts = new FastNonDominatedSort<AssignmentObjective>(repository.ObjectiveCoefficients)
                .Sort(population.Chromosomes.Cast<IChromosome<AssignmentObjective>>())
                .Select(f =>
                    f.Cast<AssignmentChromosome<AssignmentObjective>>().OrderByDescending(s => s.Fitness).ToArray()
                ).ToArray();
            Console.WriteLine("Front Count {0}", fronts.Length);
////
////            // TODO: Save best chromosome and firstFront to File
//
//            Console.WriteLine("first front count: {0}", firstFrontSolution.Length);
//            foreach (var solution in firstFrontSolution)
//            {
//                Console.WriteLine(solution.Fitness);
//                Console.WriteLine(string.Join(", ", solution.ObjectiveValues.Select(a => a.Value)));
//            }

//            foreach (var assistant in repository.Assistants)
//            {
//                var count = solutions.Count(s => s.AssistantCombination.Assistants.Contains(assistant.Id));
//                Console.WriteLine("Assistant {0}: {1}", ByteConverter.ToString(assistant.Id), count);
//            }

//
            var chromosomes = population.Chromosomes.Cast<AssignmentChromosome<AssignmentObjective>>().ToArray();
            var ranges = Enum.GetValues(typeof(AssignmentObjective))
                .Cast<AssignmentObjective>()
                .Select(a =>
                {
                    var sign = repository.ObjectiveCoefficients[a];
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
                            $"{o.Value:00.000} ({repository.ObjectiveCoefficients[o.Key]})")
                        )
                    );
                }

                Console.WriteLine();
            }

            var best = (AssignmentChromosome<AssignmentObjective>) population.Chromosomes
                .OrderByDescending(c => c.Fitness).First();

            foreach (var assistant in repository.Assistants)
            {
                Console.WriteLine(
                    "Assistant: {0}, Subjects: {1}, Schedules: {2}",
                    assistant.Id,
                    string.Join(",", assistant.Subjects.Select(s => s.Id)),
                    best.Phenotype.Count(solution =>
                        solution.AssistantCombination.Assistants.Contains(assistant)
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
                Console.WriteLine("\tSubject: {0}", solution.Schedule.Subject.Id);
                Console.WriteLine("\tDay: {0} Session: {1}", solution.Schedule.Day, solution.Schedule.Session);
                Console.WriteLine("\tAssistants: {0}",
                    string.Join(", ", solution.AssistantCombination.Assistants.Select(a => a.Id)));
            }

//
//            throw new NotImplementedException();
        }

        private static void Test()
        {
            var sorter = new FastNonDominatedSort<Fuck>(new Dictionary<Fuck, double>
            {
                {Fuck.A, 1d},
                {Fuck.B, 1d},
                {Fuck.C, 1d},
            });
            var items = new[]
            {
                new Dictionary<Fuck, double> {{Fuck.A, 3}, {Fuck.B, 2}, {Fuck.C, 4}},
                new Dictionary<Fuck, double> {{Fuck.A, 1}, {Fuck.B, 4}, {Fuck.C, 4}},
                new Dictionary<Fuck, double> {{Fuck.A, 2}, {Fuck.B, 2}, {Fuck.C, 1}},
                new Dictionary<Fuck, double> {{Fuck.A, 1}, {Fuck.B, 1}, {Fuck.C, 4}},
                new Dictionary<Fuck, double> {{Fuck.A, 1}, {Fuck.B, 1}, {Fuck.C, 1}}
            }.Select((v, i) => new AssignmentChromosome<Fuck>(new[] {(byte) i}.ToImmutableArray())
            {
                ObjectiveValues = new ObjectiveValues<Fuck>(v)
            });
            foreach (var front in sorter.Sort(items))
            {
                Console.WriteLine("Front");
                foreach (var item in front)
                {
                    Console.WriteLine("\tItem");
                    foreach (var (objective, value) in item.ObjectiveValues)
                    {
                        Console.WriteLine("\t\t{0}: {1}", objective, value);
                    }
                }
            }
        }

        private enum Fuck
        {
            A,
            B,
            C
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
                // other arguments
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

        private static IDataRepository<AssignmentObjective> ExtractDataConfiguration()
        {
            var subjects = DummyDataFactory.CreateSubject(5);
            var schedules = DummyDataFactory.CreateSchedule(subjects.Cast<Subject>());
            var assistants = DummyDataFactory.CreateAssistant(subjects.Cast<Subject>());
//            var optimum = new Dictionary<AssignmentObjective, OptimumValue>
//            {
//                {AssignmentObjective.AssistantScheduleCollision, OptimumValue.Minimum},
//                {AssignmentObjective.BelowThresholdAssessment, OptimumValue.Minimum},
//                {AssignmentObjective.AboveThresholdAssessment, OptimumValue.Maximum},
//                {AssignmentObjective.AverageOfNormalizedAssessment, OptimumValue.Maximum}
//            };
            var coefficient = new Dictionary<AssignmentObjective, double>
            {
                {AssignmentObjective.AssistantScheduleCollision, -10d},
                {AssignmentObjective.BelowThresholdAssessment, -1d},
                {AssignmentObjective.AboveThresholdAssessment, 3d},
                {AssignmentObjective.AverageOfNormalizedAssessment, 1d}
            };
            return new DataRepository(
                subjects.ToImmutableArray(),
                schedules.ToImmutableArray(),
                assistants.ToImmutableArray(),
//                optimum,
                coefficient
            );
        }
    }
}
