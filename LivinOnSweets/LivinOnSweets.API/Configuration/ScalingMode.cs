using LivinOnSweets.API.Localisation.Strings;
using osu.Framework.Localisation;

namespace LivinOnSweets.API.Configuration
{
    // Should probably tweak it out myself
    // https://github.com/ppy/osu/blob/master/osu.Game/Configuration/ScalingMode.cs
    public enum ScalingMode
    {
        [LocalisableDescription(typeof(LayoutSettings), nameof(LayoutSettings.ScalingOff))]
        Off,

        [LocalisableDescription(typeof(LayoutSettings), nameof(LayoutSettings.ScaleGameplay))]
        Gameplay,

        [LocalisableDescription(typeof(LayoutSettings), nameof(LayoutSettings.ScaleEverything))]
        Everything,

        [LocalisableDescription(typeof(LayoutSettings), nameof(LayoutSettings.ScaleEverythingExcludingOverlays))]
        ExcludeOverlays,
    }
}
