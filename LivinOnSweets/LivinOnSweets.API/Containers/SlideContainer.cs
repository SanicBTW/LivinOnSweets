using LivinOnSweets.API.Sprites;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Threading;
using osuTK;

namespace LivinOnSweets.API.Containers
{
    // Kind of a port or code deobfuscation of the Slide object from the OG js file
    public partial class SlideContainer : Container<SlideElement>
    {
        public const double SCROLL_DURATION = 700D;
        public const Easing SCROLL_EASE = Easing.InOutCubic;
        public const float SCROLL_FACTOR = 1.2F;

        protected override Container<SlideElement> Content => Elements;

        protected Container<SlideElement> Elements;

        private ScheduledDelegate hideDelegate;

        public SlideContainer()
        {
            Anchor = Origin = Anchor.Centre;

            // This is to be able to use add internal since it doesnt redirect calls to Content which only accepts SlideElement
            AddInternal(Elements = new Container<SlideElement>()
            {
                RelativeSizeAxes = Axes.Both,
            });
        }

        public SlideElement AddElement(Texture texture, float startX, int direction, int depth, double delay = 0, BlendingParameters? blending = null)
        {
            SlideElement element = new SlideElement(startX, direction, delay)
            {
                Texture = texture,

                // since the depth on fucking phaser seems to act like a zindex rather than a real depth
                // we set the depth to its negative value, because we chill like that
                // (i actually dont know how the phaser depth works, but it doesnt seem to act like the depth from here,
                // the reason why we use the depth as negative is because it looks better and accurate)
                Depth = -depth,
            };

            if (blending != null)
                element.Blending = blending.Value;

            Add(element);

            return element;
        }

        public override void Show()
        {
            Alpha = 1;
            Elements.Alpha = 1;
        }

        public override void Hide()
        {
            Elements.Alpha = 0;
            Alpha = 0;
        }

        public void Scroll(float targetX)
        {
            this.MoveToX(targetX, SCROLL_DURATION, SCROLL_EASE);
        }

        public void FadeIn(float factor)
        {
            // Cancel the delayed hide call if we switched screens too fast
            hideDelegate?.Cancel();

            Position = new Vector2(-DrawWidth * factor, Position.Y);
            Show();
            Scroll(0);
            fadeInElements(factor);
        }

        public void FadeOut(float factor)
        {
            Scroll(DrawWidth * factor);
            fadeOutElements(factor);
            hideDelegate = Scheduler.AddDelayed(Hide, SCROLL_DURATION + 50);
        }

        // my dumb ahh was using the outX or "targetX" to move the sprite INSIDE the view
        // turns out it was STARTING from that position not MOVING to that position
        private void fadeInElements(float factor)
        {
            foreach (SlideElement element in Elements)
            {
                int direction = element.Direction != 0 ? element.Direction : 1;
                float outX = factor * -DrawWidth * direction;
                element.X = outX;
                element.Delay(element.Delay).MoveToX(element.StartingX, SCROLL_DURATION, SCROLL_EASE);
            }
        }

        // yeah here we want to scroll out of the view fr
        private void fadeOutElements(float factor)
        {
            foreach (SlideElement element in Elements)
            {
                int direction = -(element.Direction != 0 ? element.Direction : 1);
                float outX = factor * -DrawWidth * direction;
                element.Delay(element.Delay).MoveToX(outX, SCROLL_DURATION, SCROLL_EASE);
            }
        }
    }
}
