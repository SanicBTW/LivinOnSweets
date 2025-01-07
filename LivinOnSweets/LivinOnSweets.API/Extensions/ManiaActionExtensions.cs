using LivinOnSweets.API.Input;

namespace LivinOnSweets.API.Extensions
{
    // Mostly for debug but might be for sum else soon idk (1/6/25)
    public static class ManiaActionExtensions
    {
        public static bool IsDebugAction(this ManiaAction action)
        {
            #if DEBUG
            return action == ManiaAction.REFRESH || action == ManiaAction.EDITOR;
            #else
            return false;
            #endif
        }
    }
}

