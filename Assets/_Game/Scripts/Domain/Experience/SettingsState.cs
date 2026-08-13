using System;

namespace StockMarket.Domain.Experience
{
    public sealed class SettingsState
    {
        public double MasterVolume { get; private set; } = 1d;
        public double MusicVolume { get; private set; } = 1d;
        public double EffectsVolume { get; private set; } = 1d;
        public bool IsMuted { get; private set; }
        public bool ReducedMotion { get; private set; }
        public bool NotificationsEnabled { get; private set; } = true;

        public void SetVolumes(double master, double music, double effects)
        {
            ValidateVolume(master);
            ValidateVolume(music);
            ValidateVolume(effects);
            MasterVolume = master;
            MusicVolume = music;
            EffectsVolume = effects;
        }

        public void SetMuted(bool muted) => IsMuted = muted;
        public void SetReducedMotion(bool reduced) => ReducedMotion = reduced;
        public void SetNotificationsEnabled(bool enabled) => NotificationsEnabled = enabled;
        public double EffectiveMusicVolume => IsMuted ? 0d : MasterVolume * MusicVolume;
        public double EffectiveEffectsVolume => IsMuted ? 0d : MasterVolume * EffectsVolume;

        private static void ValidateVolume(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0d || value > 1d)
                throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
}
