using LivinOnSweets.API.Attributes;
using LivinOnSweets.API.Containers.Editor;
using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Sprites.Editor;

namespace LivinOnSweets.API.EditorComponents
{
    [EditorImportOrder(3)]
    public partial class EditorSettings(EditorSideBar controller) : EditorEntry(controller, "settings", EditorEntryContentAnimation.EXPANDABLE)
    {

    }
}
