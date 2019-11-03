using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Albar.AssistantAssignment.DataAbstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;
using Albar.AssistantAssignment.WebApp.Models;
using Assistant = Albar.AssistantAssignment.WebApp.Models.Assistant;
using Schedule = Albar.AssistantAssignment.WebApp.Models.Schedule;
using Subject = Albar.AssistantAssignment.WebApp.Models.Subject;

namespace Albar.AssistantAssignment.Parsing
{
    class Program
    {
        private static Random _random = new Random();

        static async Task Main(string[] args)
        {
            var basePath = "/home/albar/Codes/GA/Framework/thesis/src/Albar.AssistantAssignment.Parsing";
            var file = $"{basePath}/data.csv";
            var lines = File.ReadLines(file);
            var subjects = new List<Subject>();
            var schedules = new List<Schedule>();
            var assistants = new List<Assistant>();
            var assistantSubjects = new List<AssistantSubject>();

            foreach (var line in lines)
            {
                var columns = line.Split(',');
                var subjectCode = columns[0];
                var day = columns[1];
                var session = columns[2];
                var lab = columns[3];
                var assistantArray = columns.Skip(4).Where(a => !string.IsNullOrEmpty(a)).ToArray();

                var subject = FindOrCreateSubject(subjects, subjectCode);
                AddSchedule(schedules, subject, int.Parse(day), int.Parse(session), int.Parse(lab));
                foreach (var assistantNpm in assistantArray)
                {
                    var assistant = FindOrCreateAssistant(assistants, assistantNpm);
                    AddAssistantSubject(assistantSubjects, subject, assistant);
                }
            }

//            foreach (var subject in subjects)
//            {
//                Console.WriteLine($"Subject: {subject.Code}, Threshold: {string.Join(", ", subject.AssessmentsThreshold.Select(t => $"{t.Key} ({t.Value})"))}");
//                Console.WriteLine("Schedules:");
//                foreach (var schedule in schedules.Where(schedule => schedule.Subject.Code == subject.Code))
//                {
//                    Console.WriteLine($"\tDay: {schedule.Day}, Session: {schedule.Session}, Lab: {schedule.Lab}");
//                }
//                Console.WriteLine("Assistants:");
//                foreach (var assistantSubject in assistantSubjects.Where(ass => ass.Subject.Code == subject.Code))
//                {
//                    Console.WriteLine($"\tAssistant: {assistantSubject.Assistant.Npm} Assessment: {string.Join(", ", assistantSubject.Assessments.Select(ass => $"{ass.Key} ({ass.Value})"))}");
//                }
//            }

            var writeSubjectTask = File.WriteAllLinesAsync($"{basePath}/subject.csv", subjects.Select(subject =>
                $"{subject.Code},{subject.AssistantPerScheduleCount},{string.Join(";", subject.AssessmentsThreshold.Values)}"
            ));
            var writeScheduleTask = File.WriteAllLinesAsync($"{basePath}/schedule.csv", schedules.Select(schedule =>
                $"{schedule.Subject.Code},{(int) schedule.Day},{(int) schedule.Session},{schedule.Lab}"
            ));
            var writeAssistantTask = File.WriteAllLinesAsync($"{basePath}/assistant.csv", assistants.Select(assistant =>
                $"{assistant.Npm}"
            ));
            var writeAssistantSubjectTask = File.WriteAllLinesAsync($"{basePath}/assistant_subject.csv",
                assistantSubjects.Select(ass => 
                    $"{ass.Assistant.Npm},{ass.Subject.Code},{string.Join(";", ass.Assessments.Values)}"
                ));

            var tasks = new[] {writeSubjectTask, writeScheduleTask, writeAssistantTask, writeAssistantSubjectTask};
            await Task.WhenAll(tasks);
        }

        private static void AddAssistantSubject(
            List<AssistantSubject> assistantSubjects,
            Subject subject,
            Assistant assistant)
        {
            if (assistantSubjects.Any(ass =>
                ass.Subject.Code == subject.Code &&
                ass.Assistant.Npm.Equals(assistant.Npm)
            )) return;

            assistantSubjects.Add(new AssistantSubject
            {
                Subject = subject,
                Assistant = assistant,
                Assessments = Enum.GetValues(typeof(AssistantAssessment))
                    .Cast<AssistantAssessment>()
                    .ToDictionary(assessment => assessment,
                        _ => Math.Round(7 + Math.Abs(_random.NextDouble() * 9) % 2, 2))
            });
        }

        private static Assistant FindOrCreateAssistant(
            List<Assistant> assistants,
            string assistantNpm)
        {
            var assistant = assistants.FirstOrDefault(a => a.Npm.ToString().Equals(assistantNpm));
            if (assistant != null) return assistant;

            assistant = new Assistant
            {
                Npm = new Npm(assistantNpm)
            };
            assistants.Add(assistant);
            return assistant;
        }

        private static void AddSchedule(
            List<Schedule> schedules,
            Subject subject,
            int day,
            int session,
            int lab)
        {
            if (schedules.Any(schedule =>
                schedule.Subject.Code == subject.Code &&
                (int) schedule.Day == day &&
                (int) schedule.Session == session &&
                schedule.Lab == lab
            )) return;

            schedules.Add(new Schedule
            {
                Subject = subject,
                Day = (DayOfWeek) day,
                Session = (SessionOfDay) (session - 1),
                Lab = lab
            });
        }

        private static Subject FindOrCreateSubject(List<Subject> subjects, string subjectCode)
        {
            try
            {
                var subject = subjects.FirstOrDefault(s => s.Code.Equals(subjectCode));
                if (subject != null) return subject;
                subject = new Subject
                {
                    Code = subjectCode,
                    AssistantPerScheduleCount = 3,
                    AssessmentsThreshold = Enum.GetValues(typeof(AssistantAssessment))
                        .Cast<AssistantAssessment>()
                        .ToDictionary(assessment => assessment, _ => Math.Round(8d, 2))
                };
                subjects.Add(subject);
                return subject;
            }
            catch
            {
                Console.WriteLine(subjectCode);
                throw;
            }
        }
    }
}