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
    public class Crossover : IReproduction<Chromosome>
    {
        private readonly ImmutableArray<Schedule> _schedules;
        private readonly Random _random = new Random();

        public Crossover(IDataRepository repository)
        {
            _schedules = repository.Schedules;
        }

        public async Task<IEnumerable<Chromosome>> ReproduceAsync(
            ImmutableHashSet<Chromosome> parents, CancellationToken token)
        {
            var reproducibleParents = SelectParents(parents);
            var tasks = reproducibleParents.Select(parent =>
                CrossoverAsync(parent.Item1, parent.Item2, token));

            return await Task.WhenAll(tasks);
        }

        private IEnumerable<(Chromosome, Chromosome)> SelectParents(
            ImmutableHashSet<Chromosome> parents)
        {
            var count = parents.Count / 2;

            return Enumerable.Range(0, count)
                .Select(_ =>
                {
                    var selected = parents.OrderBy(__ =>
                        _random.Next()).Take(2).ToArray();
                    parents = parents.Except(selected);

                    return (selected[0], selected[1]);
                });
        }

        private async Task<Chromosome> CrossoverAsync(
            Chromosome parent1, Chromosome parent2, CancellationToken token)
        {
            var tasks = parent1.Genotype.Select((parent1Gene, locus) =>
                Task.Run(() =>
                {
                    var parent2Gene = parent2.Genotype[locus];
                    if (parent1Gene.Equals(parent2Gene))
                    {
                        return parent1Gene;
                    }

                    var assistantsIds = new HashSet<int>();
                    assistantsIds.UnionWith(parent1Gene.AssistantsIds);
                    assistantsIds.UnionWith(parent2Gene.AssistantsIds);

                    var ids = assistantsIds
                        .OrderBy(_ => _random.Next())
                        .Take(_schedules[locus].RequiredAssistantsCount)
                        .ToImmutableHashSet();

                    return new Gene(ids);
                }, token));

            var genotype = await Task.WhenAll(tasks);

            return new Chromosome(genotype.ToImmutableArray());
        }
    }
}
