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

namespace LivinOnSweets.API.Sprites.Editor
{
    // I dont really like the level of nesting im reaching but uhh you win some you lose some i guess
    // TODO: When wanting to trigger Drag Event for the scroll container, this click can block the initialization of it and thus opening the bound container
    // TODO: Add a loading spinner (for the DebugContainer only) to show if theres an asynchronous task in the background
    public partial class EditorEntry
    {
        public abstract partial class EntryPreview : Container, IAccentColorReceiver
        {
            [Resolved]
            private Container<SlideContainer> editorSliders { get; set; }

            [Resolved]
            private SlideContainer parentSlider { get; set; }

            [Resolved]
            private AccentComponent accentComponent { get; set; }

            protected BindableColour4 PrimaryColor = new();
            protected BindableColour4 SecondaryColor = new();

            public double ColorFadeDuration = 500D;

            protected override Container Content => PreviewContent;
            protected readonly Container PreviewContent;

            private Box background;

            public EntryPreview()
            {
                Anchor = Anchor.BottomCentre;
                Origin = Anchor.BottomCentre;
                RelativeSizeAxes = Axes.X;
                Height = 92;
                Padding = new MarginPadding(8);

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
                        PreviewContent = new Container()
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    }
                };

                SecondaryColor.BindValueChanged((ev) =>
                {
                    background.Colour = ev.NewValue;
                });
            }

            protected override bool OnHover(HoverEvent e)
            {
                background.FadeColour(SecondaryColor.Value.Darken(0.25f), ColorFadeDuration, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                background.FadeColour(SecondaryColor.Value, ColorFadeDuration, Easing.OutQuint);
                base.OnHoverLost(e);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                // just in case, with the new addition, read SlideContainer on mouse down
                if (!parentSlider.SlideBlock.Value)
                    parentSlider.SlideBlock.Value = true;

                parentSlider.ClickOutClosesContainer = false;
                LoadComponentAsync(CreateSlideContainer(), editorSliders.Add);

                return true;
            }

            protected virtual SlideContainer CreateSlideContainer()
            {
                EditorSlider newContainer = new EditorSlider(parentSlider.LeftSide, parentSlider);
                newContainer.ScrollContent.Child = CreateSlideContent();
                return newContainer;
            }

            protected abstract Container CreateSlideContent();

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

        internal partial class EntryPreviewPlaceholder : EntryPreview
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                SpriteText text;
                PreviewContent.Add(text = new SpriteText()
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Font = new FontUsage(family: "DNFBitBit", size: 32F),
                    Text = "placeholder",
                    Margin = new MarginPadding() { Left = Padding.Left / 2 },
                    Colour = Colour4.Black // Opposite as the background, to not need to wait for the accents to apply
                });

                PrimaryColor.BindValueChanged((ev) =>
                {
                    text.Colour = ev.NewValue;
                });
            }

            protected override Container CreateSlideContent()
            {
                return new Container()
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Child = new SpriteText()
                    {
                        Margin = new MarginPadding(16),
                        Text = "override entrypreview to customize this",
                        Font = new FontUsage(family: "DNFBitBit", size: 24F),
                        Colour = SecondaryColor.Value
                    }
                };
            }
        }
    }
}
