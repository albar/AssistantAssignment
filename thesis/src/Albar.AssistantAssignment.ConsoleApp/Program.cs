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
            var configuration = BuildConfiguration(args);
            var (capacity, timeLimit) = ExtractValuesConfiguration(configuration);
            var repository = ExtractDataConfiguration();

            // Declaration
            var genotypePhenotypeMapper = new GenotypePhenotypeMapper(repository);
            var reproduction = new AssignmentReproduction<AssignmentObjective>(
                genotypePhenotypeMapper,
                new ReproductionSelection(genotypePhenotypeMapper)
            );

            var objectiveEvaluator = new AssignmentChromosomesEvaluator<AssignmentObjective>(repository.OptimumValue)
            {
                {AssignmentObjective.ScheduleCollision, new ScheduleCollisionEvaluator()},
                {AssignmentObjective.AboveThresholdAssessment, new AboveThresholdAssessmentEvaluator(repository)},
                {AssignmentObjective.BelowThresholdAssessment, new BelowThresholdAssessmentEvaluator(repository)},
                {AssignmentObjective.AverageOfNormalizedAssessment, new AverageOfNormalizedAssessmentEvaluator()}
            };
            var nsga = new NSGA2<AssignmentObjective>(reproduction, objectiveEvaluator, repository.OptimumValue);
            var ga = new GeneticAlgorithm(nsga);

            // Initialization
            var population = new PopulationFactory<AssignmentObjective>(genotypePhenotypeMapper, objectiveEvaluator)
                .Create(capacity);
            objectiveEvaluator.EvaluateAll(population.Chromosomes.Cast<IChromosome<AssignmentObjective>>());

//            Print(repository);

//            Console.WriteLine("{0}, {1}", capacity.Minimum, capacity.Maximum);
//            // GA Iteration
            var result = await ga.EvolveUntil(population, state => state.EvolutionTime >= timeLimit);

//            foreach (var chromosome in population.Chromosomes)
//            {
//                var ch = (AssignmentChromosome<AssignmentObjective>) chromosome;
//                Console.WriteLine(string.Join(":", ch.Genotype.Select(g => new[] {g}).Select(ByteConverter.ToString)));
//                Console.WriteLine("\tFitness: {0}", ch.Fitness);
//                Console.WriteLine("\tObjective: {0}", string.Join(", ", ch.ObjectiveValues.Select(a => a.Value)));
//            }

            var bestChromosome = (AssignmentChromosome<AssignmentObjective>) population.Chromosomes
                .OrderByDescending(c => c.Fitness).First();
            var bestSolution = genotypePhenotypeMapper.ToSolution(bestChromosome).ToArray();
            
            Console.WriteLine("Evolution Count: {0}", result.EvolutionCount);
            Console.WriteLine("Evolution TimeL {0}", result.EvolutionTime);

            Console.WriteLine("Schedule Count {0}", repository.Schedules.Length);
            foreach (var (objective, value) in bestChromosome.ObjectiveValues)
            {
                Console.WriteLine("{0}: {1}", objective.ToString(), value);
            }

//            var firstFrontSolution = new FastNonDominatedSort<AssignmentObjective>(repository.OptimumValue)
//                .Sort(population.Chromosomes.Cast<IChromosome<AssignmentObjective>>())
//                .First().Cast<AssignmentChromosome<AssignmentObjective>>()
////                .Select(genotypePhenotypeMapper.ToSolution)
//                .ToArray();
////
////            // TODO: Save best chromosome and firstFront to File
//
//            Console.WriteLine("first front count: {0}", firstFrontSolution.Length);
//            foreach (var solution in firstFrontSolution)
//            {
//                Console.WriteLine(solution.Fitness);
//                Console.WriteLine(string.Join(", ", solution.ObjectiveValues.Select(a => a.Value)));
//            }

            var solutions = bestSolution.Select(s => (ScheduleSolutionRepresentation) s)
                .OrderBy(s => s.Schedule.Day)
                .ThenBy(s => s.Schedule.Session)
                .ThenBy(s => s.Schedule.Lab)
                .ToArray();
