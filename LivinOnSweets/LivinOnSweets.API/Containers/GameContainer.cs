using JetBrains.Annotations;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Data;
using LivinOnSweets.API.Enums;
using LivinOnSweets.API.Sprites.UI;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osuTK;

namespace LivinOnSweets.API.Containers
{
    public partial class GameContainer : Container
    {
        [Resolved]
        private GameStateManager stateManager { get; set; }

        // Used to block the propagation of input events in the screen when unwanted (e.g runtime state is not in game)
        private bool propagateInput = true;
        public override bool PropagateNonPositionalInputSubTree => propagateInput;

        public override bool PropagatePositionalInputSubTree => propagateInput;

        public Vector2 GameSize = new(820, 461);
        public Vector2 TargetSize = new(1280, 720);
        public DrawSizePreservationStrategy Strategy = DrawSizePreservationStrategy.Minimum;

        public BindableMarginPadding GameMargin = new(new MarginPadding()
        {
            Left = 88,
            Top = 100
        });

        private Box background;
        private ScreenStack screenStack;

        // expose the spinner to indicate something is loading and its taking some time inside the game itself
        [Cached]
        private LoadingSpinner spinner;

        public GameContainer()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.None;
            Size = GameSize;

            Margin = GameMargin.Default;
            Masking = true;

            Children = new Drawable[]
            {
                background = new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.FromHex("#3a3a3a"), // color of the game container,
                    Alpha = 0f,
                },
                // All of the content will be sized as 1280x720, to avoid issues with positioning and scaling artifacts
                // This could change in the future however, allowing the users to uncap the size of the screens
                // thats actually great content for an update lmao
                new TransitionContainer()
                {
                    TargetDrawSize = TargetSize, // OMFG THIS SAVED MY LIFE, I LOVE YOU DRAW SIZE PRESERVING FILL CONTAINER
                    Strategy = Strategy,
                    Child = screenStack = new ScreenStack()
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Scale = new Vector2(0.8f),
                        Alpha = 0,
                    }
                },
                spinner = new LoadingSpinner(true, true)
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding(40)
                }
            };

            GameMargin.BindValueChanged((ev) => Margin = ev.NewValue);
            screenStack.ScreenPushed += ScreenStackOnScreenPushed;
        }

        public void EnterGame(GameScreenData screenData)
        {
            switch (stateManager.GpState.Value)
            {
                // sanco 24/2/25 - i noticed that this was only working if we were on the main menu, blocking the return to the game screen
                // if we were in other menu like song select and such, only marking the uninitialized value as a possible match fixes the issue

                // Only load up the next screen WHEN uninitialized, otherwise just invoke the load callback which does the magic
                default:
                    screenData.OnLoad?.Invoke();
                    ScheduleAfterChildren(enableBacking);
                    break;

                case GameplayState.UNINITIALIZED:
                    // i disabled most of the animations on first load since its blocked by the transition background on startup screen & sgame screen
                    SweetScreen nextScreen = screenData.CreateScreen();
                    if (nextScreen == null)
                    {
                        enableBacking();
                        // since we are not yet into the SGameScreen (middleware) we have to update the runtime state by ourselves and call on error which is the resume call from StartupScreen
                        stateManager.UpdateRuntimeState(true, true);
                        screenData.OnError?.Invoke();
                        return;
                    }

                    background.FadeIn(1000D, Easing.OutQuint).OnComplete((_) =>
                    {
                        LoadComponentAsync(nextScreen, _ =>
                        {
                            screenStack.Push(nextScreen);
                            screenStack
                                .FadeTo(1, LoadingSpinner.TRANSITION_DURATION / 2, Easing.OutQuint)
                                .ScaleTo(1, LoadingSpinner.TRANSITION_DURATION, Easing.OutQuart);

                            stateManager.GpState.Value = GameplayState.INITIALIZED;
                            enableBacking();
                            screenData.OnLoad?.Invoke();
                        });
                    });
                    break;
            }
        }

        public void EnableInput() => propagateInput = true;

        public void DisableInput() => propagateInput = false;

        private void enableBacking()
        {
            stateManager.ProgressionBlock.SetDefault();
            stateManager.CanBack.SetDefault();
        }

        // Made to cap the new screens to the target size (1280x720) while also masking them to
        // hide anything out of the target size, I don't really like this approach
        // because I think there's another way to do this but I don't want to do it manually
        // either so I'm gonna stay with this for now :grin:
        private void ScreenStackOnScreenPushed(IScreen lastScreen, IScreen newScreen)
        {
            SweetScreen currentScreen = (SweetScreen)newScreen;
            currentScreen.Anchor = currentScreen.Origin = Anchor.Centre;
            currentScreen.Masking = true;
            currentScreen.RelativeSizeAxes = Axes.None;
            currentScreen.Size = TargetSize;
        }
    }
}
