using JetBrains.Annotations;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Data;
using LivinOnSweets.API.Enum;
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

        public BindableMarginPadding GameMargin = new(new MarginPadding()
        {
            Left = 88,
            Top = 100
        });

        private Box background;
        private ScreenStack screenStack;

        private LoadingSpinner spinner;
        private ScheduledDelegate spinnerShow;

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
                spinner = new LoadingSpinner(true, true)
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding(40)
                },
                // All of the content will be sized as 1280x720, to avoid issues with positioning and scaling artifacts
                new DrawSizePreservingFillContainer()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    TargetDrawSize = new Vector2(1280, 720), // OMFG THIS SAVED MY LIFE, I LOVE YOU DRAW SIZE PRESERVING FILL CONTAINER
                    Child = screenStack = new ScreenStack()
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Scale = new Vector2(0.8f),
                        Alpha = 0,
                    }
                }
            };

            GameMargin.BindValueChanged((ev) => Margin = ev.NewValue);
        }

        public void EnterGame(GameScreenData screenData)
        {
            switch (stateManager.GpState.Value)
            {
                case GameplayState.INITIALIZED:
                    screenData.OnLoad?.Invoke();
                    ScheduleAfterChildren(enableBacking);
                    break;

                case GameplayState.UNINITIALIZED:
                    // Only show the spinner if the game is uninitialized
                    spinnerShow = Scheduler.AddDelayed(spinner.Show, 100);

                    // i disabled most of the animations on first load since its blocked by the transition background on startup screen & sgame screen
                    SweetScreen nextScreen = screenData.CreateScreen();
                    if (nextScreen == null)
                    {
                        enableBacking();
                        // since we are not yet into the SGameScreen (middleware) we have to update the runtime state by ourselves and call on error which is the resume call from StartupScreen
                        stateManager.UpdateRuntimeState(true, true);
                        screenData.OnError?.Invoke();
                        checkSpinner(null);
                        return;
                    }

                    background.FadeIn(1000D, Easing.OutQuint).OnComplete((_) =>
                    {
                        LoadComponentAsync(nextScreen, _ =>
                        {
                            checkSpinner(nextScreen);

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

        private void checkSpinner([CanBeNull] SweetScreen nextScreen)
        {
            spinnerShow?.Cancel();

            if (spinner.State.Value == Visibility.Visible)
            {
                spinner.Hide();

                if (nextScreen != null)
                {
                    screenStack.Push(nextScreen);
                    screenStack
                        .FadeTo(1, LoadingSpinner.TRANSITION_DURATION / 2, Easing.OutQuint)
                        .ScaleTo(1, LoadingSpinner.TRANSITION_DURATION, Easing.OutQuart);
                }
            }
            else
            {
                if (nextScreen != null)
                    screenStack.Push(nextScreen);
            }
        }
    }
}
