using JetBrains.Annotations;
using LivinOnSweets.API.Graphics.Containers;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Platform;

namespace LivinOnSweets.API.Components
{
    /// <summary>
    /// Simple component which manages the ownership of the <see cref="GameView"/> for the "embedded" game.
    /// </summary>
    public partial class GameSession : Component
    {
        [Resolved] private GameHost host { get; set; }

        [CanBeNull] private IFocusManager focusManager { get; set; }

        private readonly Bindable<Type> ownerType = new();
        private ValueChangedEvent<Container>? lastOwnerContainer;
        [CanBeNull] private Bindable<GameView> lastBindableOwner;
        private readonly Bindable<GameView> bidirectional = new();

        private readonly GameView gameView = new();

        /// <summary>
        /// A <see cref="BindableBool"/> which represents the lock state of the <see cref="gameView"/>.
        /// <para>If true then no ownership change will take effect since it's not able to do one.</para>
        /// </summary>
        public readonly BindableBool OwnershipLocked = new();

        /// <summary>
        /// A <see cref="BindableBool"/> to enable the input propagation into <see cref="gameView"/>.
        /// </summary>
        public readonly BindableBool InputEnabled = new();

        [BackgroundDependencyLoader]
        private void load(SingleThreadLoad stl)
        {
            // load the game view in the meantime
            stl.ScheduleLoad(gameView, null);

            ownerType.BindValueChanged(_ =>
            {
                ValueChangedEvent<Container> last = lastOwnerContainer!.Value;

                if (last.OldValue != null && last.OldValue.Contains(gameView))
                    last.OldValue.Remove(gameView, false);

                // last bindable now gets reset inside the ownership, kinda crazy honestly

                bidirectional.Value = gameView;
                last.NewValue.Add(gameView);
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            focusManager = GetContainingFocusManager();
        }

        // this is really overworked honestly, now it needs a type that when changed it will change the ownership
        // since tracking the container would be a problem since its not triggering because its probably the same instance
        public void RequestOwnership(Type newOwnerType, Container newOwnerContainer, [CanBeNull] Bindable<GameView> gameViewBindable = null)
        {
            if (OwnershipLocked.Value)
            {
                if (gameViewBindable != null) gameViewBindable.Value = null;
                return;
            }

            if (!lastOwnerContainer.HasValue)
                lastOwnerContainer = new ValueChangedEvent<Container>(null, newOwnerContainer);
            else
            {
                ValueChangedEvent<Container> lastOwner = lastOwnerContainer.Value;
                lastOwnerContainer = new ValueChangedEvent<Container>(lastOwner.NewValue, newOwnerContainer);
            }

            if (lastBindableOwner != null)
            {
                bidirectional.Value = null;
                bidirectional.UnbindFrom(lastBindableOwner);
            }

            bidirectional.BindTo(gameViewBindable);
            lastBindableOwner = gameViewBindable;

            // the internal scheduler of this component was kinda fucking everything so i opted
            // to schedule it in the update thread and surprisingly it works
            // the render thread was a bit rude but I put faith into the update thread to treat me nicely and not misbehave. I'll give it a cookie if it works
            host.UpdateThread.Scheduler.AddOnce(() => ownerType.Value = newOwnerType);
        }

        // leaving this here just in case i come back someday but its pretty broken, losing the focus on a click or just losing it altogether
        public bool RequestGameFocus(bool focus = true)
        {
            Drawable target = focus ? gameView : null;
            bool focusRes = focusManager?.ChangeFocus(target) ?? false;
            if (focusRes && target != null)
                focusManager?.TriggerFocusContention(target);
            return focusRes;
        }
    }
}
