using System;
using System.Collections.Generic;

namespace StockMarket.Domain.Tutorial
{
    public sealed class TutorialState
    {
        private readonly HashSet<string> completed = new HashSet<string>(StringComparer.Ordinal);

        public TutorialState(IEnumerable<string> completedStepIds = null, bool isSkipped = false)
        {
            if (completedStepIds != null)
            {
                foreach (string id in completedStepIds)
                {
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        throw new ArgumentException("Completed tutorial IDs cannot be empty.");
                    }

                    completed.Add(id);
                }
            }

            IsSkipped = isSkipped;
        }

        public string ActiveStepId { get; private set; }
        public bool HasActiveStep => ActiveStepId != null;
        public bool IsSkipped { get; private set; }
        public int CompletedCount => completed.Count;

        public bool IsCompleted(string stepId) => stepId != null && completed.Contains(stepId);

        internal void Activate(string stepId)
        {
            ActiveStepId = stepId;
        }

        internal void CompleteActive()
        {
            if (ActiveStepId == null)
            {
                throw new InvalidOperationException("No tutorial step is active.");
            }

            completed.Add(ActiveStepId);
            ActiveStepId = null;
        }

        internal void Skip()
        {
            IsSkipped = true;
            ActiveStepId = null;
        }
    }
}