//            foreach (var solution in solutions)
//            {
//                Console.WriteLine("Schedule: {0}", ByteConverter.ToString(solution.Schedule.Id));
//                Console.WriteLine("\tSubject: {0}", ByteConverter.ToString(solution.Schedule.Subject));
//                Console.WriteLine("\tDay: {0} Session: {1}", solution.Schedule.Day, solution.Schedule.Session);
//                Console.WriteLine("\tAssistants: {0}",
//                    string.Join(", ", solution.AssistantCombination.Assistants.Select(ByteConverter.ToString)));
//            }
            
            foreach (var assistant in repository.Assistants)
            {
                var count = solutions.Count(s => s.AssistantCombination.Assistants.Contains(assistant.Id));
                Console.WriteLine("Assistant {0}: {1}", ByteConverter.ToString(assistant.Id), count);
            }

//            
//            foreach (var solution in firstFrontSolution)
//            {
//                Console.WriteLine(solution.ObjectiveValues[AssignmentObjective.NoCollision]);
//            }
//
//            throw new NotImplementedException();
        }

        private static void Print(IDataRepository<AssignmentObjective> repository)
        {
            foreach (var subject in repository.Subjects.Cast<Subject>())
            {
                Console.WriteLine("Subject Id: {0}", ByteConverter.ToString(subject.Id));
                var schedules = repository.Schedules
                    .Where(h => subject.Schedules.Contains(h.Id))
                    .Cast<Schedule>()
                    .OrderBy(h => h.Day)
                    .ThenBy(h => h.Session)
                    .ThenBy(h => h.Lab);
                Console.WriteLine("Schedules:");
                foreach (var schedule in schedules)
                {
                    Console.WriteLine("\tId: {0}, Day: {1}, Session: {2}, Lab: {3}",
                        ByteConverter.ToString(schedule.Id), schedule.Day, schedule.Session, schedule.Lab);
                }

                var assistants = repository.Assistants.Where(a => subject.Assistants.Contains(a.Id));
                Console.Write("Assistants: ");
                Console.WriteLine(string.Join(", ", assistants.Select(a => ByteConverter.ToString(a.Id))));

                Console.WriteLine("Combination");
                var combinations = repository.AssistantCombinations
                    .Where(c => c.Subject.Equals(subject.Id))
                    .Cast<AssistantCombination>();
                foreach (var combination in combinations)
                {
                    Console.WriteLine("\tId: {0}, Subject: {1}, Combination: {2}",
                        ByteConverter.ToString(combination.Id),
                        ByteConverter.ToString(combination.Subject),
                        string.Join(", ", combination.Assistants.Select(ByteConverter.ToString)));
                }

                Console.WriteLine();
            }

            throw new NotImplementedException();
        }

        private static IConfiguration BuildConfiguration(string[] args)
        {
            var map = new Dictionary<string, string>
            {
                {"-h", "schedules"},
                {"-a", "assistants"},
                {"-c", "capacity"},
                {"-t", "time"},
                // other arguments
            };

            return new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddCommandLine(args, map)
                .Build();
        }

        private static (PopulationCapacity, TimeSpan) ExtractValuesConfiguration(IConfiguration configuration)
        {
            var capacity = configuration["capacity"].Split(",").Take(2).Select(int.Parse).OrderBy(v => v).ToArray();
            var time = int.Parse(configuration["time"]);

            var populationCapacity = new PopulationCapacity(
                capacity[0], capacity.Length > 1 ? capacity[1] : capacity[0]
            );
            var timeLimit = TimeSpan.FromSeconds(time);

            return (populationCapacity, timeLimit);
        }

        private static IDataRepository<AssignmentObjective> ExtractDataConfiguration()
        {
            var subjects = DummyDataFactory.CreateSubject(5);
            var schedules = DummyDataFactory.CreateSchedule(subjects.Cast<Subject>());
            var assistants = DummyDataFactory.CreateAssistant(subjects.Cast<Subject>());
            var optimum = new Dictionary<AssignmentObjective, OptimumValue>
            {
                {AssignmentObjective.ScheduleCollision, OptimumValue.Minimum},
                {AssignmentObjective.AboveThresholdAssessment, OptimumValue.Maximum},
                {AssignmentObjective.BelowThresholdAssessment, OptimumValue.Minimum},
                {AssignmentObjective.AverageOfNormalizedAssessment, OptimumValue.Maximum}
            };
            return new DataRepository(
                subjects.ToImmutableArray(),
                schedules.ToImmutableArray(),
                assistants.ToImmutableArray(),
                optimum
            );
        }
    }
}