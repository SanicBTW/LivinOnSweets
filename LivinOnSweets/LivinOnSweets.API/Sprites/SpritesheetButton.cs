using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osuTK.Input;

namespace LivinOnSweets.API.Sprites
{
    // A button whose states are represented by a spritesheet frames (active, idle)
    // Each implementation will behave differently but will most likely render using some spritesheet frames
    public abstract partial class SpritesheetButton : Sprite
    {
        protected Texture IdleTexture;
        protected Texture ActiveTexture;

        protected void SetFrames(Texture[] frames)
        {
            Texture = IdleTexture = frames[0];
            ActiveTexture = frames[1];
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            Texture = MouseUpTexture();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            Texture = MouseDownTexture();
            return true;
        }

        protected override bool OnClick(ClickEvent e)
        {
            FireEvent();
            return true;
        }

        protected abstract void FireEvent();
        protected virtual Texture MouseUpTexture() => IdleTexture;
        protected virtual Texture MouseDownTexture() => ActiveTexture;
    }
}
