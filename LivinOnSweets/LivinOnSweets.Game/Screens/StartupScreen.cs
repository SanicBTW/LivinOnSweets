using LivinOnSweets.API.Components;
using LivinOnSweets.API.Container;
using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.StartupObjects;
using LivinOnSweets.API.Stores;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osuTK;

namespace LivinOnSweets.Game.Screens
{
    // TODO: Make the texture store scale be like 4 or 6?
    // TODO: Horrible variable naming, clean up
    // TODO: Fix being allowed to press enter again even if the banners anre not present yet
    // TODO: Afaik, target scale values for the game container are 0.4, 0.4 (i have to position it properly), so how do I get them based off the master container (SweetScrollContainer)
    public partial class StartupScreen : SweetScreen, IKeyBindingHandler<ManiaAction>
    {
        [Resolved]
        private GameStateManager stateManager { get; set; }

        protected SweetScrollContainer Container;
        protected BindableFloat ContainerScale = new(1);
        protected ClosePopup CloseModal;

        protected SpinningCD CD;

        protected DrawSizePreservingFillContainer CentralContainer;
        protected Container GameBgContainer;
        protected GameContainer GameContainer;

        protected Container<StudentBanner> Banners;
        protected StudentBanner LeftBanner;
        protected StudentBanner RightBanner;

        protected Sprite Footer;

        private double gameContainerDelay = 1250;
        private float gameContainerDelayFactor = 1.2f;
        private float animOffset = 150;
        private float minScrollShow = 140; // the minimum value the scroll container has to reach to show or animate sprites that are out of bounds

        private float lastScrollPos = 0;

        public StartupScreen()
        {
            InternalChildren =
                [
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.FromHex("#e1ddd7")
                    },
                    Container = new SweetScrollContainer(startBlocked: true)
                    {
                        Alpha = 0f,
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        ClampExtension = 40,
                        ScrollBarMaxAlpha = new BindableFloat(),
                    },
                    CloseModal = new ClosePopup()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 0f,
                    }
                ];

