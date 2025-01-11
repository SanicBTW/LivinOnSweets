using LivinOnSweets.API.Container;
using LivinOnSweets.API.Enum;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using OContainer = osu.Framework.Graphics.Containers.Container;

namespace LivinOnSweets.API.Sprites
{
    // TODO: Fix design having issues with mouse events (they get fired even if its out of bounds, probably due to the padding)
    public partial class EditorEntry : OContainer
    {
        public readonly string Category;
        public readonly EditorEntryContentAnimation ContentAnimation;
        public double ColorFadeDuration = 500D;

        protected OContainer RoundedMask; // The parent container of all the content inside this entry

        private Box background;
        private EditorSideBar controller;

        public EditorEntry(EditorSideBar controller, string category, EditorEntryContentAnimation entryType)
        {
            this.controller = controller;
            Category = category;
            ContentAnimation = entryType;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            // Figma reported this size so I'm doing it :grin:
            Size = new Vector2(400, 150);
            Padding = new MarginPadding(6);

            InternalChild = RoundedMask = new OContainer()
            {
                Masking = true,
                CornerRadius = 6f,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    background = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.Black
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RoundedMask.Add(CreateHeader(Category));
            switch (ContentAnimation)
            {
                case EditorEntryContentAnimation.EXPANDABLE:

                    break;

                case EditorEntryContentAnimation.ANOTHER_VIEW:
                    RoundedMask.Add(CreatePreview());
                    break;
            }

            controller.PrimaryColor.BindValueChanged((ev) =>
            {
                background.Colour = ev.NewValue;
            });
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(controller.PrimaryColor.Value.Darken(0.15f), ColorFadeDuration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeColour(controller.PrimaryColor.Value, ColorFadeDuration, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected virtual EntryHeader CreateHeader(string category) => new(category, controller);

        protected virtual EntryPreview CreatePreview() => new EntryPreviewPlaceholder(controller);

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
                            Margin = new MarginPadding(){ Left = Padding.Left / 2 },
                            Colour = Colour4.Black // Opposite as the background, to not need to wait for the accents to apply
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

        public partial class EntryPreview : OContainer
        {
            [Resolved]
            private BindableBool slideBlock { get; set; }

            public double ColorFadeDuration = 500D;

            protected EditorSideBar Controller;

            protected override Container<Drawable> Content => PreviewContent;
            protected readonly Container<Drawable> PreviewContent;

            private Box background;

            public EntryPreview(EditorSideBar controller)
            {
                Controller = controller;

                Anchor = Anchor.BottomCentre;
                Origin = Anchor.BottomCentre;
                RelativeSizeAxes = Axes.X;
                Height = 92;
                Padding = new MarginPadding(8);

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
                        PreviewContent = new Container<Drawable>()
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
                if (!slideBlock.Value)
                    slideBlock.Value = true;

                return true;
            }
        }

        private partial class EntryPreviewPlaceholder(EditorSideBar controller) : EntryPreview(controller)
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
                    Margin = new MarginPadding(){ Left = Padding.Left / 2 },
                    Colour = Colour4.Black // Opposite as the background, to not need to wait for the accents to apply
                });

                Controller.PrimaryColor.BindValueChanged((ev) =>
                {
                    text.Colour = ev.NewValue;
                });
            }
        }
    }
}
