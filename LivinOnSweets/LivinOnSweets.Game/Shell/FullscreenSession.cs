using JetBrains.Annotations;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Graphics.Containers;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.Screens;
using LivinOnSweets.API.StateMachines;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osuTK;

namespace LivinOnSweets.Game.Shell
{
    // aka "GameMiddlewareScreen", "SGameScreen"
    public partial class FullscreenSession : SweetScreen, IKeyBindingHandler<ManiaAction>
    {
        [Resolved] private GameStateManager stateManager { get; set; }
        [Resolved] private GameSession gameSession { get; set; }

        private Container content;
        private readonly Box fadeOverlay = new() { RelativeSizeAxes = Axes.Both, Colour = Colour4.Black, Depth = -1 };
        private bool transitioning;
        [CanBeNull] private GameView gameView;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = content = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Children = [fadeOverlay]
            };

            gameSession.RequestOwnership(typeof(FullscreenSession), content, gv =>
            {
                gameView = gv;
                setGameVisuals();
            });
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            fadeOverlay.FadeOutFromOne(500D, Easing.OutQuint);
            gameSession.InputEnabled.Value = true;
            ScheduleAfterChildren(() => gameSession.RequestGameFocus()); // not alive here, schedule it after children since its alive then
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            if (transitioning || e.Action != ManiaAction.BACK) return false;

            // analyzes the point in game where we are
            RuntimeState runtimeState = stateManager.RuntimeMachine.CurrentState.Value;

            if (runtimeState != RuntimeState.InGame)
                return false;

            GameplayState gameState = stateManager.GameplayMachine.CurrentState.Value;
            if (gameState > GameplayState.Ready)
                return false;

            transitioning = true;
            fadeOverlay.FadeInFromZero(500, Easing.OutQuint).OnComplete(_ => cleanSession());

            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }

        private void setGameVisuals()
        {
            // kinda redundant but just to be safe (im tired)
            if (gameView == null)
                return;

            gameView.GameMargin.Value = new MarginPadding(0);
            gameView.ContainerSize.Value = Vector2.Zero;
            gameView.RelativeSizeAxes = Axes.Both;
        }

        private void cleanSession()
        {
            gameSession.RequestGameFocus(false);

            // we need to call the go back function which returns to the previous screen AFTER returning the ownership to nothing
            // this is to AVOID having the bindable events getting cleaned when exiting to the previous screen
            gameSession.ReleaseOwnership(goBack);
        }

        private void goBack()
        {
            // first exit this screen and then update the runtime to properly execute the transitions and resets
            ScreenStack.Exit();

            stateManager.RuntimeMachine.UpdateState(true);

            // block the backing until explicitly set on the previous screen
            stateManager.CanBack.Value = false;
            stateManager.ProgressionBlock.Value = true;
        }
    }
}
