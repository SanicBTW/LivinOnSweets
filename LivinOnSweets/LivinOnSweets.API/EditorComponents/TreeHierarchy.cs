using LivinOnSweets.API.Attributes;
using LivinOnSweets.API.Containers.Editor;
using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Sprites.Editor;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace LivinOnSweets.API.EditorComponents
{
    [EditorImportOrder(0)]
    public partial class TreeHierarchy(EditorSideBar controller) : EditorEntry(controller, "tree hierarchy", EditorEntryContentAnimation.ANOTHER_VIEW)
    {
        protected override EntryPreview CreatePreview() => new TreePreview(Controller);

        public partial class TreePreview(EditorSideBar controller) : EntryPreview(controller)
        {
            private SpriteText selectedObject;

            [BackgroundDependencyLoader]
            private void load()
            {
                PreviewContent.Add(selectedObject = new SpriteText()
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Font = new FontUsage(family: "DNFBitBit", size: 32F),
                    Text = "Select a master container",
                    Margin = new MarginPadding(){ Left = Padding.Left / 2 },
                    Colour = Colour4.Black // Opposite as the background, to not need to wait for the accents to apply
                });

                Controller.PrimaryColor.BindValueChanged((ev) =>
                {
                    selectedObject.Colour = ev.NewValue;
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