            ContainerScale.BindValueChanged((ev) =>
            {
                Vector2 newScale = new Vector2(ev.NewValue);
                Container.Scale = newScale;
                // GameContainer.Scale = Vector2.Divide(new Vector2(ContainerScale.Value), Vector2.Divide(Container.DrawSize, GameContainer.GameSize));
            });
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, PixelArtTextureStore pixArtStore, LargeTextureStore largeStore)
        {
            // CD
            Container.Add(new DrawSizePreservingFillContainer()
            {
                TargetDrawSize = new Vector2(898, 898), // Texture Size (859x858)
                Y = -4,
                Child = CD = new()
            });

            // Main
            Vector2 containerSize = new Vector2(1280, 905); // Debugger reported this size, so that's what I'm using rn, hours later: I added some extra (835 + 70) to account for the footer and margin of it
            Container.Add(CentralContainer = new()
            {
                RelativeSizeAxes = Axes.None,
                TargetDrawSize = containerSize,
                Size = containerSize,
                Strategy = DrawSizePreservationStrategy.Maximum,
                Margin = new MarginPadding()
                {
                    Top = 65
                },
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    Banners = new Container<StudentBanner>()
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new[]
                        {
                            LeftBanner = new(isLeft: true)
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.Centre,
                                Alpha = 0,
                                Margin = new()
                                {
                                    Bottom = 10,
                                    Left = 80
                                }
                            },

                            RightBanner = new()
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.Centre,
                                Alpha = 0,
                                Margin = new()
                                {
                                    Bottom = 20,
                                    Right = 39
                                }
                            }
                        }
                    },

                    GameBgContainer = new()
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new Sprite()
                            {
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                                Scale = new Vector2(0.8f),
                                Texture = largeStore.Get("Startup/UI/GameContainer.png"),
                                Depth = 2
                            },
                            GameContainer = new GameContainer(), // Depth 1
                            new Sprite()
                            {
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                                Scale = new Vector2(0.88f),
                                Margin = new MarginPadding()
                                {
                                    Top = 68,
                                    Left = 120
                                },
                                Texture = pixArtStore.Get("Startup/UI/Logo.png")
                            }
                        }
                    },
                }
            });

            CentralContainer.Add(Footer = new Sprite()
            {
                Origin = Anchor.BottomCentre,
                Anchor = Anchor.BottomCentre,
                Scale = new Vector2(1.5f),
                Alpha = 0,
                Margin = new MarginPadding()
                {
                    Bottom = 12
                },
                Texture = textures.Get("Startup/UI/Footer.png")
            });

            // Branding
            Container.Add(new Sprite()
            {
                Origin = Anchor.TopLeft,
                Anchor = Anchor.TopLeft,
                Margin = new MarginPadding()
                {
                    Left = 18,
                    Top = 16
                },
                Scale = new Vector2(1.7f),
                Texture = textures.Get("Startup/UI/Branding.png")
            });
        }

        // Screen loaded, entering in view (PUSH)
        public override void OnEntering(ScreenTransitionEvent e)
        {
            stateManager.ProgressionBlock.Value = true;

            PrepareCentral();
            AnimateCentral();

            base.OnEntering(e);
        }

        // Previous screen exited, resuming this screen (EXIT)
        public override void OnResuming(ScreenTransitionEvent e)
        {
            if (e.Last != null && e.Last is SGameScreen)
                GameContainer.GameMargin.SetDefault();

            ResizeGameContainer();
            SlideBanners(gameContainerDelay / gameContainerDelayFactor, gameContainerDelayFactor);
            CD.Slide();

            base.OnResuming(e);
        }

        protected override void Update()
        {
            base.Update();

            // ehhhhh
            if (!Footer.IsPresent && Container.Current >= minScrollShow)
                ShowFooter();
        }

        protected virtual void PrepareCentral()
        {
            CentralContainer.MoveToOffset(new Vector2(0, animOffset));

            float bannerAnimOffset = animOffset * 2;
            LeftBanner.SetupSlideIn(bannerAnimOffset);
            RightBanner.SetupSlideIn(bannerAnimOffset); // gets multiplied by -1 because it aint the left side
            Footer.MoveToOffset(new Vector2(0, animOffset));
        }

        protected virtual void AnimateCentral()
        {
            Container.FadeIn(850);
            CentralContainer.FadeTo(0); // cancel the fade animation from its parent container (Container)

            Scheduler.AddDelayed(() =>
            {
                double delay = gameContainerDelay;
                CentralContainer.FadeIn(delay).MoveToOffset(new Vector2(0, -animOffset), delay, Easing.OutQuint);

                delay /= gameContainerDelayFactor;
                Scheduler.AddDelayed(() => SlideBanners(delay, gameContainerDelayFactor), delay);
                Scheduler.AddDelayed(FinishingAnimation, delay * 1.5f);
            }, 500);
        }

        protected virtual void SlideBanners(double delay, float factor, bool transIn = true)
        {
            double moveDelay = delay * factor;
            double fadeDelay = delay / factor; // decrease more the delay for the fade in

            foreach (StudentBanner banner in Banners)
                banner.FadeNSlideWrap(transIn, fadeDelay, moveDelay);
        }

        protected virtual void ShowFooter()
        {
            double delay = gameContainerDelay;
            float factor = gameContainerDelayFactor;

            double moveDelay = delay * factor;
            double fadeDelay = delay / factor; // decrease more the delay for the fade in
            Footer.FadeIn(fadeDelay).MoveToOffset(new Vector2(0, -animOffset), moveDelay, Easing.OutQuint);
        }

        protected virtual void FinishingAnimation()
        {
            CD.Slide();

            Container.ScrollBarMaxAlpha.Default = 0.75f;
            Container.ScrollBarMaxAlpha.SetDefault();
            Container.AllowScroll();
            Container.ScrollBy(0.1f); // trigger the scroll event to show that you can now scroll

            stateManager.ProgressionBlock.Value = false;
            stateManager.RTState.BindValueChanged(ProcessRTState); // dont trigger since the banners are already mid animation prob
        }

        protected virtual void ResizeGameContainer(bool transIn = true)
        {
            if (!transIn)
            {
                lastScrollPos = Container.Current;
                Container.BlockScroll();
                Container.TransformBindableTo(Container.ScrollBarAlpha, 0, Container.AlphaDuration);

                bool notInit = stateManager.GPState.Value == GameplayState.UNINITIALIZED;
                Container.ScrollTo(notInit ? GameBgContainer[2] : GameBgContainer[1]); // because we dont change the depth of the sprite anymore, we have to properly index the target

                Container.Delay(500D)
                    .TransformBindableTo(ContainerScale, 4f, gameContainerDelay, Easing.OutQuint)
                    .Schedule(() => GameContainer.GameMargin.Value = new MarginPadding(0))
                    .Schedule(() => GameContainer.Scale = new Vector2(0.4f))
                    .OnComplete((_) =>
                {
                    // Since the screens are part of the game and not the API package we pass a type reference to the next screen that will be created thru activator
                    // When entering the game, let the game load first then after its done loading, change the current screen
                    GameContainer.EnterGame(notInit
                        ? typeof(TestScreen)
                        : null, () => ScreenStack.Push(new SGameScreen(GameBgContainer.Remove, GameContainer)));
                });
            }
            else
            {
                Container.TransformBindableTo(ContainerScale, 1f, gameContainerDelay / 2, Easing.OutQuint).OnComplete((_) =>
                {
                    Container.TransformBindableTo(Container.ScrollBarAlpha, 1, Container.AlphaDuration);
                    Container.ScrollTo(lastScrollPos);
                    Container.AllowScroll();
                });
            }
        }

        protected virtual void ProcessRTState(ValueChangedEvent<RuntimeState> ev)
        {
            RuntimeState newState = ev.NewValue;
            RuntimeState oldState = ev.OldValue;

            switch (newState)
            {
                // instead of fading the container, we make the close popup visible which has a bg that "fades" the whole container
                // TODO! Make the popup handle the back action
                case RuntimeState.CLOSE_PROMPT:
                    CloseModal.ToggleVisibility();
                    break;

                case RuntimeState.STARTUP:
                    switch (oldState)
                    {
                        case RuntimeState.CLOSE_PROMPT:
                            CloseModal.ToggleVisibility();
                            break;
                    }
                    break;

                case RuntimeState.IN_GAME:
                    stateManager.CanBack.Value = false; // First run should wait for the container to fully load
                    stateManager.ProgressionBlock.Value = true; // Block any possible progression
                    SlideBanners(gameContainerDelay / gameContainerDelayFactor, gameContainerDelayFactor, false);
                    CD.Slide(false);
                    ResizeGameContainer(false);
                    break;
            }
        }

        // passes the new game container, prob just a ref too
        public void SwapGameCtx(GameContainer game)
        {
            // Reset the properties
            game.RelativeSizeAxes = Axes.None;
            game.Size = game.GameSize;
            GameBgContainer.Add(GameContainer = game);
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            // Just in case a key is kept pressed
            if (e.Repeat)
                return false;

            RuntimeState runtimeState = stateManager.RTState.Value;

            switch (e.Action)
            {
                case ManiaAction.REFRESH:
                    stateManager.Reset();

                    ScreenStack.Exit();
                    ScreenStack.Push(new StartupScreen());
                    break;

                case ManiaAction.UI_UP:
                    GameContainer.Scale += new Vector2(0, 0.1f);
                    break;

                case ManiaAction.UI_DOWN:
                    GameContainer.Scale -= new Vector2(0, 0.1f);
                    break;

                case ManiaAction.UI_RIGHT:
                    GameContainer.Scale += new Vector2(0.1f, 0);
                    break;

                case ManiaAction.UI_LEFT:
                    GameContainer.Scale -= new Vector2(0.1f, 0);
                    break;

                case ManiaAction.CONFIRM:
                    stateManager.UpdateRuntimeState();
                    break;

                case ManiaAction.BACK:
                    stateManager.UpdateRuntimeState(true, true);
                    break;
            }

            return runtimeState != RuntimeState.IN_GAME; // If the player player IS NOT in game, stop the propagation
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }

    }
}
