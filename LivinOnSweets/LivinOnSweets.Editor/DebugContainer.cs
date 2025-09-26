using JetBrains.Annotations;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.Overlays;
using LivinOnSweets.API.Screens;
using LivinOnSweets.Editor.Containers;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;

namespace LivinOnSweets.Editor;

// Now sits inside of the overlay container as a topmost overlay
// This is the only exposed type for the Game package
public partial class DebugContainer : Container, IKeyBindingHandler<ManiaAction>
{
    [Resolved] [CanBeNull]
    private IOverlayManager overlayManager { get; set; }

    private readonly SweetScreenStack masterStack;

    private EditorContainer editor;
    [CanBeNull] private IDisposable editorOverlayRegistration;

    public DebugContainer(SweetScreenStack targetStack)
    {
        masterStack = targetStack;

        Depth = -99;
        RelativeSizeAxes = Axes.Both;
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        editorOverlayRegistration = overlayManager?.RegisterBlockingOverlay(editor = new EditorContainer(masterStack));
    }

    public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
    {
        if (e.Action != ManiaAction.EDITOR)
            return false;

        // Since now there's only one debug action, just toggle that bih
        editor.ToggleVisibility();
        return true;
    }

    public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);

        editorOverlayRegistration?.Dispose();
        editorOverlayRegistration = null;
    }
}
