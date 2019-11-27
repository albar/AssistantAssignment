using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Thesis.DataType;

namespace Thesis.Algorithm.ObjectiveValueCalculators
{
    public class AboveThresholdCalculator : ObjectiveValueCalculatorBase
    {
        private readonly ImmutableDictionary<int, ImmutableDictionary<Assesments, double>> _thresholds;
        private readonly ImmutableDictionary<int, int> _schedulesCourse;

        public AboveThresholdCalculator(IDataRepository repository)
        {
            _thresholds = repository.Courses.ToImmutableDictionary(
                course => course.Id,
                course => course.Threshold);

            _schedulesCourse = repository.Schedules.Select((schedule, id) =>
                    new KeyValuePair<int, int>(id, schedule.CourseId))
                .ToImmutableDictionary(kv => kv.Key, kv => kv.Value);
        }

        public override Objectives Objective { get; } = Objectives.AboveThreshold;

        public override bool NeedToBeNormalized { get; } = true;

        public override Optimum Optimum { get; } = Optimum.Maximum;

        public override async Task<double> CalculateAsync(Chromosome chromosome, CancellationToken token)
        {
            var tasks = chromosome.Phenotype.Select((phenotype, scheduleId) => Task.Run(() =>
                    _thresholds[_schedulesCourse[scheduleId]].All(assesment =>
                        phenotype.AssesmentsValues[assesment.Key] >= assesment.Value),
                token));

            var result = await Task.WhenAll(tasks);
            return result.Count(isAbove => isAbove);
        }
    }
}