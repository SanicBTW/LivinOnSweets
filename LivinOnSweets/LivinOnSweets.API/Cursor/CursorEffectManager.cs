using JetBrains.Annotations;
using LivinOnSweets.API.Configuration;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;

namespace LivinOnSweets.API.Cursor
{
    /// <summary>
    /// Class that manages various effects related to the mouse/touch input, completely modular and plugged into <see cref="ModularCursorDisplay"/>.
    /// </summary>
    public partial class CursorEffectManager : CompositeDrawable
    {
        private readonly List<ICursorEffect> effects = [];
        private readonly List<ICursorReceiver> receiverEffects = [];
        private readonly List<ICursorPressEffect> clickableEffects = [];
        private readonly List<ICursorMovementEffect> movementEffects = [];

        [CanBeNull] private CursorContainer currentCursor;

        [Resolved] private SessionConfig sessionConfig { get; set; }

        private Bindable<bool> touchActive = new(RuntimeInfo.IsMobile);

        public CursorEffectManager()
        {
            RelativeSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            sessionConfig.BindWith(SessionSetting.TouchInputActive, touchActive);
        }

        // because of touch related issues, now the manager acts like a container that holds the effects inside it rather than using
        // the passed container through cursor changed, the reason is because when triggering a touch event, the cursor will hide
        // and since the effect gets added into the cursor container, then theres no effects, only showing up once a click is triggered
        public void AddEffect(ICursorEffect effect)
        {
            effects.Add(effect);

            if (effect is ICursorReceiver receiver)
            {
                receiverEffects.Add(receiver);
                if (currentCursor != null)
                    receiver.OnCursorChanged(null, currentCursor);
            }

            if (effect is ICursorPressEffect clickable)
                clickableEffects.Add(clickable);

            if (effect is ICursorMovementEffect movement)
                movementEffects.Add(movement);

            if (effect is CompositeDrawable drawable)
                AddInternal(drawable);
        }

        // this function thinks we only have ONE effect of the given type, but if more were to exist
        // then it will remove all the effects of the given type
        public void RemoveEffect(Type type)
        {
            foreach (ICursorEffect effect in effects)
            {
                Type efType = effect.GetType();
                if (efType != type)
                    continue;

                Schedule(() => RemoveEffect(effect));
            }
        }

        public void RemoveEffect(ICursorEffect effect)
        {
            effects.Remove(effect);

            if (effect is ICursorReceiver receiver)
            {
                receiverEffects.Remove(receiver);
                if (currentCursor != null)
                    receiver.OnCursorChanged(null, currentCursor);
            }

            if (effect is ICursorPressEffect clickable)
                clickableEffects.Remove(clickable);

            if (effect is ICursorMovementEffect movement)
                movementEffects.Remove(movement);

            if (effect is CompositeDrawable drawable)
                RemoveInternal(drawable, false);
        }

        public void ChangedCursor(CursorContainer newCursor)
        {
            foreach (ICursorReceiver effect in receiverEffects)
                effect.OnCursorChanged(currentCursor, newCursor);

            currentCursor = newCursor;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (currentCursor == null || clickableEffects.Count <= 0 || touchActive.Value)
                return base.OnMouseDown(e);

            foreach (ICursorPressEffect eff in clickableEffects)
                eff.OnMouseDown(e, currentCursor);

            return base.OnMouseDown(e);
        }

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            // probably redundant, since you cannot really trigger a touch event from a click
            if (currentCursor == null || clickableEffects.Count <= 0 || !touchActive.Value)
                return base.OnTouchDown(e);

            foreach (ICursorPressEffect eff in clickableEffects)
                eff.OnTouchDown(e, currentCursor);

            return base.OnTouchDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (currentCursor == null || clickableEffects.Count <= 0 || touchActive.Value)
                return;

            foreach (ICursorPressEffect eff in clickableEffects)
                eff.OnMouseUp(e, currentCursor);

            base.OnMouseUp(e);
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            if (currentCursor == null || clickableEffects.Count <= 0 || !touchActive.Value)
                return;

            foreach (ICursorPressEffect eff in clickableEffects)
                eff.OnTouchUp(e, currentCursor);

            base.OnTouchUp(e);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (currentCursor == null || movementEffects.Count <= 0 || touchActive.Value)
                return base.OnMouseMove(e);

            foreach (ICursorMovementEffect eff in movementEffects)
                eff.OnMouseMove(e, currentCursor);

            return base.OnMouseMove(e);
        }

        protected override void OnTouchMove(TouchMoveEvent e)
        {
            if (currentCursor == null || movementEffects.Count <= 0 || !touchActive.Value)
                return;

            foreach (ICursorMovementEffect eff in movementEffects)
                eff.OnTouchMove(e, currentCursor);

            base.OnTouchMove(e);
        }
    }
}
