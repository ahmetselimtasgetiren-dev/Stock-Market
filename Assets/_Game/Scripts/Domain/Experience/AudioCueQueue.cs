using System;

namespace StockMarket.Domain.Experience
{
    public sealed class AudioCueQueue
    {
        private readonly string[] cues;
        private int oldestIndex;

        public AudioCueQueue(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            cues = new string[capacity];
        }

        public int Count { get; private set; }
        public string this[int index] =>
            index < 0 || index >= Count
                ? throw new ArgumentOutOfRangeException(nameof(index))
                : cues[(oldestIndex + index) % cues.Length];

        public void Enqueue(string cueId)
        {
            if (string.IsNullOrWhiteSpace(cueId)) throw new ArgumentException(nameof(cueId));
            if (Count < cues.Length)
            {
                cues[(oldestIndex + Count) % cues.Length] = cueId;
                Count++;
            }
            else
            {
                cues[oldestIndex] = cueId;
                oldestIndex = (oldestIndex + 1) % cues.Length;
            }
        }
    }
}
