using System;

namespace StockMarket.Domain.Market.Simulation
{
    /// <summary>
    /// Small deterministic generator whose state can later be included in save data.
    /// </summary>
    public sealed class SeededRandomSource
    {
        private const ulong ZeroSeedReplacement = 0x9E3779B97F4A7C15UL;
        private const double UnitDoubleScale = 1d / 9007199254740992d;

        private ulong state;

        public SeededRandomSource(ulong seed)
        {
            state = seed == 0UL ? ZeroSeedReplacement : seed;
        }

        public ulong State => state;

        public double NextUnitDouble()
        {
            return (NextUInt64() >> 11) * UnitDoubleScale;
        }

        public double NextSignedUnitDouble()
        {
            return (NextUnitDouble() * 2d) - 1d;
        }

        public void RestoreState(ulong restoredState)
        {
            if (restoredState == 0UL)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(restoredState),
                    "The deterministic random state cannot be zero.");
            }

            state = restoredState;
        }

        private ulong NextUInt64()
        {
            ulong value = state;
            value ^= value >> 12;
            value ^= value << 25;
            value ^= value >> 27;
            state = value;
            return value * 2685821657736338717UL;
        }
    }
}
