using LivinOnSweets.API.Overlays;
using osu.Framework.Bindables;
using osu.Framework.Screens;

namespace LivinOnSweets.API.Screens
{
    // https://github.com/ppy/osu/blob/master/osu.Game/Screens/IOsuScreen.cs
    public interface ISweetScreen : IScreen
    {
        /// <summary>
        /// Whether overlays should be able to be opened when this screen is current.
        /// </summary>
        IBindable<OverlayActivation> OverlayActivationMode { get; }
    }
}
