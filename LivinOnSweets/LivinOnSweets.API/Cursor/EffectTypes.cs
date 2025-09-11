using JetBrains.Annotations;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;

namespace LivinOnSweets.API.Cursor
{
    // default interface to unify all the effects and make them able to be added into the effects manager
    public interface ICursorEffect;

    public interface ICursorReceiverEffect : ICursorEffect
    {
        void OnCursorChanged([CanBeNull] CursorContainer oldCursor, [CanBeNull] CursorContainer newCursor);
    }

    public interface ICursorMovementEffect : ICursorEffect
    {
        void OnMouseMove(MouseMoveEvent e, CursorContainer cursor);
    }

    public interface IClickableCursorEffect : ICursorEffect
    {
        void OnMouseDown(MouseDownEvent e, CursorContainer cursor);
        void OnMouseUp(MouseUpEvent e, CursorContainer cursor);
        void OnDoubleClick(DoubleClickEvent e, CursorContainer cursor) { } // does double click even work?
    }
}
