using System;
using System.Collections.Generic;

namespace StockMarket.Domain.Tutorial
{
    public sealed class TutorialService
    {
        private readonly TutorialStepSpec[] orderedSteps;

        public TutorialService(TutorialState state, IEnumerable<TutorialStepSpec> orderedSteps)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));

            if (orderedSteps == null)
            {
                throw new ArgumentNullException(nameof(orderedSteps));
            }

            var list = new List<TutorialStepSpec>();
            var ids = new HashSet<string>(StringComparer.Ordinal);

            foreach (TutorialStepSpec step in orderedSteps)
            {
                if (step == null || !ids.Add(step.Id))
                {
                    throw new ArgumentException("Tutorial steps are missing or duplicated.");
                }

                list.Add(step);
            }

            this.orderedSteps = list.ToArray();
        }

        public TutorialState State { get; }

        public string Signal(TutorialTrigger trigger)
        {
            if (State.IsSkipped || State.HasActiveStep)
            {
                return State.ActiveStepId;
            }

            for (int index = 0; index < orderedSteps.Length; index++)
            {
                TutorialStepSpec step = orderedSteps[index];

                if (step.Trigger == trigger && !State.IsCompleted(step.Id) &&
                    (string.IsNullOrEmpty(step.PrerequisiteStepId) || State.IsCompleted(step.PrerequisiteStepId)))
                {
                    State.Activate(step.Id);
                    return step.Id;
                }
            }

            return null;
        }

        public void CompleteActiveStep() => State.CompleteActive();
        public void SkipTutorial() => State.Skip();
    }
}
