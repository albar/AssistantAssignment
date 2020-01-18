using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssistantAssignment.Data.Abstractions;
using AssistantAssignment.Data.Types;

namespace AssistantAssignment.Algorithm.ObjectiveValueCalculators
{
    public class SchedulesCollisionCalculator : ObjectiveValueCalculatorBase
    {
        private readonly ImmutableArray<Schedule> _schedules;

        public SchedulesCollisionCalculator(IDataRepository repository)
        {
            _schedules = repository.Schedules;
        }

        public override Objectives Objective { get; } =
            Objectives.SchedulesCollision;
        public override bool NeedToBeNormalized { get; } = true;
        public override Optimum Optimum { get; } = Optimum.Minimum;

        public override Task<double> CalculateAsync(
            Chromosome chromosome,
            CancellationToken token)
        {
            var count = 0;
            var schedulesIds = Enumerable.Range(0, _schedules.Length).ToArray();
            while (schedulesIds.Length > 0)
            {
                var currentScheduleId = schedulesIds.First();
                var similarTimeSchedulesIds = schedulesIds
                    .Where(id => _schedules[currentScheduleId]
                        .TimeEquals(_schedules[id]))
                    .ToArray();

                if (similarTimeSchedulesIds.Length > 1)
                    count += CountCollisions(similarTimeSchedulesIds, chromosome);

                schedulesIds = schedulesIds
                    .Except(similarTimeSchedulesIds)
                    .ToArray();
            }

            return Task.FromResult((double)count);
        }

        private int CountCollisions(
            int[] similarTimeSchedulesIds,
            Chromosome chromosome)
        {
            var count = 0;
            var currentScheduleId = similarTimeSchedulesIds[0];
            similarTimeSchedulesIds = similarTimeSchedulesIds.Skip(1).ToArray();

            if (ScheduleCollided(
                    currentScheduleId,
                    similarTimeSchedulesIds,
                    chromosome.Genotype,
                    chromosome.Phenotype))
            {
                count++;
            }

            if (similarTimeSchedulesIds.Length > 1)
                count += CountCollisions(similarTimeSchedulesIds, chromosome);

            return count;
        }

        private bool ScheduleCollided(
            int currentScheduleId,
            int[] similarTimeSchedulesIds,
            ImmutableArray<Gene> genotype,
            ImmutableArray<PhenotypeRepresentation> phenotype)
        {
            foreach (var scheduleid in similarTimeSchedulesIds)
            {
                foreach (var currentAssistantId in genotype[currentScheduleId].AssistantsIds)
                {
                    foreach (var assistantId in genotype[scheduleid].AssistantsIds)
                    {
                        if (currentAssistantId.Equals(assistantId))
                        {
                            phenotype[currentScheduleId].IsCollided = true;
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
