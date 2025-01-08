using LivinOnSweets.API.Container;
using LivinOnSweets.API.Enum;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using OContainer = osu.Framework.Graphics.Containers.Container;

namespace LivinOnSweets.API.Sprites
{
    public partial class EditorEntry : OContainer
    {
        public readonly string Category;
        public double ColorFadeDuration = 500D;

        protected override OContainer Content => NewContent;
        protected OContainer NewContent; // The content that will use any Child/Children call set, in this case probably an EntryContent object

        protected OContainer RoundedMask; // The parent container of all the content inside this entry

        private Box background;
        private EditorSideBar controller;

        public EditorEntry(EditorSideBar controller, string category, EditorEntryContentAnimation entryType)
        {
            this.controller = controller;
            Category = category;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            // Figma reported this size so I'm doing it :grin:
            Size = new Vector2(400, 110);
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

            controller.PrimaryColor.BindValueChanged((ev) =>
            {
                background.Colour = ev.NewValue;
            });
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(controller.PrimaryColor.Value.Lighten(0.15f), ColorFadeDuration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeColour(controller.PrimaryColor.Value, ColorFadeDuration, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected virtual EntryHeader CreateHeader(string category) => new(category, controller);

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
    }
}
