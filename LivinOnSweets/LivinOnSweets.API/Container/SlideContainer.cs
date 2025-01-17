using LivinOnSweets.API.Interfaces;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;
using OContainer = osu.Framework.Graphics.Containers.Container;

namespace LivinOnSweets.API.Container
{
    // Basic container that slides when hovered
    // TODO: Increase the hover region
    public partial class SlideContainer : OContainer, ISlideContainerCloseBlock
    {
        protected MarginPadding BasePadding;

        protected override OContainer Content => NewContent;
        protected OContainer NewContent;
        protected OContainer RoundedMask;

        public Nudge PanelNudge { get; protected set; }

        public BindableBool SlideBlock = new();

        public float OutOfBoundsPosition { get; protected set; }
        public double SlideDuration = 500D;
        public readonly bool LeftSide;

        public bool ClickOutClosesContainer { get; set; } = true;

        public SlideContainer(bool leftSide)
        {
            LeftSide = leftSide;

            NewContent = new OContainer()
            {
                Name = "Slide Content",
                RelativeSizeAxes = Axes.Y
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // Defer the creation to the load method to be able to properly apply the set variables
            BasePadding = new MarginPadding(12);
            if (LeftSide)
                BasePadding.Right = 24;
            else
                BasePadding.Left = 24;

            AddRangeInternal(new Drawable[]
            {
                new OContainer()
                {
                    Anchor = LeftSide ? Anchor.CentreRight : Anchor.CentreLeft,
                    Origin = LeftSide ? Anchor.CentreRight : Anchor.CentreLeft,

                    Name = "Slide Padding",
                    RelativeSizeAxes = Axes.Both,
                    Padding = BasePadding,
                    Child = RoundedMask = new OContainer()
                    {
                        Name = "Slide Rounded Mask",
                        Masking = true,
                        RelativeSizeAxes = Axes.Both,
                        CornerRadius = 6f,
                        Child = NewContent
                    },
                },
                PanelNudge = new Nudge(LeftSide)
                {
                    Size = new Vector2(12, 32)
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            NewContent.Width = RoundedMask.DrawWidth;
            OutOfBoundsPosition = OutOfBoundsPos();
            X = OutOfBoundsPos();
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (IsHidden())
                this.MoveToX(0, SlideDuration, Easing.OutQuint);

            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (SlideBlock.Value)
                return;

            this.MoveToX(OutOfBoundsPosition, SlideDuration, Easing.OutQuint);
        }

        // WARNING, this could lead to potential unwanted behaviour BUT this is to avoid making the container hide when editor entry preview is clicked
        // and blocks the slide out, when clicking on the whole slide container it can pass the event down to editor container making the container slide out
        // which is essentially unwanted behaviour, so this fixes that thing
        protected override bool OnMouseDown(MouseDownEvent e)
        {
            // a few minutes later: i decided to block the slide HERE rather than having to toggle it manually on the children
            // its not really good tbh and having to propagate a bindable isnt my favourite thing either
            SlideBlock.Value = true;

            return true;
        }

        protected virtual float OutOfBoundsPos()
        {
            float halfPad = (LeftSide ? BasePadding.Right : BasePadding.Left) / 2;
            return LeftSide ? -NewContent.DrawWidth - halfPad : NewContent.DrawWidth + halfPad;
        }

        public bool IsHidden() => LeftSide ? X <= 0 : X >= 0;

        public bool IsVisible() => LeftSide ? X >= OutOfBoundsPosition : X <= OutOfBoundsPosition;

        public partial class Nudge : CompositeDrawable
        {
            public BindableColour4 NudgeColor = new(Colour4.Black);
            public BindableFloat NudgeAlpha = new(1);

            public Nudge(bool isLeft)
            {
                Anchor = isLeft ? Anchor.CentreRight : Anchor.CentreLeft;
                Origin = isLeft ? Anchor.CentreRight : Anchor.CentreLeft;

                Box sprite;
                InternalChild = new OContainer()
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 6f,
                    Child = sprite = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                };

                NudgeColor.BindValueChanged((ev) => sprite.Colour = ev.NewValue, true);
                NudgeAlpha.BindValueChanged((ev) => sprite.Alpha = ev.NewValue, true);
            }
        }
    }
}
