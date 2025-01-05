using LivinOnSweets.API.Components;
using LivinOnSweets.API.Data;
using LivinOnSweets.API.Enum;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osuTK;

namespace LivinOnSweets.API.Container
{
    public partial class GameContainer : ZoomeableContainer
    {
        [Resolved]
        private GameStateManager stateManager { get; set; }

        public Vector2 GameSize = new(820, 461);

        public BindableMarginPadding GameMargin = new(new MarginPadding()
        {
            Left = 88,
            Top = 100
        });

        private Box background;
        private ScreenStack screenStack;

        public GameContainer()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            ClipAnchor(Anchor);
            ClipOrigin(Origin);

            RelativeSizeAxes = Axes.None;
            Size = GameSize;

            Margin = GameMargin.Default;

            Children = new Drawable[]
            {
                background = new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.FromHex("#3a3a3a"), // color of the game container,
                    Alpha = 0f,
                },
            };

            GameMargin.BindValueChanged((ev) => Margin = ev.NewValue);
        }

        public void EnterGame(GameScreenData screenData)
        {
            switch (stateManager.GPState.Value)
            {
                case GameplayState.INITIALIZED:
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
                        LoadComponentAsync(screenStack = new ScreenStack()
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                        }, stack =>
                        {
                            Add(screenStack);
                            screenStack.Push(nextScreen);

                            stateManager.GPState.Value = GameplayState.INITIALIZED;
                            enableBacking();
                            screenData.OnLoad?.Invoke();
                        });
                    });
                    break;
            }
        }

        private void enableBacking()
        {
            stateManager.ProgressionBlock.SetDefault();
            stateManager.CanBack.SetDefault();
        }
    }
}
