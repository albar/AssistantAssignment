using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Thesis.DataType;

namespace Thesis.Algorithm
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
                var worker = Enumerable.Range(0, count).Select(_ => CreateAsync(token));
                var result = await Task.WhenAll(worker);
                builder.UnionWith(result);
            }
            return builder.ToImmutable();
        }

        public async Task<Chromosome> CreateAsync(CancellationToken token)
        {
            var random = new Random();
            var worker = _schedules.Select((schedule, index) =>
                Task.Run(() =>
                {
                    var combination = _coursesAssistants[schedule.CourseId]
                        .OrderBy(_ => random.Next())
                        .Take(schedule.RequiredAssistantsCount)
                        .ToImmutableHashSet();
                    return new KeyValuePair<int, ImmutableHashSet<int>>(index, combination);
                }, token));
            var result = await Task.WhenAll(worker);
            var genotype = result.OrderBy(kv => kv.Key)
                .Select(kv => new Gene(kv.Value))
                .ToImmutableArray();
            return new Chromosome(genotype);
        }
    }
}
