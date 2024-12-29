using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using OContainer = osu.Framework.Graphics.Containers.Container;

namespace LivinOnSweets.API.StartupObjects
{
    public partial class ClosePopup : OverlayContainer
    {
        private OContainer content;

        public double FadeInDuration = 1300D;
        public double SlideYDuration = 1200D;

        public ClosePopup()
        {
            Depth = -99; // Show up in front of everything
            AddRangeInternal(new Drawable[]
            {
                new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black,
                    Alpha = 0.5f
                },
                content = new OContainer()
                {
                    Masking = true,
                    CornerRadius = 15,
                    Size = new Vector2(452, 240),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box()
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = Colour4.MidnightBlue,
                        },
                        new OContainer()
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Padding = new MarginPadding(16),
                            Children = new Drawable[]
                            {
                                new SpriteText()
                                {
                                    Text = "Are you sure you want to quit the game?"
                                }
                            }
                        }
                    },
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            content.Y = -DrawHeight;
        }

        protected override void PopIn()
        {
            content.MoveToY(0, SlideYDuration, Easing.OutQuint);
            this.FadeIn(FadeInDuration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            content.MoveToY(-DrawHeight, SlideYDuration, Easing.OutQuint);
            this.FadeOut(FadeInDuration, Easing.OutQuint);
        }
    }
}
