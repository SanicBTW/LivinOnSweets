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
    public partial class SlideContainer : OContainer
    {
        protected override OContainer Content => NewContent;
        protected OContainer NewContent;
        protected OContainer RoundedMask;

        protected Nudge PanelNudge;

        protected MarginPadding BasePadding;

        public double SlideDuration = 500D;
        public bool LeftSide = true;

        public SlideContainer()
        {
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
        }

        protected override bool OnHover(HoverEvent e)
        {
            this.MoveToX(0, SlideDuration, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            float halfPad = (LeftSide ? BasePadding.Right : BasePadding.Left) / 2;
            this.MoveToX(
                LeftSide ? -NewContent.DrawWidth - halfPad : NewContent.DrawWidth + halfPad
                , SlideDuration, Easing.OutQuint);
        }

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
