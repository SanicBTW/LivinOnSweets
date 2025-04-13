using osu.Framework.Localisation;

namespace LivinOnSweets.API.Localisation.Strings
{
    public static class GraphicsSettings
    {
        /// <summary>
        /// "JPG (web-friendly)
        /// </summary>
        public static LocalisableString Jpg => new TranslatableString("settings.graphics:jpg_wf", "JPG (web-friendly)");

        /// <summary>
        /// "PNG (lossless)"
        /// </summary>
        public static LocalisableString Png =>
            new TranslatableString("settings.graphics:png_lossless", "PNG (lossless)");
    }
}
