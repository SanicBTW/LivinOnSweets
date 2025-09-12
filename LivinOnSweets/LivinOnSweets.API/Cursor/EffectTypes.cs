using JetBrains.Annotations;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osuTK;

namespace LivinOnSweets.API.Cursor
{
    // default interface to unify all the effects and make them able to be added into the effects manager
    public interface ICursorEffect;

    public interface ICursorReceiver : ICursorEffect
    {
        void OnCursorChanged([CanBeNull] CursorContainer oldCursor, [CanBeNull] CursorContainer newCursor);
    }

    public interface ICursorMovementEffect : ICursorEffect
    {
        void OnMouseMove(MouseMoveEvent e, CursorContainer cursor) { OnMove(e.MousePosition); }
        void OnTouchMove(TouchMoveEvent e, CursorContainer cursor) { OnMove(e.MousePosition); }
        void OnMove(Vector2 pos);
    }

    public interface ICursorPressEffect : ICursorEffect
    {
        void OnMouseDown(MouseDownEvent e, CursorContainer cursor) { OnPressDown(e.MousePosition); }
        void OnMouseUp(MouseUpEvent e, CursorContainer cursor) { OnPressUp(e.MousePosition); }
        void OnTouchDown(TouchDownEvent e, CursorContainer cursor) { OnPressDown(e.MousePosition); }
        void OnTouchUp(TouchUpEvent e, CursorContainer cursor) { OnPressUp(e.MousePosition); }
        void OnPressDown(Vector2 pos);
        void OnPressUp(Vector2 pos);
    }
}
