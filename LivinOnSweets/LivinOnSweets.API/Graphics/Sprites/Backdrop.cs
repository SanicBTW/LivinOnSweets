using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osuTK;
// ReSharper disable MemberCanBePrivate.Global

namespace LivinOnSweets.API.Graphics.Sprites
{
    public partial class Backdrop : Sprite
    {
        // base mult is 10 since it could look really slow
        private const float speed_mult = 10;

        public bool Running { get; private set; }

        // A bindable to be able to transform it
        // A negative speed will go the opposite direction
        // +x => left to right / -x => right to left | +y => up to down / -y => down to up
        public readonly Bindable<Vector2> ScrollSpeed = new();

        private Vector2 scrollOffset = Vector2.Zero;
        private readonly bool startImmediately;

        public Backdrop(float speed = 1F, bool startOnLoad = false)
        {
            ScrollSpeed.Value = new Vector2(speed);
            startImmediately = startOnLoad;
        }

        public void Start()
        {
            if (Running)
                return;

            Running = true;
        }

        public void Freeze()
        {
            if (!Running)
                return;

            Running = false;
        }

        protected override void Update()
        {
            base.Update();

            if (!Running)
                return;

            float speedX = speed_mult * ScrollSpeed.Value.X;
            float speedY = speed_mult * ScrollSpeed.Value.Y;

            float dt = (float)Time.Elapsed / 1000f;
            scrollOffset += new Vector2(speedX * dt, speedY * dt);
            scrollOffset.X %= TextureRectangle.Width;
            scrollOffset.Y %= TextureRectangle.Height;

            TextureRectangle = new RectangleF(scrollOffset.X, scrollOffset.Y, TextureRectangle.Width, TextureRectangle.Height);
        }

        protected override void LoadComplete()
        {
            TextureRelativeSizeAxes = Axes.None;
            RelativeSizeAxes = Axes.Both;
            Size = Vector2.One;

            if (startImmediately)
                Start();
        }
    }
}
