using System.Collections.Specialized;
using LivinOnSweets.API.Container;
using LivinOnSweets.API.Data;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using OContainer = osu.Framework.Graphics.Containers.Container;

namespace LivinOnSweets.API.Overlays
{
    public partial class EditorContainer : OverlayContainer
    {
        private SlideContainer sideBar;
        private SlideContainer propertiesPanel;
        private FillFlowContainer palette;
        private SlideContainer[] Sliders;

        private bool isLeftPopulation = true;
        private double colorChangeDuration = 1200D;

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
                sideBar = new SlideContainer(true)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Y,
                    Width = 450,
                },
                propertiesPanel = new SlideContainer(false)
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Y,
                    Width = 450,
                },
                /*
                palette = new FillFlowContainer()
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 200,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(10),
                }*/
            };

            Sliders = [sideBar, propertiesPanel];
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Box sideBg;
            SweetScrollContainer scroller;
            EditorSideBar editorSide;
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
                    ClampExtension = 10,
                    Child = editorSide = new EditorSideBar()
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

            // I should use the sender or args, whatever
            EditorColours.PrimaryColors.BindCollectionChanged((sender, args) =>
            {
                ApplyColors((List<Colour4>)EditorColours.PrimaryColors.SyncRoot, sideBg, editorSide, scroller, sideBar.PanelNudge);
            });

            EditorColours.SecondaryColors.BindCollectionChanged((sender, args) =>
            {
                ApplyColors((List<Colour4>)EditorColours.SecondaryColors.SyncRoot, propsBg, wipText, propertiesPanel.PanelNudge);
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

        protected virtual void ApplyColors(List<Colour4> newColors, params dynamic[] targets)
        {
            // I still kinda hate this buttt its somewhat better than before ig

            // Schedule the mutation since its done in another thread for the accent task
            Schedule(() =>
            {
                Box background = targets[0];

                background.FadeColour(newColors[0], colorChangeDuration, Easing.OutQuint);

                if (isLeftPopulation)
                {
                    EditorSideBar editorSide = targets[1];
                    SweetScrollContainer scroller = targets[2];
                    SlideContainer.Nudge panelNudge = targets[3];

                    // I should be doing these transforms inside their respective containers but whatever
                    this.TransformBindableTo(editorSide.PrimaryColor, newColors[1], colorChangeDuration, Easing.OutQuint);
                    this.TransformBindableTo(editorSide.SecondaryColor, newColors[2], colorChangeDuration, Easing.OutQuint);

                    this.TransformBindableTo(scroller.ScrollBarColour, newColors[1], colorChangeDuration, Easing.OutQuint);
                    this.TransformBindableTo(panelNudge.NudgeColor, newColors[2], colorChangeDuration, Easing.OutQuint);

                    isLeftPopulation = false;
                }
                else
                {
                    SpriteText text = targets[1];
                    SlideContainer.Nudge propNudge = targets[2];

                    text.FadeColour(newColors[2], colorChangeDuration, Easing.OutQuint);
                    this.TransformBindableTo(propNudge.NudgeColor, newColors[2], colorChangeDuration, Easing.OutQuint);
                }
            });
        }

        protected override void PopIn() => this.FadeIn(500D, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(500D, Easing.OutQuint);

        // Resets the slide blocks when clicked outside
        protected override bool OnMouseDown(MouseDownEvent e)
        {
            foreach (SlideContainer slider in Sliders)
            {
                if (slider.IsVisible() && slider.SlideBlock.Value)
                {
                    slider.SlideBlock.Value = false;
                    slider.MoveToX(slider.OutOfBoundsPosition, slider.SlideDuration, Easing.OutQuint);
                }
            }

            return base.OnMouseDown(e);
        }
    }
}
