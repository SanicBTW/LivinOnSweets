using LivinOnSweets.API.Localisation.Strings;
using osu.Framework.Localisation;

namespace LivinOnSweets.API.Configuration
{
    public enum ScreenshotFormat
    {
        [LocalisableDescription(typeof(GraphicsSettings), nameof(GraphicsSettings.Jpg))]
        Jpg = 1,

        [LocalisableDescription(typeof(GraphicsSettings), nameof(GraphicsSettings.Png))]
        Png = 2,
    }
}
