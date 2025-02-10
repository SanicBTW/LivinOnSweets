using LivinOnSweets.API.Input;

namespace LivinOnSweets.API.Extensions
{
    // Mostly for debug but might be for sum else soon idk (1/6/25)
    // Now the debug actions are exposed to the public (10/2/25)
    public static class ManiaActionExtensions
    {
        public static bool IsDebugAction(this ManiaAction action) => action == ManiaAction.EDITOR;
    }
}
