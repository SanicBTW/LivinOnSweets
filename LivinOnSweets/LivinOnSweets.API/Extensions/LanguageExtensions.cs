using LivinOnSweets.API.Localisation;
using osu.Framework.Localisation;

namespace LivinOnSweets.API.Extensions
{
    // https://github.com/ppy/osu/blob/master/osu.Game/Extensions/LanguageExtensions.cs
    public static class LanguageExtensions
    {
        public static string ToCultureCode(this Language language) => language.ToString().Replace("_", "-");

        public static bool TryParseCultureCode(string cultureCode, out Language language) =>
            Enum.TryParse(cultureCode.Replace("-", "_"), out language);

        public static Language GetLanguageFor(string frameworkLocale, LocalisationParameters localisationParameters)
        {
            if (TryParseCultureCode(frameworkLocale, out var language))
                return language;

            if (localisationParameters.Store == null) return Language.en;
            return TryParseCultureCode(localisationParameters.Store.EffectiveCulture.Name, out language) ? language : Language.en;
        }
    }
}
