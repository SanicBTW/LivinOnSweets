using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Containers.Editor;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;

namespace LivinOnSweets.API.Sprites.Editor
{
    // I dont really like the level of nesting im reaching but uhh you win some you lose some i guess
    public partial class EditorEntry
    {
        public abstract partial class EntryPreview : Container
        {
            [Resolved]
            private Container<SlideContainer> editorSliders { get; set; }

            [Resolved]
            private SlideContainer parentSlider { get; set; }

            public double ColorFadeDuration = 500D;

            protected EditorSideBar Controller;

            protected override Container Content => PreviewContent;
            protected readonly Container PreviewContent;

            protected SlideContainer EntrySlider;

            private Box background;

            public EntryPreview(EditorSideBar controller)
            {
                Controller = controller;

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

                controller.SecondaryColor.BindValueChanged((ev) =>
                {
                    background.Colour = ev.NewValue;
                });
            }

            protected override bool OnHover(HoverEvent e)
            {
                background.FadeColour(Controller.SecondaryColor.Value.Darken(0.25f), ColorFadeDuration, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                background.FadeColour(Controller.SecondaryColor.Value, ColorFadeDuration, Easing.OutQuint);
                base.OnHoverLost(e);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                // just in case, with the new addition, read SlideContainer on mouse down
                if (!parentSlider.SlideBlock.Value)
                    parentSlider.SlideBlock.Value = true;

                parentSlider.ClickOutClosesContainer = false;
                editorSliders.Add(EntrySlider = CreateSlideContainer());

                return true;
            }

            protected virtual SlideContainer CreateSlideContainer()
            {
                EditorSlider newContainer = new EditorSlider(parentSlider.LeftSide, parentSlider, Controller);
                newContainer.ScrollContent.Child = CreateSlideContent();
                return newContainer;
            }

            protected abstract Container CreateSlideContent();
        }

        internal partial class EntryPreviewPlaceholder(EditorSideBar controller) : EntryPreview(controller)
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

                Controller.PrimaryColor.BindValueChanged((ev) =>
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
                        Colour = Controller.SecondaryColor.Value
                    }
                };
            }
        }
    }
}
