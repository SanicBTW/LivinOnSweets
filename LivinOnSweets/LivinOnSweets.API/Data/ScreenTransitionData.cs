using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Enums;

namespace LivinOnSweets.API.Data
{
    public class ScreenTransitionData(ScreenTransitionType transitionType, SweetScreen currentScreen, SweetScreen nextScreen)
    {
        public readonly ScreenTransitionType TransitionType = transitionType;
        public readonly SweetScreen CurrentScreen = currentScreen;
        public readonly SweetScreen NextScreen = nextScreen;
    }
}
