using LivinOnSweets.API.Container;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osuTK;
using OContainer = osu.Framework.Graphics.Containers.Container;

namespace LivinOnSweets.API.Overlays
{
    public partial class EditorContainer : OverlayContainer
    {
        private OContainer canvas;
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
                canvas = new OContainer()
                {
                    RelativeSizeAxes = Axes.Both,
                },
                sideBar = new SlideContainer()
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Y,
                    Width = 450,
                    Child = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.Black
                    }
                },
                propertiesPanel = new SlideContainer()
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Y,
                    Width = 450,
                    LeftSide = false,
                    Child = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.Black
                    }
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

        protected override void PopIn() => this.FadeIn(250D, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(250D, Easing.OutQuint);
    }
}
