using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Albar.AssistantAssignment.Abstractions;
using Albar.AssistantAssignment.Algorithm;
using Albar.AssistantAssignment.ThesisSpecificImplementation;
using Albar.AssistantAssignment.ThesisSpecificImplementation.Factories;
using Albar.AssistantAssignment.ThesisSpecificImplementation.ObjectiveEvaluators;
using Albar.AssistantAssignment.WebApp.Models;
using Bunnypro.GeneticAlgorithm.Abstractions;
using Bunnypro.GeneticAlgorithm.MultiObjective.NSGA2;
using Bunnypro.GeneticAlgorithm.Primitives;
using Microsoft.Extensions.Logging;

namespace Albar.AssistantAssignment.WebApp.Services.GeneticAlgorithm
{
    public class GeneticAlgorithmTask : IGeneticAlgorithmTask, IDisposable
    {
        private readonly GeneticAlgorithmTaskInfo _info;

        private IGeneticAlgorithm _geneticAlgorithm;
        private IPopulation _population;

        private CancellationTokenSource _tokenSource;

        public GeneticAlgorithmTask(
            Group @group,
            IReadOnlyDictionary<AssignmentObjective, double> coefficients,
            PopulationCapacity capacity)
        {
            _info = new GeneticAlgorithmTaskInfo(group, capacity, coefficients);
        }

        public IGeneticAlgorithmTaskInfo Info => _info;
        public IGeneticAlgorithmTaskListener Listener { get; set; }
        public Task<GeneticEvolutionStates> Task { get; private set; }

        public void Build(IDataRepository<AssignmentObjective> repository, Action<Exception> onError)
        {
            _info.State = GeneticAlgorithmTaskState.Building;
            try
            {
                var genotypePhenotypeMapper = new GenotypePhenotypeMapper(repository);

                var reproduction = new AssignmentReproduction<AssignmentObjective>(
                    genotypePhenotypeMapper,
                    new ReproductionSelection(repository)
                );
                var objectiveEvaluator = new AssignmentChromosomesEvaluator<AssignmentObjective>(_info.Coefficients)
                {
                    {AssignmentObjective.AssistantScheduleCollision, new AssistantScheduleCollisionEvaluator()},
                    {AssignmentObjective.AboveThresholdAssessment, new AboveThresholdAssessmentEvaluator()},
                    {AssignmentObjective.BelowThresholdAssessment, new BelowThresholdAssessmentEvaluator()},
                    {
                        AssignmentObjective.AverageOfNormalizedAssessment,
                        new AverageOfNormalizedAssessmentEvaluator(repository)
                    }
                };
                var nsga = new NSGA2<AssignmentObjective>(
                    reproduction,
                    objectiveEvaluator,
                    _info.Coefficients
                );
                _geneticAlgorithm = new Bunnypro.GeneticAlgorithm.Core.GeneticAlgorithm(nsga);
                var factory = new PopulationFactory<AssignmentObjective>(genotypePhenotypeMapper, objectiveEvaluator);
                _population = factory.Create(_info.Capacity);
                _info.State = GeneticAlgorithmTaskState.BuildCompleted;
            }
            catch(Exception e)
            {
                _info.State = GeneticAlgorithmTaskState.BuildFailed;
                onError.Invoke(e);
            }
        }

        public void Reset()
        {
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();
        }

        public void Start(TerminationKind kind, int value,
            Action<GeneticEvolutionStates, IPopulation> onFinished)
        {
            _info.State = GeneticAlgorithmTaskState.Starting;
            Reset();
            var terminationFunction = BuildTermination(kind, value);
            Task = _geneticAlgorithm.TryEvolveUntil(_population, terminationFunction, _tokenSource.Token)
                .ContinueWith(task =>
                {
                    if (task.Result.Item2)
                        _info.State = GeneticAlgorithmTaskState.Finished;
                    else
                        _info.State = GeneticAlgorithmTaskState.Stopped;

                    onFinished.Invoke(task.Result.Item1, _population);
                    return task.Result.Item1;
                });
            _info.State = GeneticAlgorithmTaskState.Running;
        }

        public void Stop()
        {
            _info.State = GeneticAlgorithmTaskState.Stopping;
            _tokenSource.Cancel();
        }

        public Func<GeneticEvolutionStates, bool> BuildTermination(TerminationKind kind, int value)
        {
            return states =>
            {
                Listener?.EvolvedOnce(Info.Id, states, _population.Chromosomes);
                if (kind == TerminationKind.EvolutionCount)
                    return states.EvolutionCount >= value;
                return states.EvolutionTime >= TimeSpan.FromSeconds(value);
            };
        }

        public void Dispose()
        {
            _tokenSource?.Dispose();
        }

        private class GeneticAlgorithmTaskInfo : IGeneticAlgorithmTaskInfo
        {
            public GeneticAlgorithmTaskInfo(
                Group @group,
                PopulationCapacity capacity,
                IReadOnlyDictionary<AssignmentObjective,double> coefficients)
            {
                Id = Guid.NewGuid().ToString();
                Group = @group;
                Capacity = capacity;
                Coefficients = coefficients;
                State = GeneticAlgorithmTaskState.Registered;
            }

            public GeneticAlgorithmTaskState State { get; set; }
            public string Id { get; }
            public Group Group { get; }
            public PopulationCapacity Capacity { get; }
            public IReadOnlyDictionary<AssignmentObjective, double> Coefficients { get; }
        }
    }
}