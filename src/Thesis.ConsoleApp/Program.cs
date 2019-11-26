using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EvolutionaryAlgorithm.GeneticAlgorithm.NSGA2;
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
            int population;
            int evolution;
            int db;
            if (!int.TryParse(args.ElementAtOrDefault(0), out population))
            {
                population = 28;
            }
            if (!int.TryParse(args.ElementAtOrDefault(1), out evolution))
            {
                evolution = 50;
            }
            if (!int.TryParse(args.ElementAtOrDefault(2), out db))
            {
                db = 1;
            }

            var repository = await BuildRepositoryAsync(db);
            var crossover = new Crossover(repository);
            var mutation = new Mutation(repository);
            var resolver = new PhenotypeResolver(repository);
            var calculators = new ObjectiveValueCalculatorBase[]
            {
                new SchedulesCollisionCalculator(repository),
                new NormalizedAssessmentsValuesCalculator(),
            }.ToImmutableHashSet();
            var evaluator = new ChromosomeEvaluator(resolver, calculators);
            var reinsertion = new EuclideanBasedOffspringSelector<Chromosome, Objectives, ObjectivesValue>(
                Enum.GetValues(typeof(Objectives)).Cast<Objectives>(),
                new ObjectivesValueMapper(),
                ObjectivesValue.ObjectivesValueComparer);
            var factory = new ChromosomeFactory(repository);
            var ga = new NSGA2<Chromosome, Objectives>(
                crossover,
                mutation,
                evaluator,
                reinsertion);
            var chromosomes = await factory.CreateAsync(population, default);
            await evaluator.EvaluateAsync(chromosomes, default);

            var count = 0;
            var startTime = DateTime.Now;

            var cts = new CancellationTokenSource();
            ga.OnEvolvedOnce += value =>
            {
                if (count >= evolution - 1)
                {
                    cts.Cancel();
                }
                count++;
            };

            var result = await ga.EvolveAsync(chromosomes, cts.Token);

            var duplicated = result.Count(chromosome =>
                result.Count(other => chromosome.Genotype.SequenceEqual(other.Genotype)) > 1);
            Console.WriteLine($"Contain Duplicate = {duplicated}");
            Console.WriteLine($"Schedule Count = {repository.Schedules.Length}");
            await evaluator.EvaluateAsync(result, default);
            Console.WriteLine($"Evolution Count = {count}");
            Console.WriteLine($"Evolution Time = {DateTime.Now - startTime}");
            var fronts = FastNondominatedSorter.Sort(result, new Comparer());
            foreach (var front in fronts)
            {
                Console.WriteLine("Front");
                foreach (var chromosome in front.OrderByDescending(chr => chr.Fitness.Values.Average()))
                {
                    Console.Write("Original = ");
                    Console.Write(string.Join(", ", chromosome.OriginalObjectivesValue.Values));
                    Console.Write(". Normalized = ");
                    Console.WriteLine(string.Join(", ", chromosome.Fitness.Values));
                }
                Console.WriteLine();
            }
            Console.ReadKey();
        }

        private static void Print(IEnumerable<Chromosome> chromosomes)
        {
            Console.Clear();
            foreach (var chromosome in chromosomes)
            {
                Console.Write("Original = ");
                Console.Write(string.Join(", ", chromosome.OriginalObjectivesValue.Values));
                Console.Write(". Normalized = ");
                Console.WriteLine(string.Join(", ", chromosome.Fitness.Values));
            }
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
