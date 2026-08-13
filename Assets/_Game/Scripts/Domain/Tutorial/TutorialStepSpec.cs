using System;

namespace StockMarket.Domain.Tutorial
{
    public sealed class TutorialStepSpec
    {
        public TutorialStepSpec(string id, TutorialTrigger trigger, string prerequisiteStepId = "")
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Tutorial step ID is required.", nameof(id));
            }

            if (!Enum.IsDefined(typeof(TutorialTrigger), trigger))
            {
                throw new ArgumentOutOfRangeException(nameof(trigger));
            }

            Id = id;
            Trigger = trigger;
            PrerequisiteStepId = prerequisiteStepId ?? string.Empty;
        }

        public string Id { get; }
        public TutorialTrigger Trigger { get; }
        public string PrerequisiteStepId { get; }
    }
}
