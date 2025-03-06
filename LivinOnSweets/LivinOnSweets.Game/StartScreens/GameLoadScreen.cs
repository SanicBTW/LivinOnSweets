using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.Interfaces;
using LivinOnSweets.API.Sprites;
using LivinOnSweets.API.Stores;
using LivinOnSweets.Game.GameScreens;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osuTK;
using osuTK.Input;

namespace LivinOnSweets.Game.StartScreens
{
    // Renamed to GameLoadScreen since it loads the main game screen (MainMenuScreen)
    // Which could be confused with the new LoadingScreen inside GameScreens namespace
    // TODO: Add logo and fix progression
    public partial class GameLoadScreen : SweetScreen, IKeyBindingHandler<ManiaAction>
    {
        private SweetScreen nextScreen;
        private bool isReady() => nextScreen != null && nextScreen.LoadState == LoadState.Ready;

        private LoadingBar bgLoadingHandle;
        private SpriteText bgLoadingText;
        private bool sining;

        private Box fadeOverlay;
        private bool transitioning;

        public GameLoadScreen()
        {
            ValidForResume = false;
        }

        // Screen is loaded, currently visible, lets load shit up
        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            // lets delay the load a lil bit
            Scheduler.AddDelayed(() =>
            {
                LoadComponentAsync(nextScreen = CreateNextScreen());
                ScheduleAfterChildren(updateProgress);
            }, 500D);
        }

        [BackgroundDependencyLoader]
        private void load(MainMenuStore mmStore)
        {
            // Kind of a hacky way if you ask me, this represents the "music controls" visible in the background
            // it mimics its layout size, only to position the "loading bar" (only the handle sprite) on the proper
            // position of the background, i could use hardcoded positions but im full of "what ifs"
            Container bgMusicOverlay = new Container()
            {
                Size = new Vector2(616, 182),
                Position = new Vector2(60, 486),
                Child = new Container()
                {
                    Size = new Vector2(616, 98),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Children =
                    [
                        bgLoadingHandle = new LoadingBar(),
                        bgLoadingText = new SpriteText()
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "NOW LOADING...",
                            // More accurate position and size relative to the OG version, the letters are a bit offseted on that version
                            Font = new FontUsage(family: "DNFBitBit", size: 58F),
                            Margin = new MarginPadding() { Right = 8, Bottom = 10 }
                        }
                    ]
                }
            };

            // This is made to properly position Airi with the background
            // Since the container "resizes", Airi is anchored to the Centre Right of her hitbox, which anchors to the
            // right part of the screen and when resized it wouldnt look correctly
            InternalChild = new Container()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Sprite()
                    {
                        Texture = mmStore.Get("MainMenu/Backgrounds/LoadScreen.png"),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new AiriVibe(),
                    bgMusicOverlay,
                    fadeOverlay = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0f,
                        Colour = Colour4.Black
                    }
                }
            };

        }

        protected override void Update()
        {
            base.Update();

            if (bgLoadingHandle.Progress >= 1)
                sineWaitText();
            else
                ScheduleAfterChildren(updateProgress);
        }

        private void updateProgress()
        {
            if (nextScreen != null && nextScreen is IProgressReporter reporter)
                bgLoadingHandle.UpdateProgress(reporter.GetLoadProgress());
        }

        private void sineWaitText()
        {
            bgLoadingText.Text = "TOUCH TO START";

            if (!sining)
            {
                sining = true;

                bgLoadingText.Loop(t =>
                    t.FadeOut(750D).Then().FadeIn(750D));
            }
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (transitioning)
                return false;

            if (isReady() && e.Button == MouseButton.Left)
            {
                changeScreen();
                return true;
            }

            return false;
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            if (transitioning)
                return false;

            if (isReady() && e.Action == ManiaAction.CONFIRM)
            {
                changeScreen();
                return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }

        private void changeScreen()
        {
            transitioning = true;
            fadeOverlay.FadeInFromZero(1000D, Easing.OutQuint).OnComplete(_ =>
            {
                ScreenStack.Push(nextScreen);
            });
        }

        protected virtual SweetScreen CreateNextScreen() => new MainMenuScreen();

        private partial class LoadingBar : Container
        {
            public float Progress { get; private set; }
            private Sprite handle;

            public LoadingBar()
            {
                AutoSizeAxes = Axes.Y;
                Anchor = Anchor.BottomCentre;
                Origin = Anchor.BottomCentre;
                Margin = new MarginPadding() { Right = 12 }; // kind of a forced value but can i do anything else?
                Width = 512;
            }

            [BackgroundDependencyLoader]
            private void load(MainMenuStore mmStore)
            {
                Add(handle = new Sprite()
                {
                    Texture = mmStore.Get("MainMenu/UI/General/LoadingHandle.png"),
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft
                });
            }

            public void UpdateProgress(float progress)
            {
                if (float.IsNaN(progress))
                    return;

                Progress = progress;
                handle.MoveToX(progress * (Width - handle.DrawWidth), 1500D, Easing.OutQuint);
            }
        }
    }
}
