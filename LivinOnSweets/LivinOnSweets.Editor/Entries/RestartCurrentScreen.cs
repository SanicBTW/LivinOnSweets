using LivinOnSweets.API.Screens;
using LivinOnSweets.Editor.Containers;
using LivinOnSweets.Editor.Enums;
using LivinOnSweets.Editor.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace LivinOnSweets.Editor.Entries;

internal partial class RestartCurrentScreen() : ToolBarButton(FontAwesome.Solid.RedoAlt, ToolBarActionType.STANDARD), IHasTooltip
{
    private SweetScreenStack masterStack;

    // this is the only good way i found of propagating the stack without having to pass it down
    // through each item, the dependency tree is somewhat fucked up since debug container tries to cache
    // the stack and the editor container is added on the overlay manager
    [BackgroundDependencyLoader]
    private void load(EditorContainer editorContainer)
    {
        masterStack = editorContainer.TargetStack;
    }

    protected override void Clicked()
    {
        // shouldnt refresh a subscreen
        if (masterStack.IsSubScreenOpen)
        {
            JumpOutAnimation();
            return;
        }

        SweetScreen screen = (SweetScreen)masterStack.CurrentScreen;
        Type screenType = screen.GetType();
        SweetScreen newScreen = (SweetScreen)Activator.CreateInstance(screenType);
        masterStack.Exit();
        masterStack.PushSynchronously(newScreen);
    }

    public LocalisableString TooltipText => "Restart the current screen";
}
