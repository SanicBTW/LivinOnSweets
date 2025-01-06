using LivinOnSweets.API.Components;
using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Input;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osuTK;
using OContainer = osu.Framework.Graphics.Containers.Container;

namespace LivinOnSweets.API.Overlays
{
    // TODO: Finish the design of this screen
    public partial class ClosePopup : OverlayContainer, IKeyBindingHandler<ManiaAction>
    {
        [Resolved]
        private GameStateManager stateManager { get; set; }

        [Resolved]
        private GameHost host { get; set; }

        private Box background;
        private OContainer content;

        public double FadeInDuration = 1300D;
        public double SlideYDuration = 1200D;

        public ClosePopup()
        {
            Depth = -99; // Show up in front of everything
            AddRangeInternal(new Drawable[]
            {
                background = new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black,
                    Alpha = 0.5f
                },
                content = new OContainer()
                {
                    Masking = true,
                    CornerRadius = 15,
                    Size = new Vector2(452, 240),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box()
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = Colour4.FromHex("#5b527e"),
                        },
                        new OContainer()
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Padding = new MarginPadding(16),
                            Children = new Drawable[]
                            {
                                new SpriteText()
                                {
                                    Text = "Are you sure you want to quit the game?"
                                }
                            }
                        }
                    },
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            stateManager.RTState.BindValueChanged(ProcessRtState);
            // Not working as expected, will come back to it later, for now the main thing is working
            // sanco here, 15 min later, read lazer source code to see how they do it, uhhh ill work on this another day
            // host.ExitRequested += ExitGame;

            content.Y = -DrawHeight;
        }

        protected override void PopIn()
        {
            content.MoveToY(0, SlideYDuration, Easing.OutQuint);
            this.FadeIn(FadeInDuration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            content.MoveToY(-DrawHeight, SlideYDuration, Easing.OutQuint);
            this.FadeOut(FadeInDuration, Easing.OutQuint);
        }

        protected virtual void ProcessRtState(ValueChangedEvent<RuntimeState> ev)
        {
            RuntimeState newState = ev.NewValue;
            RuntimeState oldState = ev.OldValue;

            switch (newState)
            {
                case RuntimeState.CLOSING:
                    content.MoveToY(-DrawHeight, SlideYDuration, Easing.OutQuint);
                    background.FadeIn(FadeInDuration, Easing.OutQuint).OnComplete((_) => ExitGame());
                    break;

                // (StartupScreen) instead of fading the container, we make the close popup visible which has a bg that "fades" the whole container
                case RuntimeState.CLOSE_PROMPT:
                    ToggleVisibility();
                    break;

                case RuntimeState.STARTUP:
                    switch (oldState)
                    {
                        case RuntimeState.CLOSE_PROMPT:
                            ToggleVisibility();
                            break;
                    }
                    break;
            }
        }

        protected virtual void ExitGame()
        {
            // Since we are closing we only fade the background, no need to slide the content
            if (State.Value != Visibility.Visible)
            {
                ToggleVisibility();
                content.Alpha = 0;
                background.FadeIn(FadeInDuration, Easing.OutQuint).OnComplete((_) => HostExit());
            }
            else
                HostExit();
        }

        protected virtual void HostExit()
        {
            if (host.CanExit)
                host.Exit();
            else if (host.CanSuspendToBackground)
                host.SuspendToBackground();
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            switch (e.Action)
            {
                case ManiaAction.CONFIRM:
                    // This will close the game
                    stateManager.UpdateRuntimeState();
                    break;

                case ManiaAction.BACK:
                    // This will close the popup and get back to the game
                    stateManager.UpdateRuntimeState(true);
                    break;
            }

            return stateManager.RTState.Value != RuntimeState.CLOSE_PROMPT;
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }
    }
}
