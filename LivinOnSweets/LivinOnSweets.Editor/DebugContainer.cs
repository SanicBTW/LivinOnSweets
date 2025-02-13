using LivinOnSweets.API.Extensions;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.Overlays;
using LivinOnSweets.Editor.Containers;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;

namespace LivinOnSweets.Editor;

// This is the only exposed type for the Game package
[Cached]
public partial class DebugContainer : Container, IKeyBindingHandler<ManiaAction>
{
    internal ScreenStack MasterStack;

    private EditorContainer editor;

    public DebugContainer(ScreenStack masterStack)
    {
        Depth = -99;
        RelativeSizeAxes = Axes.Both;

        InternalChildren = [ MasterStack = masterStack, editor = new EditorContainer(), new ScreenshotOverlay() ];
    }

    public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
    {
        if (!e.Action.IsDebugAction())
            return false;

        // Since now there's only one debug action, just toggle that bih
        editor.ToggleVisibility();
        return true;
    }

    public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }
}
