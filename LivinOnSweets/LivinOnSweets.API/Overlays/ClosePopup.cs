using LivinOnSweets.API.Components;
using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.Stores;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Platform;

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
        private Container content;

        public double FadeInDuration = 1300D;
        public double SlideYDuration = 1200D;

        public ClosePopup()
        {
            Depth = -98; // Show up in front of everything
            AddRangeInternal(new Drawable[]
            {
                background = new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black,
                    Alpha = 0.5f
                },
                content = new Container()
                {
                    Masking = true,
                    CornerRadius = 15,
                    // auto sized because the bg already sets the size
                    //Size = new Vector2(452, 240),
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(PixelArtTextureStore pixArt)
        {
            // Since the pixel art store has a scale adjust of 2, we need to set the texture to 1
            Texture texture = pixArt.Get("Startup/UI/ClosePopup.png");
            texture.ScaleAdjust = 1;

            content.Children = new Drawable[]
            {
                new Sprite()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = texture
                },
                new SpriteText()
                {
                    Font = new FontUsage(family: "DNFBitBit", size: 40F),
                    Text = "CLOSING",
                    Margin = new MarginPadding()
                    {
                        Left = 52,
                        Top = 4
                    }
                }
            };
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
