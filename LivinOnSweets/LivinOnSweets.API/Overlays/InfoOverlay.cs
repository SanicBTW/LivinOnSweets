// https://github.com/ppy/osu-framework/blob/master/osu.Framework/Graphics/Visualisation/InfoOverlay.cs

using LivinOnSweets.API.Sprites.Editor;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osuTK;

namespace LivinOnSweets.API.Overlays
{
    internal partial class InfoOverlay : Container<FlashyBox>
    {
        private Drawable target;

        public Drawable Target
        {
            get => target;
            set
            {
                if (target == value) return;

                target = value;

                foreach (FlashyBox b in Children)
                    b.Target = target;

                Alpha = target != null ? 1.0f : 0.0f;

                Pulse();
            }
        }

        private static Quad quadAroundPosition(Vector2 pos, float sideLength)
        {
            Vector2 size = new Vector2(sideLength);
            return new Quad(pos.X - size.X / 2, pos.Y - size.Y / 2, size.X, size.Y);
        }

        private readonly FlashyBox layout;
        private readonly FlashyBox shape;
        private readonly FlashyBox childShape;

        public InfoOverlay()
        {
            RelativeSizeAxes = Axes.Both;

            Children =
            [
                layout = new FlashyBox((d) => d.ToScreenSpace(d.LayoutRectangle))
                {
                    Colour = Colour4.Green,
                    Alpha = 0.5f
                },
                shape = new FlashyBox((d) => d.ScreenSpaceDrawQuad)
                {
                    Colour = Colour4.Blue,
                    Alpha = 0.5f
                },
                childShape = new FlashyBox(delegate(Drawable d)
                {
                    if (!(d is CompositeDrawable c))
                        return d.ScreenSpaceDrawQuad;

                    RectangleF rect = new RectangleF(c.ChildOffset, c.ChildSize);
                    return d.ToScreenSpace(rect);
                })
                {
                    Colour = Colour4.Red,
                    Alpha = 0.5f
                },
                // TODO: Round these boxes
                new FlashyBox(d => quadAroundPosition(d.ToScreenSpace(d.OriginPosition), d.Origin == d.Anchor ? 10 : 15)) { Colour = Colour4.Blue, Alpha = 0.5f },

                new FlashyBox(d => quadAroundPosition(d.ToScreenSpace(d.AnchorPosition), 15)) { Colour = Colour4.Red, Alpha = 0.5f },
            ];
        }

        public void Pulse()
        {
            layout.FlashColour(Colour4.White, 250);
            shape.FlashColour(Colour4.White, 250);
            childShape.FlashColour(Colour4.White, 250);
        }

        protected override void Update()
        {
            base.Update();

            foreach (FlashyBox b in Children)
                b.Invalidate(Invalidation.DrawNode);
        }
    }
}
