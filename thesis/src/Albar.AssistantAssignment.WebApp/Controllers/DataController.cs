using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Albar.AssistantAssignment.DataAbstractions;
using Albar.AssistantAssignment.ThesisSpecificImplementation;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Data;
using Albar.AssistantAssignment.WebApp.Models;
using Albar.AssistantAssignment.WebApp.Services.DatabaseTask;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Albar.AssistantAssignment.WebApp.Controllers
{
    [Route("api/data")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly AssignmentDatabase _database;
        private readonly IDatabaseBackgroundTaskQueue _taskQueue;
        private readonly ILogger<DataController> _logger;

        public DataController(
            AssignmentDatabase database,
            IDatabaseBackgroundTaskQueue taskQueue,
            ILogger<DataController> logger)
        {
            _database = database;
            _taskQueue = taskQueue;
            _logger = logger;
        }

        [HttpGet("coefficient")]
        public JsonResult Coefficient()
        {
            return new JsonResult(
                Enum.GetValues(typeof(AssignmentObjective))
                    .Cast<AssignmentObjective>()
                    .Select(objective => objective.ToString())
            );
        }


        [HttpGet("group")]
        public JsonResult Group()
        {
            var groups = _database.Groups.Select(group => new
            {
                group.Id,
                group.Name
            }).ToArray();

            return new JsonResult(groups);
        }

        [HttpGet("group/{id}/subject")]
        public JsonResult Subject(int id, bool schedules = false, bool assistants = false)
        {
            var group = _database.Groups.FirstOrDefault(g => g.Id == id);
            if (group == null) return new JsonResult(NotFound());

            var subjects = _database.Subjects.Where(s => s.Group.Id == id)
                .When(schedules, query => query.Include(subject => subject.Schedules))
                .When(assistants, query =>
                    query.Include(subject => subject.AssistantSubjects)
                        .ThenInclude(ass => ass.Assistant)
                )
                .ToList()
                .Select(subject => new
                {
                    subject.Id,
                    GroupId = subject.Group.Id,
                    subject.Code,
                    Schedules = subject.Schedules?.Select(schedule => new
                    {
                        schedule.Id,
                        Day = schedule.Day.ToString(),
                        Session = schedule.Session.ToString(),
                        schedule.Lab
                    }),
                    subject.AssistantPerScheduleCount,
                    subject.AssessmentsThreshold,
                    Assistants = subject.Assistants?.Select(assistant => new
                    {
                        assistant.Id,
                        Npm = assistant.Npm.ToString(),
                        Assessment = assistant.AssistantSubjects.First(ass => ass.SubjectId == subject.Id)?.Assessments
                    })
                }).ToArray();

            var result = new
            {
                Group = new
                {
                    group.Id,
                    group.Name
                },
                Subjects = subjects
            };

            return new JsonResult(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateGroupFromFiles(
            IFormFile subject,
            IFormFile assistant,
            IFormFile schedule,
            IFormFile assistant_subject)
        {
            var name = Request.Form["name"];
            var files = new Dictionary<UploadFile, IFormFile>
            {
                {UploadFile.Subject, subject},
                {UploadFile.Assistant, assistant},
                {UploadFile.Schedule, schedule},
                {UploadFile.AssistantSubject, assistant_subject}
            };

            var complete = files.All(file => file.Value != null);
            if (string.IsNullOrEmpty(name) || !complete)
                return BadRequest();

            var tmpPath = Path.GetTempPath();

            _logger.LogInformation("Creating Task");
            var tasks = files.Select(async file =>
            {
                var filePath = $"{tmpPath}{file.Key.ToString()}.csv";

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.Value.CopyToAsync(stream);
                }

                return new KeyValuePair<UploadFile, string>(file.Key, filePath);
            }).ToList();

            var filePaths = (await Task.WhenAll(tasks))
                .ToDictionary(file => file.Key, file => file.Value);

            var taskId = _taskQueue.Enqueue(async (database, token) =>
            {
                var group = new Group
                {
                    Name = name
                };

                var createSubjectTask = Task.Run(() =>
                {
                    var lines = System.IO.File.ReadLines(filePaths[UploadFile.Subject]);
                    return lines.Select(line =>
                    {
                        var split = line.Split(',');
                        var code = split[0];
                        var requirement = int.Parse(split[1]);
                        var threshold = split[2].Split(';')
                            .Select((value, i) => new KeyValuePair<AssistantAssessment, double>(
                                (AssistantAssessment) i, double.Parse(value)
                            )).ToDictionary(a => a.Key, a => a.Value);

                        return new Models.Subject
                        {
                            Code = code,
                            Group = group,
                            AssistantPerScheduleCount = requirement,
                            AssessmentsThreshold = threshold,
                            AssistantSubjects = new List<AssistantSubject>()
                        };
                    }).ToList();
                }, token);

                var createAssistantTask = Task.Run(() =>
                {
                    var lines = System.IO.File.ReadLines(filePaths[UploadFile.Assistant]);
                    return lines.Select(line => new Models.Assistant
                    {
                        Group = group,
                        Npm = new Npm(line),
                        AssistantSubjects = new List<AssistantSubject>()
                    }).ToList();
                }, token);

                var subjects = await createSubjectTask;
                var createScheduleTask = Task.Run(() =>
                {
                    var lines = System.IO.File.ReadLines(filePaths[UploadFile.Schedule]);
                    return lines.Select(line =>
                    {
                        var split = line.Split(',');
                        var subjectCode = split[0];
                        var day = (DayOfWeek) int.Parse(split[1]);
                        var session = (SessionOfDay) int.Parse(split[2]);
                        var lab = int.Parse(split[3]);

                        return new Models.Schedule
                        {
                            Group = group,
                            Subject = subjects.First(s => s.Code == subjectCode),
                            Day = day,
                            Session = session,
                            Lab = lab
                        };
                    });
                }, token);

                var assistants = await createAssistantTask;
                var createAssistantSubjectTask = Task.Run(() =>
                {
                    var lines = System.IO.File.ReadLines(filePaths[UploadFile.AssistantSubject]);
                    foreach (var line in lines)
                    {
                        var split = line.Split(',');
                        var npm = new Npm(split[0]);
                        var subjectCode = split[1];
                        var assessments = split[2].Split(';').Select(
                            (value, i) => new KeyValuePair<AssistantAssessment, double>(
                                (AssistantAssessment) i,
                                double.Parse(value)
                            )
                        ).ToDictionary(a => a.Key, a => a.Value);

                        var currentSubject = subjects.First(s => s.Code == subjectCode);
                        var currentAssistant = assistants.First(a => a.Npm.Equals(npm));

                        var ass = new AssistantSubject
                        {
                            Subject = currentSubject,
                            Assistant = currentAssistant,
                            Assessments = assessments
                        };

                        currentSubject.AssistantSubjects.Add(ass);
                        currentAssistant.AssistantSubjects.Add(ass);
                    }
                }, token);

                var schedules = await createScheduleTask;
                await createAssistantSubjectTask;

                var lastTasks = new[]
                {
                    database.Groups.AddAsync(group, token),
                    database.Subjects.AddRangeAsync(subjects, token),
                    database.Assistants.AddRangeAsync(assistants, token),
                    database.Schedules.AddRangeAsync(schedules, token)
                };

                await Task.WhenAll(lastTasks);

                await database.SaveChangesAsync(token);
            });
            _logger.LogInformation($"Task Created: {taskId}");

            var result = new Dictionary<string, string>
            {
                {"task_id", taskId}
            };

            return new JsonResult(result);
        }

        [HttpPost("generate")]
        public JsonResult CreateDataRandomly()
        {
            var taskId = _taskQueue.Enqueue(async (database, token) =>
            {
                var group = new Group();

                var randomize = new Random();
                var subjects = Enumerable.Range(0, 5).Select(_ =>
                {
                    var subject = database.Subjects.Add(new Models.Subject
                    {
                        Group = group,
                        AssistantPerScheduleCount = 3,
                        AssessmentsThreshold = Enum.GetValues(typeof(AssistantAssessment))
                            .Cast<AssistantAssessment>()
                            .ToDictionary(
                                assessment => assessment,
                                assessment => 8d
                            )
                    }).Entity;

                    var schedules = Enumerable.Range(0, randomize.Next(8, 11)).Select(__ =>
                    {
                        var schedule = new Models.Schedule
                        {
                            Group = group,
                            Subject = subject,
                            Day = (DayOfWeek) randomize.Next(0, 6),
                            Session = (SessionOfDay) randomize.Next(0, 4),
                            Lab = randomize.Next(1, 20)
                        };

                        return database.Schedules.Add(schedule).Entity;
                    });

                    subject.Schedules = schedules.ToList();
                    subject.AssistantSubjects = new List<AssistantSubject>();

                    return subject;
                }).ToList();

                while (subjects.Any(subject =>
                    subject.AssistantSubjects.Count < subject.Schedules.Count + randomize.Next(3, 5)))
                {
                    var assistantSubjects = subjects
                        .OrderBy(subject => subject.AssistantSubjects.Count)
                        .Take(2).ToList();

                    var year = randomize.Next(10, 19);
                    var major = randomize.Next(11, 12);
                    var id = randomize.Next(6000, 9000);

                    var assistant = new Models.Assistant
                    {
                        Group = group,
                        Npm = new Npm($"{year}.{major}.{id}"),
                        AssistantSubjects = new List<AssistantSubject>()
                    };

                    foreach (var subject in assistantSubjects)
                    {
                        var assistantSubject = new AssistantSubject
                        {
                            Subject = subject,
                            Assistant = assistant,
                            Assessments = Enum.GetValues(typeof(AssistantAssessment))
                                .Cast<AssistantAssessment>()
                                .ToDictionary(
                                    assessment => assessment,
                                    _ => 7 + randomize.NextDouble() * 9 % 2
                                )
                        };
                        subject.AssistantSubjects.Add(assistantSubject);
                        assistant.AssistantSubjects.Add(assistantSubject);
                    }

                    database.Assistants.Add(assistant);
                }

                database.Groups.Add(group);

                await database.SaveChangesAsync(token);
            });

            var result = new Dictionary<string, string>
            {
                {"task_id", taskId}
            };

            return new JsonResult(Ok(result));
        }

        private enum UploadFile
        {
            Subject,
            Assistant,
            Schedule,
            AssistantSubject
        }
    }

    public static class ConditionalQueryExtension
    {
        public static IQueryable<TEntity> When<TEntity>(
            this IQueryable<TEntity> source,
            bool condition,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> nested)
        {
            return condition ? nested.Invoke(source) : source;
        }
    }
}