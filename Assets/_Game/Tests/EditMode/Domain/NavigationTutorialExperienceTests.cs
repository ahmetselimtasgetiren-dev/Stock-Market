using NUnit.Framework;
using StockMarket.Domain.Experience;
using StockMarket.Domain.Navigation;
using StockMarket.Domain.Tutorial;

namespace StockMarket.Domain.Tests
{
    public sealed class NavigationTutorialExperienceTests
    {
        [Test]
        public void Navigation_SeparatesScreensAndClosesOverlayBeforeGoingBack()
        {
            var navigation = new NavigationState();
            navigation.NavigateTo(GameScreen.Portfolio);
            navigation.OpenOverlay(OverlayType.Settings);

            Assert.That(navigation.GoBack(), Is.True);
            Assert.That(navigation.CurrentScreen, Is.EqualTo(GameScreen.Portfolio));
            Assert.That(navigation.HasOverlay, Is.False);
            Assert.That(navigation.GoBack(), Is.True);
            Assert.That(navigation.CurrentScreen, Is.EqualTo(GameScreen.Market));
        }

        [Test]
        public void Tutorial_RequiresPrerequisiteAndNeverRepeatsCompletedStep()
        {
            var state = new TutorialState();
            var tutorial = new TutorialService(
                state,
                new[]
                {
                    new TutorialStepSpec("welcome", TutorialTrigger.GameStarted),
                    new TutorialStepSpec("first_trade", TutorialTrigger.FirstTradeCompleted, "welcome")
                });

            Assert.That(tutorial.Signal(TutorialTrigger.FirstTradeCompleted), Is.Null);
            Assert.That(tutorial.Signal(TutorialTrigger.GameStarted), Is.EqualTo("welcome"));
            tutorial.CompleteActiveStep();
            Assert.That(tutorial.Signal(TutorialTrigger.GameStarted), Is.Null);
            Assert.That(tutorial.Signal(TutorialTrigger.FirstTradeCompleted), Is.EqualTo("first_trade"));
        }

        [Test]
        public void Tutorial_SkipClearsActiveAndSuppressesFutureSteps()
        {
            var tutorial = new TutorialService(
                new TutorialState(),
                new[] { new TutorialStepSpec("welcome", TutorialTrigger.GameStarted) });
            tutorial.Signal(TutorialTrigger.GameStarted);

            tutorial.SkipTutorial();

            Assert.That(tutorial.State.HasActiveStep, Is.False);
            Assert.That(tutorial.Signal(TutorialTrigger.GameStarted), Is.Null);
        }

        [Test]
        public void Feedback_RespectsNotificationAndMuteSettings()
        {
            var notifications = new NotificationCenter(3);
            var audio = new AudioCueQueue(3);
            var settings = new SettingsState();
            var feedback = new FeedbackService(notifications, audio, settings);
            settings.SetMuted(true);

            feedback.Publish(FeedbackType.TradeSucceeded, "trade.success", "trade_coin", 100);

            Assert.That(notifications.Count, Is.EqualTo(1));
            Assert.That(notifications.UnreadCount, Is.EqualTo(1));
            Assert.That(audio.Count, Is.Zero);
            Assert.That(notifications.MarkRead(notifications.Latest.Id), Is.True);
            Assert.That(notifications.UnreadCount, Is.Zero);
        }

        [Test]
        public void NotificationCenter_DropsOldestAndMaintainsUnreadCount()
        {
            var notifications = new NotificationCenter(2);
            NotificationRecord first = notifications.Publish(FeedbackType.TradeSucceeded, "one");
            notifications.MarkRead(first.Id);
            notifications.Publish(FeedbackType.DividendPaid, "two");
            notifications.Publish(FeedbackType.ContentUnlocked, "three");

            Assert.That(notifications.Count, Is.EqualTo(2));
            Assert.That(notifications[0].MessageKey, Is.EqualTo("two"));
            Assert.That(notifications.UnreadCount, Is.EqualTo(2));
        }

        [Test]
        public void Settings_ComputesEffectiveVolumesAndValidatesRange()
        {
            var settings = new SettingsState();
            settings.SetVolumes(0.5d, 0.8d, 0.4d);

            Assert.That(settings.EffectiveMusicVolume, Is.EqualTo(0.4d).Within(0.000001d));
            Assert.That(settings.EffectiveEffectsVolume, Is.EqualTo(0.2d).Within(0.000001d));
            settings.SetMuted(true);
            Assert.That(settings.EffectiveMusicVolume, Is.Zero);
            Assert.Throws<System.ArgumentOutOfRangeException>(() => settings.SetVolumes(2d, 1d, 1d));
        }

        [Test]
        public void Achievements_AreEarnedOnceWhenThresholdIsReached()
        {
            var achievements = new AchievementService(
                new[]
                {
                    new AchievementSpec("first_trade", AchievementMetric.CompletedTrades, 1),
                    new AchievementSpec("ten_trades", AchievementMetric.CompletedTrades, 10)
                });

            Assert.That(achievements.Evaluate(AchievementMetric.CompletedTrades, 1), Is.EqualTo(new[] { "first_trade" }));
            Assert.That(achievements.Evaluate(AchievementMetric.CompletedTrades, 1), Is.Empty);
            Assert.That(achievements.Evaluate(AchievementMetric.CompletedTrades, 10), Is.EqualTo(new[] { "ten_trades" }));
            Assert.That(achievements.EarnedCount, Is.EqualTo(2));
        }
    }
}
