using LivinOnSweets.API.Localisation.Strings;
using osu.Framework.Localisation;

namespace LivinOnSweets.API.Configuration
{
    public enum DiscordPresenceMode
    {
        [LocalisableDescription(typeof(OnlineSettings), nameof(OnlineSettings.DiscordPresenceOff))]
        Off,

        [LocalisableDescription(typeof(OnlineSettings), nameof(OnlineSettings.DiscordPresenceFull))]
        Full,
    }
}
