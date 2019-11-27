using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Thesis.DataType;

namespace Thesis.Algorithm.ObjectiveValueCalculators
{
    public class NormalizedAssessmentsValuesCalculator : ObjectiveValueCalculatorBase
    {
        public override Objectives Objective { get; } = Objectives.NormalizedAssessmentsValues;
        public override bool NeedToBeNormalized { get; } = false;
        public override Optimum Optimum { get; } = Optimum.Maximum;

        public override async Task<double> CalculateAsync(Chromosome chromosome, CancellationToken token)
        {
            var normalizerTasks = AssesmentsExtensions.AllAssessments
                .Select<Assesments, Task<KeyValuePair<Assesments, Func<double, double>>>>(assesment =>
                    Task.Run<KeyValuePair<Assesments, Func<double, double>>>(() =>
                    {
                        var ordered = chromosome.Phenotype.Select(phenotype =>
                            phenotype.AssesmentsValues[assesment])
                                .OrderBy(assesmentValue => assesmentValue).ToArray();

                        var highest = ordered.Last();
                        var lowest = ordered.First();
                        var range = Math.Abs(highest - lowest);

                        return new KeyValuePair<Assesments, Func<double, double>>(
                            assesment, value => Math.Abs(value - lowest) / range);
                    }, token));

            var normalizers = await Task.WhenAll(normalizerTasks);

            var tasks = chromosome.Phenotype.Select(phenotype => Task.Run(() =>
                normalizers.Select(normalizer =>
                    normalizer.Value.Invoke(phenotype.AssesmentsValues[normalizer.Key])),
                token));

            var results = await Task.WhenAll(tasks);
            return results.SelectMany(values => values).Average();
        }
    }
}
