using System;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Container;
using LivinOnSweets.API.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osuTK;

namespace LivinOnSweets.Game.Screens
{
    public partial class SGameScreen : SweetScreen, IKeyBindingHandler<ManiaAction>
    {
        [Resolved]
        private GameStateManager stateManager { get; set; }

        private GameContainer boundContainer;
        public GameContainer BoundContainer => boundContainer;

        public SGameScreen(Func<Drawable, bool, bool> disposeAction, GameContainer container)
        {
            // Made it like this so when using the built in tools of o!f it doesnt crash because of a mutation outside of a specific thread
            Schedule(() =>
            {
                disposeAction(container, false);
                container.Zoom *= 2.75f;
                AddInternal(container);
            });

            boundContainer = container;
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            boundContainer.TransformTo("Zoom", 1f, 1000D, Easing.OutQuint);
            boundContainer.GameMargin.Value = new MarginPadding(0);
            boundContainer.Size = Vector2.Zero;
            boundContainer.RelativeSizeAxes = Axes.Both;
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            // Startup screen handled this but because the transition is making the code shit itself, im managing the swap in here
            if (e.Destination is StartupScreen startScreen)
                startScreen.SwapGameCtx(boundContainer);

            return false;
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            switch (e.Action)
            {
                case ManiaAction.BACK:
                    RemoveInternal(boundContainer, false);
                    stateManager.UpdateRuntimeState(true, true);
                    ScreenStack.Exit();
                    return true;

                default:
                    return false;
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }
    }
}
