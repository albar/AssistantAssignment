using System.Collections.Generic;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;
using Albar.AssistantAssignment.WebApp.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Assistant = Albar.AssistantAssignment.WebApp.Models.Assistant;
using Schedule = Albar.AssistantAssignment.WebApp.Models.Schedule;
using Subject = Albar.AssistantAssignment.WebApp.Models.Subject;

namespace Albar.AssistantAssignment.WebApp
{
    public class AssignmentDatabase : DbContext
    {
        public DbSet<Group> Groups { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Assistant> Assistants { get; set; }

        public AssignmentDatabase(DbContextOptions<AssignmentDatabase> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subject>()
                .Property(subject => subject.AssessmentsThreshold)
                .HasConversion(
                    threshold => JsonConvert.SerializeObject(threshold),
                    threshold => JsonConvert.DeserializeObject<Dictionary<AssistantAssessment, double>>(threshold)
                );

            modelBuilder.Entity<Assistant>()
                .Property(assistant => assistant.Npm)
                .HasConversion(
                    npm => npm.ToString(),
                    npm => new Npm(npm)
                );

            modelBuilder.Entity<AssistantSubject>()
                .HasKey(ass => new {ass.AssistantId, ass.SubjectId});

            modelBuilder.Entity<AssistantSubject>()
                .Property(ass => ass.Assessments)
                .HasConversion(
                    assessments => JsonConvert.SerializeObject(assessments),
                    str => JsonConvert.DeserializeObject<Dictionary<AssistantAssessment, double>>(str)
                );

            modelBuilder.Entity<AssistantSubject>()
                .HasOne(ass => ass.Assistant)
                .WithMany(assistant => assistant.AssistantSubjects)
                .HasForeignKey(ass => ass.AssistantId);

            modelBuilder.Entity<AssistantSubject>()
                .HasOne(ass => ass.Subject)
                .WithMany(subject => subject.AssistantSubjects)
                .HasForeignKey(ass => ass.SubjectId);
        }
    }
}