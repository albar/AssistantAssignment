using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssistantAssignment.Data.Types;

namespace AssistantAssignment.Algorithm.ObjectiveValueCalculators
{
    public class AboveAverageCalculator : ObjectiveValueCalculatorBase
    {
        public override Objectives Objective { get; } = Objectives.AboveAverage;
        public override bool NeedToBeNormalized { get; } = true;
        public override Optimum Optimum { get; } = Optimum.Maximum;

        public override async Task<double> CalculateAsync(
            Chromosome chromosome,
            CancellationToken token)
        {
            var averages = AssesmentsExtensions.AllAssessments.ToDictionary(
                assesment => assesment,
                assesment => chromosome.Phenotype.Average(phenotype =>
                    phenotype.AssesmentsValues[assesment])
            );

            var tasks = chromosome.Phenotype.Select(phenotype =>
                Task.Run(() =>
                {
                    var isAbove = AboveAverage(
                        averages, phenotype.AssesmentsValues);
                    phenotype.IsAboveThreshold = isAbove;
                    return isAbove;
                }, token));

            var result = await Task.WhenAll(tasks);
            return result.Count(isAbove => isAbove);
        }

        private bool AboveAverage(
            IReadOnlyDictionary<Assesments, double> averages,
            ImmutableDictionary<Assesments, double> assesmentsValues)
        {
            return averages.All(kv => assesmentsValues[kv.Key] >= kv.Value);
        }
    }
}
