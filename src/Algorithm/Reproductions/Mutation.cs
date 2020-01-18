using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssistantAssignment.Data.Abstractions;
using AssistantAssignment.Data.Types;
using EvolutionaryAlgorithm.Abstraction;

namespace AssistantAssignment.Algorithm.Reproductions
{
    public class Mutation : IReproduction<Chromosome>
    {
        private readonly ImmutableArray<Schedule> _schedules;
        private readonly ImmutableDictionary<int, ImmutableHashSet<int>> _coursesAssistants;
        private readonly Random _random = new Random();

        public Mutation(IDataRepository repository)
        {
            _schedules = repository.Schedules;
            _coursesAssistants = repository.Courses
                .ToImmutableDictionary(course => course.Id, course =>
                    course.AssistantsIds);
        }

        public async Task<IEnumerable<Chromosome>> ReproduceAsync(
            ImmutableHashSet<Chromosome> parents, CancellationToken token)
        {
            var tasks = parents.Where(NeedToBeMutated)
                .Select(parent => MutateAsync(parent, token));

            return await Task.WhenAll(tasks);
        }

        private async Task<Chromosome> MutateAsync(
            Chromosome parent,
            CancellationToken token)
        {
            var tasks = parent.Phenotype.Select((phenotype, scheduleId) =>
                Task.Run(() =>
                {
                    if (!phenotype.IsCollided)
                        return parent.Genotype[scheduleId];

                    var ids = _coursesAssistants[_schedules[scheduleId].CourseId]
                        .OrderBy(_ => _random.Next())
                        .Take(_schedules[scheduleId].RequiredAssistantsCount)
                        .ToImmutableHashSet();

                    return new Gene(ids);
                }, token));

            var result = await Task.WhenAll(tasks);
            var genotype = result.ToImmutableArray();

            return new Chromosome(genotype);
        }

        private bool NeedToBeMutated(Chromosome parent)
        {
            return parent.OriginalObjectivesValue[Objectives.SchedulesCollision] > 0;
        }
    }
}
