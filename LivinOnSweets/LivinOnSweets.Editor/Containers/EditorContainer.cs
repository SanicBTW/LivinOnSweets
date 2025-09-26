using LivinOnSweets.API.Graphics.Containers;
using LivinOnSweets.API.Screens;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;

namespace LivinOnSweets.Editor.Containers;

// bruh
[Cached]
internal partial class EditorContainer : SweetFocusedOverlayContainer
{
    private readonly ToolBar toolBar;

    internal readonly SweetScreenStack TargetStack;

    protected override bool DimMainContent => false;

    public EditorContainer(SweetScreenStack targetStack)
    {
        TargetStack = targetStack;
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
