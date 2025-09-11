using LivinOnSweets.API.Components;
using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.StateMachines;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;

namespace LivinOnSweets.API.Cursor
{
    // https://github.com/ppy/osu/blob/master/osu.Game/Graphics/Cursor/MenuCursorContainer.cs
    public partial class ModularCursorContainer : CursorContainer
    {
        private readonly BindableBool screenshotCursorVisibility = new(true);
        public override bool IsPresent => screenshotCursorVisibility.Value && base.IsPresent;

        private bool hideCursorOnNonMouseInput;

        public bool HideCursorOnNonMouseInput
        {
            get => hideCursorOnNonMouseInput;
            set
            {
                if (hideCursorOnNonMouseInput == value)
                    return;

                hideCursorOnNonMouseInput = value;
                updateState();
            }
        }

        private MouseInputDetector mouseInputDetector = null!;
        private readonly IBindable<bool> lastInputWasMouse = new BindableBool();

        private bool visible;

        [Resolved] private GameStateManager stateManager { get; set; }

        [BackgroundDependencyLoader]
        private void load(SessionConfig sessionConfig)
        {
            sessionConfig.BindWith(SessionSetting.ScreenshotCursorVisibility, screenshotCursorVisibility);

            Add(mouseInputDetector = new MouseInputDetector());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            lastInputWasMouse.BindTo(mouseInputDetector.LastInputWasMouseSource);
            lastInputWasMouse.BindValueChanged(_ => updateState(), true);
        }

        protected override void UpdateState(ValueChangedEvent<Visibility> state) => updateState();

        private void updateState()
        {
            bool combinedVisibility = getCursorVisibility();

            if (visible == combinedVisibility)
                return;

            visible = combinedVisibility;

            if (visible)
                PopIn();
            else
                PopOut();
        }

        private bool getCursorVisibility()
        {
            // do not display when explicitly set to hidden state.
            if (State.Value == Visibility.Hidden)
                return false;

            // only hide cursor when game is focused, otherwise it should always be displayed
            if (stateManager.GameplayMachine.CurrentState.Value == GameplayState.PlayingSong)
            {
                // do not display when last input is not mouse.
                if (hideCursorOnNonMouseInput && !lastInputWasMouse.Value)
                    return false;

                // i dont have a check if the game is idle
            }

            return true;
        }

        private partial class MouseInputDetector : Component
        {
            /// <summary>
            /// Whether the last input applied to the game is sourced from mouse.
            /// </summary>
            public IBindable<bool> LastInputWasMouseSource => lastInputWasMouseSource;

            private readonly Bindable<bool> lastInputWasMouseSource = new();

            public MouseInputDetector()
            {
                RelativeSizeAxes = Axes.Both;
            }

            protected override bool Handle(UIEvent e)
            {
                switch (e)
                {
                    case MouseDownEvent:
                    case MouseMoveEvent:
                        lastInputWasMouseSource.Value = true;
                        return false;

                    case KeyDownEvent { Repeat: false }:
                    case JoystickPressEvent:
                    case MidiDownEvent:
                        lastInputWasMouseSource.Value = false;
                        return false;
                }

                return false;
            }
        }
    }
}
