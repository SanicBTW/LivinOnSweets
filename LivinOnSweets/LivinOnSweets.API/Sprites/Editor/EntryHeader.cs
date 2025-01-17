using LivinOnSweets.API.Container;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using OContainer = osu.Framework.Graphics.Containers.Container;

namespace LivinOnSweets.API.Sprites.Editor
{
    public partial class EditorEntry
    {
        public partial class EntryHeader : OContainer
        {
            public EntryHeader(string category, EditorSideBar controller)
            {
                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
                RelativeSizeAxes = Axes.X;
                Height = 52;
                Padding = new MarginPadding(8);

                Box background;
                SpriteText text;
                InternalChild = new OContainer()
                {
                    Masking = true,
                    CornerRadius = 6f,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        background = new Box()
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.Gray,
                        },
                        text = new SpriteText()
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = new FontUsage(family: "DNFBitBit", size: 32F),
                            Text = category,
                            Margin = new MarginPadding() { Left = Padding.Left / 2 },
                            Colour = Colour4
                                .Black // Opposite as the background, to not need to wait for the accents to apply
                        }
                    }
                };

                controller.PrimaryColor.BindValueChanged((ev) =>
                {
                    text.Colour = ev.NewValue;
                });

                controller.SecondaryColor.BindValueChanged((ev) =>
                {
                    background.Colour = ev.NewValue;
                });
            }
        }
    }
}
