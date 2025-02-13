using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace LivinOnSweets.Editor.Containers;

// Cache this container to be able to add content to this view, e.g: windows, icons, indicators, etc
[Cached]
internal partial class EditorContainer : OverlayContainer
{
    private ToolBar toolBar;

    public EditorContainer()
    {
        RelativeSizeAxes = Axes.Both;

        InternalChildren =
        [
            new Box()
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colour4.Black,
                Alpha = 0.75f,
            },
            toolBar = new ToolBar()
        ];
    }

    protected override void PopIn()
    {
        this.FadeIn(300D, Easing.OutQuint);
        toolBar.MoveToY(0, 350D, Easing.OutQuint);
    }

    protected override void PopOut()
    {
        toolBar.MoveToY(toolBar.DrawHeight, 350D, Easing.OutQuint);
        this.FadeOut(300D, Easing.OutQuint);
    }
}
