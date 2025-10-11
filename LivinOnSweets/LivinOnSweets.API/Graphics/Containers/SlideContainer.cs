using LivinOnSweets.API.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Threading;
using osuTK;

namespace LivinOnSweets.API.Graphics.Containers
{
    // straight copied from the original lol https://github.com/SanicBTW/LivinOnSweets/blob/master/LivinOnSweets/LivinOnSweets.API/Containers/SlideContainer.cs
    // I know i should probably improve this code to PROPERLY mimic the original version but I lowkey dont know how to and I'm already doing my best here bruh
    // Kind of a port or code deobfuscation of the Slide object from the OG js file
    public partial class SlideContainer : Container<SlideElement>
    {
        private const double scroll_duration = 700D;
        private const Easing scroll_ease = Easing.InOutCubic;
        private const float scroll_factor = 1.2F;

        protected override Container<SlideElement> Content => Elements;
        protected Container<SlideElement> Elements;

        private ScheduledDelegate hideDelegate;

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Origin = Anchor.Centre;

            AddInternal(Elements = new Container<SlideElement>() { RelativeSizeAxes = Axes.Both });
        }

        protected SlideElement AddElement(Texture texture, float startX, int direction, int depth, float startY = 0, double delay = 0, BlendingParameters? blending = null, bool autoAdd = true)
        {
            SlideElement element = new SlideElement(startX, startY, direction, delay)
            {
                Texture = texture,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                // since the depth on fucking phaser seems to act like a zindex rather than a real depth
                // we set the depth to its negative value, because we chill like that
                // (i actually dont know how the phaser depth works, but it doesnt seem to act like the depth from here,
                // the reason why we use the depth as negative is because it looks better and accurate)
                Depth = -depth,
            };

            if (blending != null)
                element.Blending = blending.Value;

            if (autoAdd)
                Add(element);

            return element;
        }

        public override void Show()
            => Alpha = 1;

        public override void Hide()
            => Alpha = 0;

        private void scroll(float targetX) => this.MoveToX(targetX, scroll_duration, scroll_ease);

        public void FadeIn(float factor)
        {
            // Cancel the delayed hide call if we switched screens too fast
            hideDelegate?.Cancel();

            Show();
            Position = new Vector2(-DrawWidth * factor, Position.Y);

            scroll(0);
            FadeInElements(factor);
        }

        public void FadeOut(float factor)
        {
            scroll(DrawWidth * factor);
            FadeOutElements(factor);
            hideDelegate = Scheduler.AddDelayed(Hide, scroll_duration + 50);
        }

        // took a different approach over the functions to help the character parallax background custom use case, i know i should do better
        protected virtual void FadeInElements(float factor) => FadeInElements(factor, Elements);

        protected void FadeInElements(float factor, IEnumerable<SlideElement> elements)
        {
            foreach (SlideElement element in elements)
            {
                int direction = element.Direction != 0 ? element.Direction : 1;
                float outX = element.StartingX + factor * -DrawWidth * scroll_factor * direction;
                element.X = outX;
                element.Delay(element.Delay).MoveToX(element.StartingX, scroll_duration, scroll_ease);
            }
        }

        protected virtual void FadeOutElements(float factor) => FadeOutElements(factor, Elements);

        protected void FadeOutElements(float factor, IEnumerable<SlideElement> elements)
        {
            foreach (SlideElement element in elements)
            {
                int direction = -(element.Direction != 0 ? element.Direction : 1);
                float outX = element.StartingX + factor * -DrawWidth * scroll_factor * direction;
                element.Delay(element.Delay).MoveToX(outX, scroll_duration, scroll_ease);
            }
        }
    }
}
