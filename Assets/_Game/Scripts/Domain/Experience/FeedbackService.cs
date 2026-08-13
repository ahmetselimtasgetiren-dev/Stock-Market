using System;

namespace StockMarket.Domain.Experience
{
    public sealed class FeedbackService
    {
        private readonly NotificationCenter notifications;
        private readonly AudioCueQueue audio;
        private readonly SettingsState settings;

        public FeedbackService(NotificationCenter notifications, AudioCueQueue audio, SettingsState settings)
        {
            this.notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
            this.audio = audio ?? throw new ArgumentNullException(nameof(audio));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public void Publish(FeedbackType type, string messageKey, string audioCueId, long amountMinorUnits = 0)
        {
            if (settings.NotificationsEnabled)
                notifications.Publish(type, messageKey, amountMinorUnits);
            if (!settings.IsMuted && !string.IsNullOrWhiteSpace(audioCueId))
                audio.Enqueue(audioCueId);
        }
    }
}
