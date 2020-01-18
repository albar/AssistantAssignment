﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EvolutionaryAlgorithm.GeneticAlgorithm.NSGA2;
using EvolutionaryAlgorithm.GeneticAlgorithm.NSGA2.OffspringSelectors;
using Microsoft.EntityFrameworkCore;
using Thesis.Algorithm;
using Thesis.Algorithm.ObjectiveValueCalculators;
using Thesis.Algorithm.Reproductions;
using Thesis.Database;
using Thesis.DatabaseDataRepositoryBuilder;
using Thesis.DataType;

namespace Thesis.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (!int.TryParse(args.ElementAtOrDefault(0), out var population))
            {
                population = 28;
            }
            if (!int.TryParse(args.ElementAtOrDefault(1), out var evolution))
            {
                evolution = 50;
            }
            if (!int.TryParse(args.ElementAtOrDefault(2), out var db))
            {
                db = 1;
            }

            Console.WriteLine("Preparing ..");
            var preparationStartTime = DateTime.Now;
            var repository = await BuildRepositoryAsync(db);
            var crossover = new Crossover(repository);
            var mutation = new Mutation(repository);
            var resolver = new PhenotypeResolver(repository);
            var calculators = new ObjectiveValueCalculatorBase[]
            {
                new SchedulesCollisionCalculator(repository),
                new AboveAverageCalculator(repository),
                new AboveThresholdCalculator(repository),
                new NormalizedAssessmentsValuesCalculator(),
            }.ToImmutableHashSet();
            var evaluator = new ChromosomeEvaluator(resolver, calculators);
            var selector = new EuclideanAllOffspringSelector<Chromosome, Objectives, ObjectivesValue>(
                Enum.GetValues(typeof(Objectives)).Cast<Objectives>(),
                new ObjectivesValueMapper());
            var reinsertion = new NSGAReinsertion<Chromosome, ObjectivesValue>(
                ObjectivesValue.ObjectivesValueComparer, selector);
            var factory = new ChromosomeFactory(repository);
            var ga = new NSGA2<Chromosome, Objectives>(
                crossover,
                mutation,
                evaluator,
                reinsertion);
            Console.WriteLine($"Preparation Time = {DateTime.Now - preparationStartTime}");

            var initializationstartTime = DateTime.Now;
            var chromosomes = await factory.CreateAsync(population, default);
            await evaluator.EvaluateAsync(chromosomes, default);
            Console.WriteLine($"Initialization Time = {DateTime.Now - initializationstartTime}");

            var existenceCount = chromosomes.ToDictionary(
                chromosome => chromosome.GetHashCode(),
                Chromosome => 1);

            var count = 0;
            var cts = new CancellationTokenSource();
            var prefix = "Evolution Count =";
            Console.Write(prefix);
            ga.OnEvolvedOnce += value =>
            {
                foreach (var chromosome in value)
                {
                    if (!existenceCount.TryAdd(chromosome.GetHashCode(), 1))
                    {
                        existenceCount[chromosome.GetHashCode()]++;
                    }
                }
                count++;
                if (count >= evolution)
                {
                    cts.Cancel();
                }
                Console.Write($" {count}");
                Console.CursorLeft = prefix.Length;
            };

            var evolutionStartTime = DateTime.Now;
            var result = await ga.EvolveAsync(chromosomes, cts.Token);
            var evolutionTime = DateTime.Now - evolutionStartTime;
            Console.WriteLine();
            Console.WriteLine($"Schedule Count = {repository.Schedules.Length}");
            await evaluator.EvaluateAsync(result, default);
            Console.WriteLine($"Evolution Time = {evolutionTime}");
            Console.WriteLine($"Unique Chromosome = {existenceCount.Count}");
            var fronts = FastNondominatedSorter.Sort(result, new Comparer());

            var fronCount = 0;
            foreach (var front in fronts)
            {
                fronCount++;
                Console.WriteLine($"\nFront {fronCount}");
                foreach (var chromosome in front.OrderByDescending(chr => chr.Fitness.Values.Average()))
                {
                    Console.Write($"Exists Count = {existenceCount[chromosome.GetHashCode()]}, ");
                    Console.Write("Original = ");
                    Console.Write(string.Join(", ", chromosome.OriginalObjectivesValue.Values));
                    Console.Write(". Normalized = ");
                    Console.WriteLine(string.Join(", ", chromosome.Fitness.Values));
                }
            }
            Console.ReadKey();
        }

        private static async Task<IDataRepository> BuildRepositoryAsync(int db)
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlite("Data Source=assignment.db")
                .Options;
            await using var database = new DatabaseContext(options);
            return await DataRepositoryBuilder.CreateDataRepositoryAsync(database, db, default);
        }

        private class Comparer : IComparer<Chromosome>
        {
            public int Compare([AllowNull] Chromosome x, [AllowNull] Chromosome y)
            {
                return x.Fitness.CompareTo(y.Fitness);
            }
        }
    }
}
