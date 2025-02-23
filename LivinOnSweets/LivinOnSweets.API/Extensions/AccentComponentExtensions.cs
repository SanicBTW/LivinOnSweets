using LivinOnSweets.API.Components;
using LivinOnSweets.API.Enums;
using LivinOnSweets.API.Interfaces;
using osu.Framework.Bindables;

namespace LivinOnSweets.API.Extensions
{
    public static class AccentComponentExtensions
    {
        public static BindableColour4 GetAccent(this AccentComponent component, IAccentColorReceiver receiver, AccentColorRole role)
            => component.GetBindable(receiver.AccentSide, role);
    }
}
