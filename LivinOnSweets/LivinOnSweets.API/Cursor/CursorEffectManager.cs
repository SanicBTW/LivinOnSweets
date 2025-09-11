using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;

namespace LivinOnSweets.API.Cursor
{
    /// <summary>
    /// Class that manages various effects related to the mouse/touch input, completely modular and plugged into <see cref="ModularCursorDisplay"/>.
    /// </summary>
    public partial class CursorEffectManager : Component
    {
        private readonly List<ICursorEffect> effects = [];
        private readonly List<ICursorReceiverEffect> receiverEffects = [];
        private readonly List<IClickableCursorEffect> clickableEffects = [];
        private readonly List<ICursorMovementEffect> movementEffects = [];

        [CanBeNull] private CursorContainer currentCursor;

        public CursorEffectManager()
        {
            RelativeSizeAxes = Axes.Both;
        }

        // the effects will have on cursor changed called with the current cursor
        // to manipulate its content, add or delete it, in this case
        // add will pass the current cursor as new cursor to add new content
        // and remove will pass the current cursor as old to delete the added content

        public void AddEffect(ICursorEffect effect)
        {
            effects.Add(effect);

            if (effect is ICursorReceiverEffect receiver)
            {
                receiverEffects.Add(receiver);
                if (currentCursor != null)
                    receiver.OnCursorChanged(null, currentCursor);
            }

            if (effect is IClickableCursorEffect clickable)
                clickableEffects.Add(clickable);

            if (effect is ICursorMovementEffect movement)
                movementEffects.Add(movement);
        }

        public void RemoveEffect(ICursorEffect effect)
        {
            effects.Remove(effect);

            if (effect is ICursorReceiverEffect receiver)
            {
                receiverEffects.Remove(receiver);
                if (currentCursor != null)
                    receiver.OnCursorChanged(null, currentCursor);
            }

            if (effect is IClickableCursorEffect clickable)
                clickableEffects.Remove(clickable);

            if (effect is ICursorMovementEffect movement)
                movementEffects.Remove(movement);
        }

        public void ChangedCursor(CursorContainer newCursor)
        {
            foreach (ICursorReceiverEffect effect in receiverEffects)
                effect.OnCursorChanged(currentCursor, newCursor);

            currentCursor = newCursor;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (currentCursor == null || clickableEffects.Count <= 0)
                return base.OnMouseDown(e);

            foreach (IClickableCursorEffect eff in clickableEffects)
                eff.OnMouseDown(e, currentCursor);

            return base.OnMouseDown(e);
        }

        protected override bool OnDoubleClick(DoubleClickEvent e)
        {
            if (currentCursor == null || clickableEffects.Count <= 0)
                return base.OnDoubleClick(e);

            foreach (IClickableCursorEffect eff in clickableEffects)
                eff.OnDoubleClick(e, currentCursor);

            return base.OnDoubleClick(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (currentCursor == null || clickableEffects.Count <= 0)
                return;

            foreach (IClickableCursorEffect eff in clickableEffects)
                eff.OnMouseUp(e, currentCursor);

            base.OnMouseUp(e);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (currentCursor == null || movementEffects.Count <= 0)
                return base.OnMouseMove(e);

            foreach (ICursorMovementEffect eff in movementEffects)
                eff.OnMouseMove(e, currentCursor);

            return base.OnMouseMove(e);
        }
    }
}
