using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using osu.Framework.Input.StateChanges;

namespace LivinOnSweets.API.Cursor
{
    // https://github.com/ppy/osu/blob/master/osu.Game/Graphics/Cursor/GlobalCursorDisplay.cs
    public partial class ModularCursorDisplay : Container, IProvideCursor
    {
        protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both };

        /// <summary>
        /// Control whether any cursor should be displayed.
        /// </summary>
        internal bool ShowCursor = true;

        public CursorContainer Cursor { get; }

        public bool ProvidingUserCursor => true;

        private InputManager inputManager = null!;

        [CanBeNull] private IProvideCursor currentOverrideProvider;

        [Cached] private CursorEffectManager effectManager;

        public ModularCursorDisplay()
        {
            AddRangeInternal(
            [
                Content = new Container { RelativeSizeAxes = Axes.Both },
                Cursor = new ModularCursorContainer { State = { Value = Visibility.Hidden } },
                effectManager = new CursorEffectManager(),
            ]);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager()!;
        }

        protected override void Update()
        {
            base.Update();

            var lastMouseSource = inputManager.CurrentState.Mouse.LastSource;
            bool hasValidInput = lastMouseSource != null && (lastMouseSource is not ISourcedFromTouch);

            if (!hasValidInput || !ShowCursor)
            {
                currentOverrideProvider?.Cursor?.Hide();
                currentOverrideProvider = null;
                return;
            }

            IProvideCursor newOverrideProvider = this;

            foreach (var d in inputManager.HoveredDrawables)
            {
                if (d is IProvideCursor p && p.ProvidingUserCursor)
                {
                    newOverrideProvider = p;
                    break;
                }
            }

            if (currentOverrideProvider == newOverrideProvider)
                return;

            effectManager.ChangedCursor(newOverrideProvider.Cursor);
            currentOverrideProvider?.Cursor?.Hide();
            newOverrideProvider.Cursor?.Show();

            currentOverrideProvider = newOverrideProvider;
        }
    }
}
