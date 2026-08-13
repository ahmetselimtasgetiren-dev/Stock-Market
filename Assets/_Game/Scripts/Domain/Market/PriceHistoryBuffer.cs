using System;
using System.Collections.Generic;

namespace StockMarket.Domain.Market
{
    /// <summary>
    /// Fixed-capacity price history exposed in oldest-to-newest order.
    /// </summary>
    public sealed class PriceHistoryBuffer
    {
        private readonly PricePoint[] points;
        private int oldestIndex;

        internal PriceHistoryBuffer(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "History capacity must be positive.");
            }

            points = new PricePoint[capacity];
        }

        public int Capacity => points.Length;

        public int Count { get; private set; }

        public PricePoint Oldest
        {
            get
            {
                EnsureNotEmpty();
                return points[oldestIndex];
            }
        }

        public PricePoint Latest
        {
            get
            {
                EnsureNotEmpty();
                return this[Count - 1];
            }
        }

        public PricePoint this[int chronologicalIndex]
        {
            get
            {
                if (chronologicalIndex < 0 || chronologicalIndex >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(chronologicalIndex));
                }

                int storageIndex = (oldestIndex + chronologicalIndex) % points.Length;
                return points[storageIndex];
            }
        }

        public void CopyTo(ICollection<PricePoint> destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            for (int index = 0; index < Count; index++)
            {
                destination.Add(this[index]);
            }
        }

        internal void Add(PricePoint point)
        {
            if (Count > 0 && point.Tick <= Latest.Tick)
            {
                throw new ArgumentException("Price history ticks must increase strictly.", nameof(point));
            }

            if (Count < points.Length)
            {
                int insertionIndex = (oldestIndex + Count) % points.Length;
                points[insertionIndex] = point;
                Count++;
                return;
            }

            points[oldestIndex] = point;
            oldestIndex = (oldestIndex + 1) % points.Length;
        }

        private void EnsureNotEmpty()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Price history is empty.");
            }
        }
    }
}
