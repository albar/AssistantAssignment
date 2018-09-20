using System;
using System.Threading.Tasks;
using Bunnypro.GeneticAlgorithm.Core.Termination;
using Bunnypro.GeneticAlgorithm.Examples.Simple;
using Bunnypro.GeneticAlgorithm.Standard;
using Xunit;

namespace Bunnypro.GeneticAlgorithm.Core.Test
{
    public class GeneticAlgorithmTest
    {
        private static IGeneticAlgorithm CreateGeneticAlgorithm()
        {
            return new GeneticAlgorithm(new SimplePopulation(), new SimpleStrategy());
        }
        
        [Fact]
        public async Task Should_evolve_until_stopped()
        {
            var ga = CreateGeneticAlgorithm();

            var evolving = ga.Evolve();
            await Task.Delay(2000);
            await ga.Stop();
            await evolving;
            
            Assert.True(ga.GenerationNumber > 0);
        }
        
        [Fact]
        public async Task Should_continue_evolve_after_stopped()
        {
            var ga = CreateGeneticAlgorithm();

            var evolving = ga.Evolve();
            await Task.Delay(1000);
            await ga.Stop();
            await evolving;
            
            Assert.True(ga.GenerationNumber > 0);
            var gn = ga.GenerationNumber;
            
            var continued = ga.Evolve();
            await Task.Delay(1000);
            await ga.Stop();
            await continued;
            
            Assert.True(ga.GenerationNumber > gn);
        }

        [Fact]
        public async Task Should_evolve_until_condition_fulfilled()
        {
            const int maxGenerationNumber = 20;
            var ga = CreateGeneticAlgorithm();
            await ga.EvolveUntil(() => ga.GenerationNumber >= maxGenerationNumber);
            
            Assert.Equal(maxGenerationNumber, ga.GenerationNumber);

            const int nextMaxGenerationNumber = maxGenerationNumber + 10;
            await ga.EvolveUntil(() => ga.GenerationNumber >= nextMaxGenerationNumber);
            
            Assert.Equal(nextMaxGenerationNumber, ga.GenerationNumber);
        }

        [Fact]
        public async Task Should_continue_evolve_until_condition_fulfilled_after_stopped()
        {
            const int maxGenerationNumber = 10;
            var ga = CreateGeneticAlgorithm();
            var evolution = ga.EvolveUntil(() =>
            {
                Task.Delay(500).Wait();
                return ga.GenerationNumber >= maxGenerationNumber;
            });

            await Task.Delay(1000);
            await ga.Stop();
            await evolution;
            
            Assert.True(maxGenerationNumber > ga.GenerationNumber);
            await ga.Evolve();
            Assert.Equal(maxGenerationNumber, ga.GenerationNumber);
        }
        
        [Fact]
        public async Task Should_not_evolve_if_condition_fulfilled()
        {
            const int maxGenerationNumber = 10;
            var ga = CreateGeneticAlgorithm();
            await ga.EvolveUntil(() => ga.GenerationNumber >= maxGenerationNumber);
            
            Assert.Equal(maxGenerationNumber, ga.GenerationNumber);
            await ga.Evolve();
            Assert.Equal(maxGenerationNumber, ga.GenerationNumber);
        }

        [Fact]
        public async Task Should_not_evolve_if_termination_fulfilled()
        {
            const int maxGenerationNumber = 10;
            var ga = CreateGeneticAlgorithm();
            await ga.EvolveUntil(new FunctionTermination(() => ga.GenerationNumber >= maxGenerationNumber));
            
            Assert.Equal(maxGenerationNumber, ga.GenerationNumber);
            await ga.Evolve();
            Assert.Equal(maxGenerationNumber, ga.GenerationNumber);
        }

        [Fact]
        public async Task Should_not_evolve_after_time_limit_expected()
        {
            const double timeLimit = 1000;
            var ga = CreateGeneticAlgorithm();
            await ga.EvolveUntil(new TimeLimitTermination(timeLimit));
            Assert.True(ga.GenerationNumber > 0);
            var gn = ga.GenerationNumber;
            await ga.Evolve();
            Assert.Equal(ga.GenerationNumber, gn);
        }

        [Fact]
        public async Task Should_continue_evolve_after_stopped_when_time_limit_not_exceed()
        {
            const double timeLimit = 1000;
            var ga = CreateGeneticAlgorithm();
            var evolving = ga.EvolveUntil(new TimeLimitTermination(timeLimit));
            await Task.Delay(200);
            await ga.Stop();
            await evolving;
            Assert.True(ga.GenerationNumber > 0);
            var gn = ga.GenerationNumber;
            await ga.Evolve();
            Assert.True(ga.GenerationNumber > gn);
        }

        [Fact]
        public async Task Should_not_evolve_after_time_span_limit_expected()
        {
            var ga = CreateGeneticAlgorithm();
            await ga.EvolveUntil(new TimeLimitTermination(new TimeSpan(0, 0, 1)));
            Assert.True(ga.GenerationNumber > 0);
            var gn = ga.GenerationNumber;
            await ga.Evolve();
            Assert.Equal(ga.GenerationNumber, gn);
        }

        [Fact]
        public async Task Should_continue_evolve_after_stopped_when_time_span_limit_not_exceed()
        {
            var ga = CreateGeneticAlgorithm();
            var evolving = ga.EvolveUntil(new TimeLimitTermination(new TimeSpan(0, 0, 1)));
            await Task.Delay(100);
            await ga.Stop();
            await evolving;
            Assert.True(ga.GenerationNumber > 0);
            var gn = ga.GenerationNumber;
            await ga.Evolve();
            Assert.True(ga.GenerationNumber > gn);
        }

        [Fact]
        public async Task Should_have_correct_evolving_state()
        {
            var ga = CreateGeneticAlgorithm();
            var evolving = ga.Evolve();
            await Task.Delay(100);
            Assert.True(ga.Evolving);
            await ga.Stop();
            await evolving;
            Assert.False(ga.Evolving);
        }
    }
}
