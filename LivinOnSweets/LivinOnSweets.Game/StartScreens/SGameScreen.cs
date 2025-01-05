using System;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Container;
using LivinOnSweets.API.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osuTK;

namespace LivinOnSweets.Game.StartScreens
{
    // The Game Container handles the backing control
    // This screen is a middleware between the game state, in startup you cannot control the game at all, only display the content inside of it
    // in this screen you will be able to control the game, while the game container also manages the behaviour for the runtime state
    // which handles the screening on the startup screen
    public partial class SGameScreen : SweetScreen, IKeyBindingHandler<ManiaAction>
    {
        [Resolved]
        private GameStateManager stateManager { get; set; }

        public double TransitionDuration = 625; // 1250 / 2

        public GameContainer BoundContainer { get; }
        public Box TransitionBackground { get; protected set; } // Independant of StartupScreen or GameContainer
        public bool Transitioning { get; protected set; } = false;

        public SGameScreen(Func<Drawable, bool, bool> disposeAction, GameContainer container)
        {
            // Made it like this so when using the built in tools of o!f it doesnt crash because of a mutation outside of a specific thread
            Schedule(() =>
            {
                disposeAction(container, false);
                AddInternal(container);
                ChangeInternalChildDepth(TransitionBackground, -1); // change the depth so the transition background is displayed over the newly added container
            });

            AddInternal(TransitionBackground = new Box()
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colour4.Black,
            });

            BoundContainer = container;
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            TransitionBackground.FadeOutFromOne(TransitionDuration, Easing.OutQuint);
            BoundContainer.GameMargin.Value = new MarginPadding(0);
            BoundContainer.Size = Vector2.Zero;
            BoundContainer.RelativeSizeAxes = Axes.Both;
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            // Startup screen handled this but because the transition is making the code shit itself, im managing the swap in here
            if (e.Destination is StartupScreen startScreen)
                startScreen.SwapGameCtx(BoundContainer);

            stateManager.CanBack.Value = false; // #1 block the back button since its in the middle of transition
            stateManager.ProgressionBlock.Value = true; // block the progression until set back in startup screen
            return false;
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            bool handled = false;
            if (!Transitioning && e.Action == ManiaAction.BACK)
            {
                handled = true;
                Transitioning = true;
                // Half the default duration because when resuming the previous screen (startup screen) it also makes a fade
                TransitionBackground.FadeInFromZero(TransitionDuration / 2, Easing.OutQuint).OnComplete((_) => GoBack());
            }

            return handled;
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }

        protected virtual void GoBack()
        {
            RemoveInternal(BoundContainer, false);
            stateManager.UpdateRuntimeState(true, true);
            ScreenStack.Exit();
        }
    }
}
