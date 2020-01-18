using System.Collections.Generic;
using AssistantAssignment.Data.Database.Entity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AssistantAssignment.Data.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Group> Groups { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Assistant> Assistants { get; set; }

        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subject>()
                .Property(subject => subject.AssessmentsThreshold)
                .HasConversion(
                    threshold => JsonConvert.SerializeObject(threshold),
                    threshold => JsonConvert.DeserializeObject<Dictionary<AssistantAssignment.Data.Types.Assesments, double>>(threshold)
                );

            modelBuilder.Entity<Entity.Assistant>()
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
                    str => JsonConvert.DeserializeObject<Dictionary<AssistantAssignment.Data.Types.Assesments, double>>(str)
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
