using System;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Graphics.Containers;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.Screens;
using LivinOnSweets.API.StateMachines;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osuTK;

namespace LivinOnSweets.Game.Screens
{
    // aka "GameMiddlewareScreen", "SGameScreen"
    public partial class FullscreenSession : SweetScreen, IKeyBindingHandler<ManiaAction>
    {
        [Resolved] private GameStateManager stateManager { get; set; }
        [Resolved] private GameSession gameSession { get; set; }

        private Container content;
        private readonly Box fadeOverlay = new() { RelativeSizeAxes = Axes.Both, Colour = Colour4.Black, Depth = -1 };
        private bool transitioning;
        private readonly Bindable<GameView> gameView = new();

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = content = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
            };

            gameView.ValueChanged += setGameVisuals;
            gameSession.RequestOwnership(typeof(FullscreenSession), content, gameView);
            gameSession.OwnershipLocked.Value = true; // lock the ownership here in case of trying to change it (its the only thing visible here)
            Scheduler.AddOnce(() =>
            {
                if (gameView.Value == null)
                    throw new InvalidOperationException($"Failed to recover ownership over {nameof(gameView)}");
            });
            content.Add(fadeOverlay);
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
            fadeOverlay.FadeInFromZero(500, Easing.OutQuint).OnComplete(_ => goBack());

            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }

        private void setGameVisuals(ValueChangedEvent<GameView> ev)
        {
            GameView game = ev.NewValue;
            game.GameMargin.Value = new MarginPadding(0);
            game.ContainerSize.Value = Vector2.Zero;
            game.RelativeSizeAxes = Axes.Both;

            gameView.ValueChanged -= setGameVisuals;
        }

        private void goBack()
        {
            gameSession.RequestGameFocus(false);
            // first exit this screen and then update the runtime to properly execute the transitions and resets
            gameSession.OwnershipLocked.Value = false;
            // manually remove without disposing to avoid binding again
            content.Remove(gameView.Value, false);
            ScreenStack.Exit();

            stateManager.RuntimeMachine.UpdateState(true);

            // block the backing until explicitly set on the previous screen
            stateManager.CanBack.Value = false;
            stateManager.ProgressionBlock.Value = true;
        }
    }
}
