using LivinOnSweets.API.Enum;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace LivinOnSweets.API.Interfaces
{
    public interface IAccentColorReceiver
    {
        public AccentBannerSide AccentSide { get; }

        // Called when PropagateInto was called manually, in order to save up references
        public void PropagateAccents(BindableColour4[] colors);

        public void AccentsUpdated(double duration, Easing easing);
    }
}
