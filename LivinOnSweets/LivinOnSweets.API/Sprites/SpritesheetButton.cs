using JetBrains.Annotations;
using LivinOnSweets.API.Stores;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Animations;
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
        [CanBeNull] private readonly string spritesheet;

        protected Texture IdleTexture;
        protected Texture ActiveTexture;

        public SpritesheetButton(string spritesheet = null)
        {
            this.spritesheet = spritesheet;
        }

        [BackgroundDependencyLoader]
        private void load(AnimatedPixelArtTextureStore animPixStore)
        {
            if (spritesheet == null)
                return;

            Texture texture = animPixStore.Get(spritesheet);
            if (texture == null)
                throw new NullReferenceException();

            texture.ScaleAdjust = 1;

            // Only supports 2 frames and the spritesheet is most likely to be spread horizontally
            List<FrameData<Texture>> frames = animPixStore.GetFrames(spritesheet, 0, 1, 2);
            SetFrames([frames[0].Content, frames[1].Content]);
        }

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
