using LivinOnSweets.API.Container;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using OContainer = osu.Framework.Graphics.Containers.Container;

namespace LivinOnSweets.API.Sprites.Editor
{
    internal partial class EditorSlider
    {
        internal partial class EditorSliderHeader : OContainer
        {
            protected EditorSideBar Controller;

            public EditorSliderHeader(EditorSideBar controller)
            {
                Controller = controller;

                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
                RelativeSizeAxes = Axes.X;
                Height = 52;
                Padding = new MarginPadding(8);

                Box background;
                SpriteText text;
                SpriteIcon icon;
                InternalChild = new OContainer()
                {
                    Masking = true,
                    CornerRadius = 6f,
                    RelativeSizeAxes = Axes.Both,
                    Children =
                    [
                        background = new Box()
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        icon = new SpriteIcon()
                        {
                            Icon = FontAwesome.Solid.ArrowLeft,
                            Size = new Vector2(24),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Margin = new MarginPadding() { Left = 4 }
                        },
                        text = new SpriteText()
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = new FontUsage(family: "DNFBitBit", size: 32F),
                            Text = "go back",
                            Margin = new MarginPadding() { Left = icon.Width + 8 }
                        }
                    ]
                };

                Controller.SecondaryColor.BindValueChanged((ev) =>
                {
                    background.Colour = ev.NewValue;
                }, true);

                Controller.PrimaryColor.BindValueChanged((ev) =>
                {
                    icon.Colour = ev.NewValue;
                    text.Colour = ev.NewValue;
                }, true);
            }
        }
    }
}
