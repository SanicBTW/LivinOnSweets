using LivinOnSweets.API.Sprites;
using LivinOnSweets.API.Stores;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace LivinOnSweets.API.Containers
{
    public partial class SweetScrollContainer : SweetScrollContainer<Drawable>
    {
        public SweetScrollContainer(Direction scrollDirection = Direction.Vertical, bool startBlocked = false)
            : base(scrollDirection, startBlocked)
        {
        }
    }

    public partial class SweetScrollContainer<T> : ScrollContainer<T>
        where T : Drawable
    {
        [Resolved]
        private AccentStore accentStore { get; set; }

        public BindableColour4 ScrollBarColour = new(Colour4.Black);
        public BindableFloat ScrollBarAlpha = new(1f);
        public BindableFloat ScrollBarMaxAlpha = new(1f);
        public double AlphaDuration = 200;

        public bool ScrollBlocked { get; protected set; }

        public SweetScrollContainer(Direction scrollDirection = Direction.Vertical, bool startBlocked = false)
            : base(scrollDirection)
        {
            ScrollbarOverlapsContent = true;
            ScrollBarAlpha.BindValueChanged((ev) => Scrollbar.Alpha = ev.NewValue);
            if (startBlocked)
                BlockScroll();
        }

        public void BlockScroll() => ScrollBlocked = true;

        public void AllowScroll() => ScrollBlocked = false;

        public void ApplyAccent(StudentBanner banner)
        {
            // Please check the student banner accent tests and the accent store comments, to avoid blocking we have to do it this way
            Task.Run(() =>
            {
                Colour4 accentColour = accentStore.GetDominantColor(banner.ImageName);

                // Schedule the sprite mutation operation to the update thread of the framework, since in this context, we are running in a foreign thread
                Schedule(() =>
                {
                    this.TransformBindableTo(ScrollBarColour, accentColour, 1200D, Easing.OutQuint);
                });
            });
        }

        protected override void OnUserScroll(float value, bool animated = true, double? distanceDecay = null)
        {
            if (ScrollBlocked)
                return;

            base.OnUserScroll(value, animated, distanceDecay);
        }

        protected override ScrollbarContainer CreateScrollbar(Direction direction) => new SweetScrollbar(direction);

        protected partial class SweetScrollbar : ScrollbarContainer
        {
            protected new SweetScrollContainer Parent => (SweetScrollContainer)base.Parent;

            private Box bar;

            private const float dim_size = 8;

            // used to fade the scroll bar after being inactive for too long
            private const double max_idle_time = 1000;

            private double lastScrollTime;
            private double lastScrollPos; // we track the Y position, if it has changed then we scrolling, if not its idle

            private float defaultBarAlpha = 1f;
            private bool transitioning;

            public SweetScrollbar(Direction direction)
                : base(direction)
            {

                Child = new Container()
                {
                    Masking = true,
                    CornerRadius = 4,
                    RelativeSizeAxes = Axes.Both,
                    Child = bar = new()
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Parent.ScrollBarColour.BindValueChanged((ev) => bar.Colour = ev.NewValue, true);

                Parent.ScrollBarMaxAlpha.BindValueChanged((ev) => defaultBarAlpha = ev.NewValue, true);
                Child.Alpha = defaultBarAlpha;
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                bool hasChanged = !Parent.ScrollBlocked && lastScrollPos != Y;

                switch (hasChanged)
                {
                    case true:
                    {
                        if (Child.Alpha <= defaultBarAlpha && !transitioning)
                        {
                            Child.FadeTo(defaultBarAlpha, Parent.AlphaDuration);
                            transitioning = true;
                        }

                        lastScrollTime = 0;
                        break;
                    }

                    case false when lastScrollTime <= max_idle_time:
                        lastScrollTime += Clock.ElapsedFrameTime;
                        break;

                    case false:
                        if (Child.Alpha >= defaultBarAlpha && transitioning)
                        {
                            Child.FadeOut(Parent.AlphaDuration);
                            transitioning = false;
                        }
                        lastScrollTime = max_idle_time;
                        break;
                }

                lastScrollPos = Y;
            }

            public override void ResizeTo(float val, int duration = 0, Easing easing = Easing.None)
            {
                Vector2 size = new Vector2(dim_size)
                {
                    [(int)ScrollDirection] = val
                };
                this.ResizeTo(size, duration, easing);
            }
        }
    }
}
