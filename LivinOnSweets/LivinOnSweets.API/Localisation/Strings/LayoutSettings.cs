using osu.Framework.Localisation;

namespace LivinOnSweets.API.Localisation.Strings
{
    // Fresh port from lazer, sorry gang
    public static class LayoutSettings
    {
        /// <summary>
        /// "Off"
        /// </summary>
        public static LocalisableString ScalingOff => new TranslatableString("settings.scaling:off", "Off");

        /// <summary>
        /// "Gameplay"
        /// </summary>
        public static LocalisableString ScaleGameplay =>
            new TranslatableString("settings.scaling:gameplay", "Gameplay");

        /// <summary>
        /// "Everything"
        /// </summary>
        public static LocalisableString ScaleEverything =>
            new TranslatableString("settings.scaling:everything", "Everything");

        /// <summary>
        /// "Excluding overlays"
        /// </summary>
        public static LocalisableString ScaleEverythingExcludingOverlays =>
            new TranslatableString("settings.scaling:everything_excluding_overlays", "Excluding overlays");
    }
}
