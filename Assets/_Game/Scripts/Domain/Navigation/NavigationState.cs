using System;
using System.Collections.Generic;

namespace StockMarket.Domain.Navigation
{
    public sealed class NavigationState
    {
        private readonly Stack<GameScreen> history = new Stack<GameScreen>();

        public NavigationState(GameScreen initialScreen = GameScreen.Market)
        {
            if (!Enum.IsDefined(typeof(GameScreen), initialScreen))
            {
                throw new ArgumentOutOfRangeException(nameof(initialScreen));
            }

            CurrentScreen = initialScreen;
        }

        public event Action Changed;
        public GameScreen CurrentScreen { get; private set; }
        public OverlayType CurrentOverlay { get; private set; }
        public bool HasOverlay => CurrentOverlay != OverlayType.None;
        public bool CanGoBack => HasOverlay || history.Count > 0;

        public void NavigateTo(GameScreen screen)
        {
            if (!Enum.IsDefined(typeof(GameScreen), screen))
            {
                throw new ArgumentOutOfRangeException(nameof(screen));
            }

            if (screen == CurrentScreen)
            {
                return;
            }

            CurrentOverlay = OverlayType.None;
            history.Push(CurrentScreen);
            CurrentScreen = screen;
            Changed?.Invoke();
        }

        public void OpenOverlay(OverlayType overlay)
        {
            if (overlay == OverlayType.None || !Enum.IsDefined(typeof(OverlayType), overlay))
            {
                throw new ArgumentOutOfRangeException(nameof(overlay));
            }

            if (CurrentOverlay == overlay)
            {
                return;
            }

            CurrentOverlay = overlay;
            Changed?.Invoke();
        }

        public bool GoBack()
        {
            if (HasOverlay)
            {
                CurrentOverlay = OverlayType.None;
                Changed?.Invoke();
                return true;
            }

            if (history.Count == 0)
            {
                return false;
            }

            CurrentScreen = history.Pop();
            Changed?.Invoke();
            return true;
        }

        public void Reset(GameScreen screen)
        {
            if (!Enum.IsDefined(typeof(GameScreen), screen))
            {
                throw new ArgumentOutOfRangeException(nameof(screen));
            }

            history.Clear();
            CurrentOverlay = OverlayType.None;
            CurrentScreen = screen;
            Changed?.Invoke();
        }
    }
}
