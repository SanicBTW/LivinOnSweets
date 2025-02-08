using LivinOnSweets.API.Components;
using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Extensions;
using LivinOnSweets.API.Interfaces;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
        internal partial class EditorSliderHeader : Container, IAccentColorReceiver
        {
            [Resolved]
            private AccentComponent accentComponent { get; set; }

            protected BindableColour4 PrimaryColor = new();
            protected BindableColour4 SecondaryColor = new();

            protected EditorSlider ParentSlider;

            protected Box Background;

            public double ColorFadeDuration = 500D;

            public EditorSliderHeader(EditorSlider parentSlider)
            {
                ParentSlider = parentSlider;

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

                SecondaryColor.BindValueChanged((ev) =>
                {
                    Background.Colour = ev.NewValue;
                });

                PrimaryColor.BindValueChanged((ev) =>
                {
                    icon.Colour = ev.NewValue;
                    text.Colour = ev.NewValue;
                });
            }

            protected override bool OnHover(HoverEvent e)
            {
                Background.FadeColour(SecondaryColor.Value.Darken(0.15f), ColorFadeDuration, Easing.OutQuint);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                Background.FadeColour(SecondaryColor.Value, ColorFadeDuration, Easing.OutQuint);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                // i forgot to set this lmao
                ParentSlider.ParentSlider.ClickOutClosesContainer = true;

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

            AccentBannerSide IAccentColorReceiver.AccentSide => AccentBannerSide.Left;

            void IAccentColorReceiver.PropagateAccents(BindableColour4[] colors)
            {
                PrimaryColor.BindTo(colors[1]);
                SecondaryColor.BindTo(colors[2]);
            }

            void IAccentColorReceiver.AccentsUpdated(double duration, Easing easing)
            {
                BindableColour4 newPrimary = accentComponent.GetAccent(this, AccentColorRole.Secondary);
                BindableColour4 newSecondary = accentComponent.GetAccent(this, AccentColorRole.Tertiary);

                this.TransformBindableTo(PrimaryColor, newPrimary.Value, duration, easing);
                this.TransformBindableTo(SecondaryColor, newSecondary.Value, duration, easing);
            }
        }
    }
}
