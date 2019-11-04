using System.Collections.Generic;
using Albar.AssistantAssignment.ThesisSpecificImplementation;
using Albar.AssistantAssignment.WebApp.PopulationTracker.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Albar.AssistantAssignment.WebApp.PopulationTracker
{
    public class EvolutionTrackerDatabase : DbContext
    {
        public EvolutionTrackerDatabase(DbContextOptions<EvolutionTrackerDatabase> options) : base(options)
        {
        }

        public DbSet<RunningTask> RunningTasks { get; set; }
        public DbSet<Generation> Generations { get; set; }
        public DbSet<Chromosome> Chromosomes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chromosome>()
                .Property(chromosome => chromosome.ObjectiveValues)
                .HasConversion(
                    chromosomes => JsonConvert.SerializeObject(chromosomes),
                    chromosomes =>
                        JsonConvert.DeserializeObject<IReadOnlyDictionary<AssignmentObjective, double>>(chromosomes)
                );

            modelBuilder.Entity<Chromosome>()
                .HasAlternateKey(chromosome => new { Id = chromosome.RunningTaskId, chromosome.Genotype});

            modelBuilder.Entity<RunningTask>()
                .HasMany(task => task.Generations)
                .WithOne(generation => generation.RunningTask);

            modelBuilder.Entity<RunningTask>()
                .HasMany(task => task.Chromosomes)
                .WithOne(chromosome => chromosome.RunningTask)
                .HasForeignKey(chromosome => chromosome.RunningTaskId);

            modelBuilder.Entity<GenerationChromosome>()
                .HasKey(gn => new {gn.ChromosomeId, gn.GenerationId});

            modelBuilder.Entity<GenerationChromosome>()
                .HasOne(gn => gn.Generation)
                .WithMany(generation => generation.GenerationChromosomes)
                .HasForeignKey(gn => gn.GenerationId);
            
            modelBuilder.Entity<GenerationChromosome>()
                .HasOne(gn => gn.Chromosome)
                .WithMany(chromosome => chromosome.GenerationChromosomes)
                .HasForeignKey(gn => gn.ChromosomeId);
        }
    }
}