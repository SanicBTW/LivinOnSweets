using LivinOnSweets.API.Container;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using OContainer = osu.Framework.Graphics.Containers.Container;

namespace LivinOnSweets.API.Overlays
{
    public partial class EditorContainer : OverlayContainer
    {
        // Will contain 6 colors, 3 for the left side (from the left banner) and 3 for the right side (from the right banner)
        public BindableList<Colour4> AccentColors = new();

        private SlideContainer sideBar;
        private SlideContainer propertiesPanel;
        private FillFlowContainer palette;

        public EditorContainer()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black,
                    Alpha = 0.1f
                },
                sideBar = new SlideContainer()
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Y,
                    Width = 450,
                },
                propertiesPanel = new SlideContainer()
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Y,
                    Width = 450,
                    LeftSide = false,
                },
                new CursorContainer()
                /*
                palette = new FillFlowContainer()
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 200,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(10),
                }*/
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Box sideBg;
            SweetScrollContainer scroller;
            sideBar.Children = new Drawable[]
            {
                sideBg = new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black,
                },
                scroller = new SweetScrollContainer()
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    ClampExtension = 20,
                    Child = new EditorSideBar()
                }
            };

            Box propsBg;
            SpriteText wipText;
            propertiesPanel.Child = new OContainer()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    propsBg = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.Black
                    },
                    wipText = new SpriteText()
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = new FontUsage(family: "DNFBitBit", size: 48F),
                        Text = "Work in progress"
                    }
                }
            };

            bool isLeft = true;
            double colorChangeDuration = 1200D;
            AccentColors.BindCollectionChanged((sender, args) =>
            {
                // First call will have the left banner colors, second call will have the right banner colors
                List<Colour4> newColors = (List<Colour4>)AccentColors.SyncRoot;
                int startingIndex = args.NewStartingIndex;

                // Schedule the mutation since its done in another thread for the accent task
                Schedule(() =>
                {
                    if (isLeft)
                    {
                        sideBg.FadeColour(newColors[startingIndex], colorChangeDuration, Easing.OutQuint);
                        this.TransformBindableTo(scroller.ScrollBarColour, newColors[startingIndex + 1], colorChangeDuration, Easing.OutQuint);
                        sideBar.PanelNudge.TransformBindableTo(sideBar.PanelNudge.NudgeColor,
                            newColors[startingIndex + 2], colorChangeDuration, Easing.OutQuint);

                        isLeft = false;
                    }
                    else
                    {
                        propsBg.FadeColour(newColors[startingIndex], colorChangeDuration, Easing.OutQuint);
                        wipText.FadeColour(newColors[startingIndex + 2], colorChangeDuration, Easing.OutQuint);
                        propertiesPanel.PanelNudge.TransformBindableTo(propertiesPanel.PanelNudge.NudgeColor,
                            newColors[startingIndex + 2], colorChangeDuration, Easing.OutQuint);
                    }
                });
            });
        }

        /*
        protected override void LoadComplete()
        {
            base.LoadComplete();

            palette.Children = new Drawable[]
            {
                createPaletteButton("Add Box", () =>
                {
                    canvas.Add(new Box
                    {
                        Size = new Vector2(100),
                        Colour = Colour4.Blue
                    });
                }),
                createPaletteButton("Add Text", () =>
                {
                    canvas.Add(new SpriteText
                    {
                        Text = "New Text",
                        Font = FontUsage.Default.With(size: 20),
                    });
                })
            };
        }*/

        private BasicButton createPaletteButton(string label, Action onClick)
        {
            return new BasicButton()
            {
                RelativeSizeAxes = Axes.X,
                Height = 40,
                Text = label,
                Action = onClick
            };
        }

        protected override void PopIn() => this.FadeIn(500D, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(500D, Easing.OutQuint);
    }
}
