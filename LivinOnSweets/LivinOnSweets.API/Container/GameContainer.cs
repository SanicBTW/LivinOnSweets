using JetBrains.Annotations;
using LivinOnSweets.API.Components;
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

        [Resolved]
        private ResizeHandler resizeHandler { get; set; }

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

        public void EnterGame([CanBeNull] Type nextScreen = null, Action onLoad = null)
        {
            switch (stateManager.GPState.Value)
            {
                case GameplayState.INITIALIZED:
                    onLoad?.Invoke();
                    ScheduleAfterChildren(enableBacking);
                    break;

                case GameplayState.UNINITIALIZED:
                    background.FadeTo(0.5f, 1000D, Easing.OutQuint).OnComplete((_) =>
                    {
                        if (nextScreen == null)
                        {
                            enableBacking();
                            background.FadeOut(1000D, Easing.OutQuint);
                            stateManager.UpdateRuntimeState(true, true);
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
                            screenStack.Push((SweetScreen)Activator.CreateInstance(nextScreen));
                            stateManager.GPState.Value = GameplayState.INITIALIZED;
                            enableBacking();
                            onLoad?.Invoke();
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
