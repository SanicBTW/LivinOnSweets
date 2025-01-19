using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Containers.Editor;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;

namespace LivinOnSweets.API.Sprites.Editor
{
    internal partial class EditorSlider
    {
        internal partial class EditorSliderHeader : Container
        {
            public double ColorFadeDuration = 500D;

            protected Box Background;

            protected EditorSlider ParentSlider;
            protected EditorSideBar Controller;

            public EditorSliderHeader(EditorSlider parentSlider, EditorSideBar controller)
            {
                ParentSlider = parentSlider;
                Controller = controller;

                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
                RelativeSizeAxes = Axes.X;
                Height = 52;
                Padding = new MarginPadding(8);

                SpriteText text;
                SpriteIcon icon;
                InternalChild = new Container()
                {
                    Masking = true,
                    CornerRadius = 6f,
                    RelativeSizeAxes = Axes.Both,
                    Children =
                    [
                        Background = new Box()
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        // should i make it pixelated?
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
                    Background.Colour = ev.NewValue;
                }, true);

                Controller.PrimaryColor.BindValueChanged((ev) =>
                {
                    icon.Colour = ev.NewValue;
                    text.Colour = ev.NewValue;
                }, true);
            }

            protected override bool OnHover(HoverEvent e)
            {
                Background.FadeColour(Controller.SecondaryColor.Value.Darken(0.15f), ColorFadeDuration, Easing.OutQuint);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                Background.FadeColour(Controller.SecondaryColor.Value, ColorFadeDuration, Easing.OutQuint);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                ParentSlider.Closing = true;
                Container<SlideContainer> parent = (Container<SlideContainer>)ParentSlider.Parent;
                ParentSlider.MoveToX(ParentSlider.OutOfBoundsPosition, ParentSlider.SlideDuration, Easing.OutQuint)
                    .FadeOut(500D, Easing.OutQuint)
                    .OnComplete(
                    (_) =>
                    {
                        // have to manually call dispose after removing it from the parent container
                        parent!.Remove(ParentSlider, false);
                        ParentSlider.Dispose();
                    });

                return true;
            }
        }
    }
}
