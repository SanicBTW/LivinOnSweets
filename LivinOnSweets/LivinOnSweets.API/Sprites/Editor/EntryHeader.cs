using LivinOnSweets.API.Components;
using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Extensions;
using LivinOnSweets.API.Interfaces;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;

namespace LivinOnSweets.API.Sprites.Editor
{
    public partial class EditorEntry
    {
        public partial class EntryHeader : Container, IAccentColorReceiver
        {
            [Resolved]
            private AccentComponent accentComponent { get; set; }

            protected BindableColour4 PrimaryColor = new();
            protected BindableColour4 SecondaryColor = new();

            public EntryHeader(string category)
            {
                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
                RelativeSizeAxes = Axes.X;
                Height = 52;
                Padding = new MarginPadding(8);

                Box background;
                SpriteText text;
                InternalChild = new Container()
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

                PrimaryColor.BindValueChanged((ev) =>
                {
                    text.Colour = ev.NewValue;
                });

                SecondaryColor.BindValueChanged((ev) =>
                {
                    background.Colour = ev.NewValue;
                });
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
