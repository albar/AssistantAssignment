using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AssistantAssignment.Data.Abstractions;
using AssistantAssignment.Data.Types;

namespace AssistantAssignment.Algorithm
{
    public class PhenotypeResolver
    {
        private readonly ImmutableArray<Schedule> _schedules;
        private readonly ImmutableDictionary<int, ImmutableDictionary<int, ImmutableDictionary<Assesments, double>>> _assistantsAssesmentsValues;

        public PhenotypeResolver(IDataRepository repository)
        {
            _schedules = repository.Schedules;
            _assistantsAssesmentsValues = repository.Assistants.ToImmutableDictionary(
                assistant => assistant.Id,
                assistant => assistant.CoursesAssesmentsValues);
        }

        public async Task<ImmutableArray<PhenotypeRepresentation>> ResolveAsync(
            ImmutableArray<Gene> genotype,
            System.Threading.CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var tasks = genotype.Select((gene, scheduleId) =>
                Task.Run(() =>
                {
                    var assesmentsValues = gene.AssistantsIds.Select(assistantId =>
                        _assistantsAssesmentsValues[assistantId][_schedules[scheduleId].CourseId])
                    .ToArray();

                    var maxAssesmentsValues = AssesmentsExtensions.AllAssessments
                        .ToImmutableDictionary(
                            assesment => assesment,
                            assesment => assesmentsValues.Max(values => values[assesment]));

                    return new PhenotypeRepresentation
                    {
                        AssesmentsValues = maxAssesmentsValues,
                        IsCollided = false,
                    };
                }, token));

            var phenotype = await Task.WhenAll(tasks);
            return phenotype.ToImmutableArray();
        }
    }
}
