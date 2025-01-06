using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transforms;

// Because Container is used as a namespace here, we have to import it thru another way
using OContainer = osu.Framework.Graphics.Containers.Container;

namespace LivinOnSweets.API.Sprites
{
    public partial class SpinningCD : CompositeDrawable
    {
        private OContainer box;

        public bool IsSpinning { get; protected set; } = false;
        public bool ShouldPositionOnLoad = true; // flag indicating if the cd should position itself for a future start animation

        public SpinningCD()
        {
            AutoSizeAxes = Axes.Both;
            Origin = Anchor.Centre;
            Anchor = Anchor.TopLeft;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            InternalChild = box = new()
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Child = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = textures.Get("Startup/UI/CD.png"),
                    Scale = new osuTK.Vector2(1.6f)
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            if (!ShouldPositionOnLoad)
                return;

            Y = -DrawHeight / 2;
            Margin = new MarginPadding()
            {
                Left = DrawWidth + 25
            };
        }

        // Stops the loop function by forcing another transform
        public void CancelSpin()
        {
            box.RotateTo(box.Rotation);
            IsSpinning = false;
        }

        public void Spin(double duration = 7600)
        {
            if (IsSpinning)
                return;

            box.Spin(duration, RotationDirection.Clockwise, box.Rotation);
            IsSpinning = true;
        }

        // Prebuilt animation
        public void Slide(bool transIn = true, double duration = 2400D)
        {
            float dest = transIn ? 0 : -DrawHeight / 2f;
            this.MoveToY(dest, duration, Easing.OutQuint);

            // check if its spinning, or not, either way its probably unnecessary
            if (transIn)
                Spin();
            else
                CancelSpin();
        }
    }
}
