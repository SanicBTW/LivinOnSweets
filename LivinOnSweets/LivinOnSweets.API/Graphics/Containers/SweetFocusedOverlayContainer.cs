using LivinOnSweets.API.Input;
using LivinOnSweets.API.Overlays;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osuTK;

namespace LivinOnSweets.API.Graphics.Containers
{
    // https://github.com/ppy/osu/blob/master/osu.Game/Graphics/Containers/OsuFocusedOverlayContainer.cs
    public abstract partial class SweetFocusedOverlayContainer : FocusedOverlayContainer, IKeyBindingHandler<ManiaAction>
    {
        protected override bool BlockNonPositionalInput => true;

        protected readonly IBindable<OverlayActivation> OverlayActivationMode = new Bindable<OverlayActivation>(OverlayActivation.All);

        /// <summary>
        /// Temporary to allow for overlays in the main screen content to not dim themselves.
        /// Should be eventually replaced by dimming which is aware of the target dim container (traverse parent for certain interface type?).
        /// </summary>
        protected virtual bool DimMainContent => true;

        [Resolved]
        private IOverlayManager overlayManager { get; set; }

        protected override void LoadComplete()
        {
            if (overlayManager != null)
                OverlayActivationMode.BindTo(overlayManager.OverlayActivationMode);

            OverlayActivationMode.BindValueChanged(mode =>
            {
                if (mode.NewValue == OverlayActivation.Disabled)
                    State.Value = Visibility.Hidden;
            }, true);

            base.LoadComplete();
        }

        /// <summary>
        /// Whether mouse input should be blocked screen-wide while this overlay is visible.
        /// Performing mouse actions outside of the valid extents will hide the overlay.
        /// </summary>
        public virtual bool BlockScreenWideMouse => BlockPositionalInput;

        // receive input outside our bounds so we can trigger a close event on ourselves.
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => BlockScreenWideMouse || base.ReceivePositionalInputAt(screenSpacePos);

        private bool closeOnMouseUp;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            closeOnMouseUp = !base.ReceivePositionalInputAt(e.ScreenSpaceMousePosition);

            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (closeOnMouseUp && !base.ReceivePositionalInputAt(e.ScreenSpaceMousePosition))
                Hide();

            base.OnMouseUp(e);
        }

        public virtual bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case ManiaAction.BACK:
                    Hide();
                    return true;

                case ManiaAction.CONFIRM:
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e)
        {
        }

        protected override void UpdateState(ValueChangedEvent<Visibility> state)
        {
            switch (state.NewValue)
            {
                case Visibility.Visible:
                    if (OverlayActivationMode.Value == OverlayActivation.Disabled)
                    {
                        State.Value = Visibility.Hidden;
                        return;
                    }

                    if (BlockScreenWideMouse && DimMainContent) overlayManager?.ShowBlockingOverlay(this);
                    break;

                case Visibility.Hidden:
                    if (BlockScreenWideMouse) overlayManager?.HideBlockingOverlay(this);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            base.UpdateState(state);
        }

        protected override void PopOut() { }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            overlayManager?.HideBlockingOverlay(this);
        }
    }

}
