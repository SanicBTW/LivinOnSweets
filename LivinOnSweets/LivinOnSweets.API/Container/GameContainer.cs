using JetBrains.Annotations;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Data;
using LivinOnSweets.API.Enum;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
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
                    Colour = Colour4.White,
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
                    background.FadeIn(1000D, Easing.OutQuint).OnComplete((_) =>
                    {
                        SweetScreen nextScreen = screenData.CreateScreen();
                        if (nextScreen == null)
                        {
                            enableBacking();
                            background.FadeOut(500D, Easing.OutQuint);
                            // since we are not yet into the SGameScreen (middleware) we have to update the runtime state by ourselves and call on error which is the resume call from StartupScreen
                            stateManager.UpdateRuntimeState(true, true);
                            screenData.OnError?.Invoke();
                            return;
                        }

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
