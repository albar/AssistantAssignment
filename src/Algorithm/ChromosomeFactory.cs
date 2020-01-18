using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssistantAssignment.Data.Types;

namespace AssistantAssignment.Algorithm
{
    public class ChromosomeFactory
    {
        private readonly ImmutableArray<Schedule> _schedules;
        private readonly ImmutableDictionary<int, ImmutableHashSet<int>> _coursesAssistants;

        public ChromosomeFactory(IDataRepository repository)
        {
            _schedules = repository.Schedules;
            _coursesAssistants = repository.Courses.ToImmutableDictionary(
                course => course.Id,
                course => course.AssistantsIds);
        }

        public async Task<ImmutableHashSet<Chromosome>> CreateAsync(int count, CancellationToken token)
        {
            var builder = ImmutableHashSet.CreateBuilder<Chromosome>();
            while (builder.Count < count)
            {
                var tasks = Enumerable.Range(0, count - builder.Count).Select(_ => CreateAsync(token));
                var result = await Task.WhenAll(tasks);
                builder.UnionWith(result);
            }
            return builder.ToImmutable();
        }

        public async Task<Chromosome> CreateAsync(CancellationToken token)
        {
            var random = new Random();
            var tasks = _schedules.Select((schedule, index) =>
                Task.Run(() =>
                {
                    var ids = _coursesAssistants[schedule.CourseId]
                        .OrderBy(_ => random.Next())
                        .Take(schedule.RequiredAssistantsCount)
                        .ToImmutableHashSet();
                    return new Gene(ids);
                }, token));

            var result = await Task.WhenAll(tasks);
            var genotype = result.ToImmutableArray();
            return new Chromosome(genotype);
        }
    }
}
