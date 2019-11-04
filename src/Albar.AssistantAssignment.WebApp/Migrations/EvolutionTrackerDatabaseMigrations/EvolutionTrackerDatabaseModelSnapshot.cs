﻿// <auto-generated />
using System;
using Albar.AssistantAssignment.WebApp.PopulationTracker;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Albar.AssistantAssignment.WebApp.Migrations.EvolutionTrackerDatabaseMigrations
{
    [DbContext(typeof(EvolutionTrackerDatabase))]
    partial class EvolutionTrackerDatabaseModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.0.0");

            modelBuilder.Entity("Albar.AssistantAssignment.WebApp.PopulationTracker.Model.Chromosome", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("Fitness")
                        .HasColumnType("REAL");

                    b.Property<string>("Genotype")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ObjectiveValues")
                        .HasColumnType("TEXT");

                    b.Property<int>("RunningTaskId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasAlternateKey("RunningTaskId", "Genotype");

                    b.ToTable("Chromosomes");
                });

            modelBuilder.Entity("Albar.AssistantAssignment.WebApp.PopulationTracker.Model.Generation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Number")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("RunningTaskId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("RunningTaskId");

                    b.ToTable("Generations");
                });

            modelBuilder.Entity("Albar.AssistantAssignment.WebApp.PopulationTracker.Model.GenerationChromosome", b =>
                {
                    b.Property<int>("ChromosomeId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GenerationId")
                        .HasColumnType("INTEGER");

                    b.HasKey("ChromosomeId", "GenerationId");

                    b.HasIndex("GenerationId");

                    b.ToTable("GenerationChromosome");
                });

            modelBuilder.Entity("Albar.AssistantAssignment.WebApp.PopulationTracker.Model.RunningTask", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TaskId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("RunningTasks");
                });

            modelBuilder.Entity("Albar.AssistantAssignment.WebApp.PopulationTracker.Model.Chromosome", b =>
                {
                    b.HasOne("Albar.AssistantAssignment.WebApp.PopulationTracker.Model.RunningTask", "RunningTask")
                        .WithMany("Chromosomes")
                        .HasForeignKey("RunningTaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Albar.AssistantAssignment.WebApp.PopulationTracker.Model.Generation", b =>
                {
                    b.HasOne("Albar.AssistantAssignment.WebApp.PopulationTracker.Model.RunningTask", "RunningTask")
                        .WithMany("Generations")
                        .HasForeignKey("RunningTaskId");
                });

            modelBuilder.Entity("Albar.AssistantAssignment.WebApp.PopulationTracker.Model.GenerationChromosome", b =>
                {
                    b.HasOne("Albar.AssistantAssignment.WebApp.PopulationTracker.Model.Chromosome", "Chromosome")
                        .WithMany("GenerationChromosomes")
                        .HasForeignKey("ChromosomeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Albar.AssistantAssignment.WebApp.PopulationTracker.Model.Generation", "Generation")
                        .WithMany("GenerationChromosomes")
                        .HasForeignKey("GenerationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
