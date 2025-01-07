using System;
using JetBrains.Annotations;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Container;
using LivinOnSweets.API.Data;
using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Extensions;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.Overlays;
using LivinOnSweets.API.Sprites;
using LivinOnSweets.API.Stores;
using LivinOnSweets.Game.GameScreens;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osuTK;

namespace LivinOnSweets.Game.StartScreens
{
    // TODO: Horrible variable naming, clean up / Incorrect variable naming and usage
    // TODO: When spamming enter/esc (BACK, CONFIRM) the scroll pos gets set to the container position (because it didn't have time to scroll to the old position), make it wait for a bit to save the new one
    // TODO: First press might lag a little bit, I don't know what's causing it
    public partial class StartupScreen : SweetScreen, IKeyBindingHandler<ManiaAction>
    {
        [Resolved]
        private GameStateManager stateManager { get; set; }

        protected SweetScrollContainer ScrollContainer;
        protected Box TransitionBackground;
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
                    ScrollContainer = new SweetScrollContainer(startBlocked: true)
                    {
                        Alpha = 0f,
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        ClampExtension = 40,
                        ScrollBarMaxAlpha = new BindableFloat(),
                    },
                    TransitionBackground = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.Black,
                        Alpha = 0,
                    },
                    CloseModal = new ClosePopup()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 0f,
                    }
                ];
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, PixelArtTextureStore pixArtStore, LargeTextureStore largeStore)
        {
            // CD
            ScrollContainer.Add(new DrawSizePreservingFillContainer()
            {
                TargetDrawSize = new Vector2(898, 898), // Texture Size (859x858)
                Y = -4,
                Child = CD = new()
            });

            // Main
            Vector2 containerSize = new Vector2(1280, 905); // Debugger reported this size, so that's what I'm using rn, hours later: I added some extra (835 + 70) to account for the footer and margin of it
            ScrollContainer.Add(CentralContainer = new()
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
            ScrollContainer.Add(new Sprite()
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

            // I was applying the accent when the banners finished loading, which would result on a few secs with the default color then changing to the accent
            // sanco here, i decided to use the left banner accent rather than the right one, since the color can blend in a lot, making the scrollbar kind of hard to see
            ScrollContainer.ApplyAccent(Banners[0]);
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
        public override void OnResuming([CanBeNull] ScreenTransitionEvent e)
        {
            if (e != null && e.Last != null && e.Last is SGameScreen)
            {
                GameContainer.GameMargin.SetDefault();
                stateManager.CanBack.SetDefault(); // #1 probably everything is finished, can back again | #2 wont be able to back until progression block is set to false
            }

            AnimateGameContainer();
            SlideBanners(gameContainerDelay / gameContainerDelayFactor, gameContainerDelayFactor);
            CD.Slide();

            base.OnResuming(e);
        }

        protected override void Update()
        {
            base.Update();

            // ehhhhh
            if (!Footer.IsPresent && ScrollContainer.Current >= minScrollShow)
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
            ScrollContainer.FadeIn(850);
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

            // Because we are applying an accent color to the scroll bar now, the color can be bright and blend with the banner
            // so to avoid that, we set the alpha to an opaque value
            ScrollContainer.ScrollBarMaxAlpha.Default = 1f;
            ScrollContainer.ScrollBarMaxAlpha.SetDefault();
            ScrollContainer.AllowScroll();
            ScrollContainer.ScrollBy(0.1f); // trigger the scroll event to show that you can now scroll

            stateManager.ProgressionBlock.SetDefault();
            stateManager.RTState.BindValueChanged(ProcessRtState); // dont trigger since the banners are already mid animation prob
        }

        protected virtual void AnimateGameContainer(bool transIn = true)
        {
            double duration = gameContainerDelay / 2;

            if (!transIn)
            {
                lastScrollPos = ScrollContainer.Current;
                ScrollContainer.BlockScroll();
                ScrollContainer.TransformBindableTo(ScrollContainer.ScrollBarAlpha, 0, ScrollContainer.AlphaDuration);

                bool notInit = stateManager.GPState.Value == GameplayState.UNINITIALIZED;
                ScrollContainer.ScrollTo(notInit ? GameBgContainer[2] : GameBgContainer[1]); // because we dont change the depth of the sprite anymore, we have to properly index the target

                TransitionBackground.Delay(500D)
                    .FadeInFromZero(duration, Easing.OutQuint)
                    .OnComplete((_) =>
                {
                    // Since the screens are part of the game and not the API package we pass a type reference to the next screen that will be created thru activator
                    // When entering the game, let the game load first then after its done loading, change the current screen
                    Type screenType = notInit
                        ? typeof(LoadingScreen)
                        : null;
                    Action onLoad = () => ScreenStack.Push(new SGameScreen(GameBgContainer.Remove, GameContainer));
                    Action onError = () => OnResuming(null); // when failing to create the next screen, call on resume to act like if we came back from another screen, resuming this context
                    GameContainer.EnterGame(new GameScreenData(screenType, onLoad: onLoad, onError: onError));
                });
            }
            else
            {
                TransitionBackground.Delay(500D)
                    .FadeOutFromOne(duration, Easing.OutQuint)
                    .OnComplete((_) =>
                    {
                        stateManager.ProgressionBlock.SetDefault(); // #1 probably everything is finished, can progress again
                        ScrollContainer.TransformBindableTo(ScrollContainer.ScrollBarAlpha, 1, ScrollContainer.AlphaDuration);
                        ScrollContainer.ScrollTo(lastScrollPos);
                        ScrollContainer.AllowScroll();
                    });
            }
        }

        protected virtual void ProcessRtState(ValueChangedEvent<RuntimeState> ev)
        {
            // Moved the close behaviour to close popup
            RuntimeState newState = ev.NewValue;

            switch (newState)
            {
                case RuntimeState.IN_GAME:
                    stateManager.CanBack.Value = false; // First run should wait for the container to fully load
                    stateManager.ProgressionBlock.Value = true; // Block any possible progression
                    SlideBanners(gameContainerDelay / gameContainerDelayFactor, gameContainerDelayFactor, false);
                    CD.Slide(false);
                    AnimateGameContainer(false);
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
                case ManiaAction.CONFIRM:
                    stateManager.UpdateRuntimeState();
                    break;

                case ManiaAction.BACK:
                    stateManager.UpdateRuntimeState(true, true);
                    break;
            }

            // If the action isn't a debug action AND the player IS NOT in game, stop the propagation
            return !e.Action.IsDebugAction() && runtimeState != RuntimeState.IN_GAME;
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }
    }
}
