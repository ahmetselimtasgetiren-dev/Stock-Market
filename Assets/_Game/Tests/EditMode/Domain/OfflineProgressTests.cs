using System.Collections.Generic;
using NUnit.Framework;
using StockMarket.Domain.Persistence;

namespace StockMarket.Domain.Tests
{
    public sealed class OfflineProgressTests
    {
        [Test]
        public void Calculate_CapsElapsedTimeAndConvertsToWholeTicks()
        {
            OfflineProgressPlan plan = new OfflineProgressService().Calculate(100, 1000, 3, 300);

            Assert.That(plan.ElapsedSeconds, Is.EqualTo(900));
            Assert.That(plan.SimulatedSeconds, Is.EqualTo(300));
            Assert.That(plan.Ticks, Is.EqualTo(100));
            Assert.That(plan.WasCapped, Is.True);
        }

        [Test]
        public void Calculate_WhenClockMovesBackward_GrantsNoProgress()
        {
            OfflineProgressPlan plan = new OfflineProgressService().Calculate(1000, 900, 1, 3600);

            Assert.That(plan.Ticks, Is.Zero);
            Assert.That(plan.ElapsedSeconds, Is.Zero);
        }

        [Test]
        public void Apply_ProcessesEachPlannedTickSequentially()
        {
            var ticks = new List<long>();
            var service = new OfflineProgressService();
            OfflineProgressPlan plan = service.Calculate(0, 3, 1, 10);

            long finalTick = service.Apply(10, plan, ticks.Add);

            Assert.That(ticks, Is.EqualTo(new[] { 11L, 12L, 13L }));
            Assert.That(finalTick, Is.EqualTo(13));
        }
    }
}
