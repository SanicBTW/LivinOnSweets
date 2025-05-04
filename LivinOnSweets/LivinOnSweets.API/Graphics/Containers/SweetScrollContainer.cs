using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osuTK;
using osuTK.Input;

namespace LivinOnSweets.API.Graphics.Containers
{
    public partial class SweetScrollContainer : SweetScrollContainer<Drawable>
    {
        public SweetScrollContainer() { }
        public SweetScrollContainer(Direction direction, bool startBlocked) : base(direction, startBlocked) { }
    }

    // Mix of OsuScrollContainer and the old SweetScrollContainer
    public partial class SweetScrollContainer<T> : ScrollContainer<T>
        where T : Drawable
    {
        public const float SCROLL_BAR_WIDTH = 10;
        public const float SCROLL_BAR_PADDING = 3;

        public BindableColour4 ScrollBarColour = new(Colour4.Black);
        public BindableFloat ScrollBarAlpha = new(1f);
        public BindableFloat ScrollBarMaxAlpha = new(1f);
        public double AlphaDuration = 200;

        public BindableBool ScrollBlocked { get; protected set; } = new();

        public SweetScrollContainer(Direction scrollDirection = Direction.Vertical, bool startBlocked = false)
            : base(scrollDirection)
        {
            ScrollBarAlpha.BindValueChanged((ev) => Scrollbar.Alpha = ev.NewValue);
            if (startBlocked)
                ScrollBlocked.Value = true;
        }

        protected override void OnUserScroll(double value, bool animated = true, double? distanceDecay = null)
        {
            if (ScrollBlocked.Value)
                return;

            base.OnUserScroll(value, animated, distanceDecay);
        }

        /// <summary>
        /// Scrolls a <see cref="Drawable"/> into view.
        /// </summary>
        /// <param name="d">The <see cref="Drawable"/> to scroll into view.</param>
        /// <param name="animated">Whether to animate the movement.</param>
        /// <param name="extraScroll">An added amount to scroll beyond the requirement to bring the target into view.</param>
        public void ScrollIntoView(Drawable d, bool animated = true, float extraScroll = 0)
        {
            double childPos0 = GetChildPosInContent(d);
            double childPos1 = GetChildPosInContent(d, d.DrawSize);

            double minPos = Math.Min(childPos0, childPos1);
            double maxPos = Math.Max(childPos0, childPos1);

            if (minPos < Current || (minPos > Current && d.DrawSize[ScrollDim] > DisplayableContent))
                ScrollTo(minPos - extraScroll, animated);
            else if (maxPos > Current + DisplayableContent)
                ScrollTo(maxPos - DisplayableContent + extraScroll, animated);
        }

        #region Absolute scrolling

        /// <summary>
        /// Controls the rate with which the target position is approached when performing a relative drag. Default is 0.02.
        /// </summary>
        public double DistanceDecayOnAbsoluteScroll = 0.02;

        protected virtual void ScrollToAbsolutePosition(Vector2 screenSpacePosition)
        {
            float fromScrollbarPosition = FromScrollbarPosition(ToLocalSpace(screenSpacePosition)[ScrollDim]);
            float scrollbarCentreOffset = FromScrollbarPosition(Scrollbar.DrawHeight) * 0.5f;

            ScrollTo(Clamp(fromScrollbarPosition - scrollbarCentreOffset), true, DistanceDecayOnAbsoluteScroll);
        }

        #endregion

        protected override ScrollbarContainer CreateScrollbar(Direction direction) => new SweetScrollbar(direction);

        protected partial class SweetScrollbar : ScrollbarContainer
        {
            protected new SweetScrollContainer Parent => (SweetScrollContainer)base.Parent;

            protected override float MinimumDimSize => SCROLL_BAR_WIDTH * 3;

            // used to fade the scroll bar after being inactive for too long
            private const double max_idle_time = 1000;

            private Colour4 hoverColour = Colour4.Gray;
            private Colour4 defaultColour = Colour4.Black;
            private Colour4 highlightColour = Colour4.Green;

            private double lastScrollTime;
            private double lastScrollPos; // we track the Y position, if it has changed then we scrolling, if not its idle

            private float defaultBarAlpha = 1f;
            private bool transitioning;

            private readonly Box box;

            public SweetScrollbar(Direction scrollDir)
                : base(scrollDir)
            {
                Blending = BlendingParameters.Additive;
                CornerRadius = 5;
                Size = new Vector2(SCROLL_BAR_WIDTH);

                const float margin = 3;

                Margin = new MarginPadding
                {
                    Left = scrollDir == Direction.Vertical ? margin : 0,
                    Right = scrollDir == Direction.Vertical ? margin : 0,
                    Top = scrollDir == Direction.Horizontal ? margin : 0,
                    Bottom = scrollDir == Direction.Horizontal ? margin : 0,
                };

                Masking = true;
                Child = box = new Box { RelativeSizeAxes = Axes.Both };
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                bool hasChanged = !Parent.ScrollBlocked.Value && !Precision.AlmostEquals(lastScrollPos, Y);

                switch (hasChanged)
                {
                    case true:
                    {
                        if (box.Alpha <= defaultBarAlpha && !transitioning)
                        {
                            box.FadeTo(defaultBarAlpha, Parent.AlphaDuration);
                            transitioning = true;
                        }

                        lastScrollTime = 0;
                        break;
                    }

                    case false when lastScrollTime <= max_idle_time:
                        lastScrollTime += Clock.ElapsedFrameTime;
                        break;

                    case false:
                        if (box.Alpha >= defaultBarAlpha && transitioning)
                        {
                            box.FadeOut(Parent.AlphaDuration);
                            transitioning = false;
                        }

                        lastScrollTime = max_idle_time;
                        break;
                }

                lastScrollPos = Y;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Parent.ScrollBarColour.BindValueChanged((ev) => box.Colour = ev.NewValue, true);

                Parent.ScrollBarMaxAlpha.BindValueChanged((ev) => defaultBarAlpha = ev.NewValue, true);
                Child.Alpha = defaultBarAlpha;
            }

            public override void ResizeTo(float val, int duration = 0, Easing easing = Easing.None)
            {
                this.ResizeTo(new Vector2(SCROLL_BAR_WIDTH)
                {
                    [(int)ScrollDirection] = val
                }, duration, easing);
            }

            protected override bool OnHover(HoverEvent e)
            {
                this.FadeColour(hoverColour, 100);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                this.FadeColour(defaultColour, 100);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if (!base.OnMouseDown(e)) return false;

                // note that we are changing the colour of the box here as to not interfere with the hover effect.
                box.FadeColour(highlightColour, 100);
                return true;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                if (e.Button != MouseButton.Left) return;

                box.FadeColour(Colour4.White, 100);

                base.OnMouseUp(e);
            }
        }
    }
}
