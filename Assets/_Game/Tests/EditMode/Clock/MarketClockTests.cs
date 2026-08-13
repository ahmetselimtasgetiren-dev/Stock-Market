using System;
using System.Collections.Generic;
using NUnit.Framework;
using StockMarket.Core.Clock;

namespace StockMarket.Core.Tests.Clock
{
    public sealed class MarketClockTests
    {
        [Test]
        public void Advance_WhenDurationIsIncomplete_DoesNotEmitTick()
        {
            var clock = new MarketClock(1d);

            long emittedTicks = clock.Advance(0.75d);

            Assert.That(emittedTicks, Is.Zero);
            Assert.That(clock.CurrentTick, Is.Zero);
            Assert.That(clock.AccumulatedSeconds, Is.EqualTo(0.75d).Within(1e-12d));
        }

        [Test]
        public void Advance_WhenDurationIsReached_EmitsSequentialTicksAndKeepsRemainder()
        {
            var clock = new MarketClock(1d);
            var observedTicks = new List<long>();
            clock.TickOccurred += tick => observedTicks.Add(tick.Index);

            long emittedTicks = clock.Advance(3.25d);

            Assert.That(emittedTicks, Is.EqualTo(3));
            Assert.That(observedTicks, Is.EqualTo(new long[] { 1, 2, 3 }));
            Assert.That(clock.CurrentTick, Is.EqualTo(3));
            Assert.That(clock.ElapsedSimulationSeconds, Is.EqualTo(3d));
            Assert.That(clock.AccumulatedSeconds, Is.EqualTo(0.25d).Within(1e-12d));
        }

        [Test]
        public void Advance_AccumulatesFractionalFrameTimeReliably()
        {
            var clock = new MarketClock(1d);

            for (int index = 0; index < 10; index++)
            {
                clock.Advance(0.1d);
            }

            Assert.That(clock.CurrentTick, Is.EqualTo(1));
            Assert.That(clock.AccumulatedSeconds, Is.Zero.Within(1e-9d));
        }

        [Test]
        public void Advance_WhilePaused_DiscardsElapsedTime()
        {
            var clock = new MarketClock(1d);
            clock.Advance(0.5d);
            clock.SetPaused(true);

            long pausedTicks = clock.Advance(10d);
            clock.SetPaused(false);
            long resumedTicks = clock.Advance(0.5d);

            Assert.That(pausedTicks, Is.Zero);
            Assert.That(resumedTicks, Is.EqualTo(1));
            Assert.That(clock.CurrentTick, Is.EqualTo(1));
        }

        [Test]
        public void Restore_ReinstatesProgressAndPauseState()
        {
            var source = new MarketClock(1d);
            source.Advance(4.4d);
            source.SetPaused(true);

            var restored = new MarketClock(1d);
            restored.Restore(source.CaptureSnapshot());

            Assert.That(restored.CurrentTick, Is.EqualTo(4));
            Assert.That(restored.AccumulatedSeconds, Is.EqualTo(0.4d).Within(1e-12d));
            Assert.That(restored.IsPaused, Is.True);
            Assert.That(restored.Advance(2d), Is.Zero);
        }

        [TestCase(0d)]
        [TestCase(-1d)]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void Constructor_WhenDurationIsInvalid_Throws(double duration)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MarketClock(duration));
        }

        [TestCase(-0.1d)]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void Advance_WhenElapsedTimeIsInvalid_Throws(double elapsedSeconds)
        {
            var clock = new MarketClock(1d);

            Assert.Throws<ArgumentOutOfRangeException>(() => clock.Advance(elapsedSeconds));
        }

        [Test]
        public void Restore_WhenRemainderDoesNotFitTickDuration_Throws()
        {
            var clock = new MarketClock(1d);
            var invalidSnapshot = new MarketClockSnapshot(2, 1d, false);

            Assert.Throws<ArgumentException>(() => clock.Restore(invalidSnapshot));
        }
    }
}
