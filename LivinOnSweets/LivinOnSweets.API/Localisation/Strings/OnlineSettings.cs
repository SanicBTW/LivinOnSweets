using osu.Framework.Localisation;

namespace LivinOnSweets.API.Localisation.Strings
{
    public static class OnlineSettings
    {
        // I don't believe theres nothing to hide so the only thing is you can toggle the discord rpc, kinda redundant to have
        // a select menu for it but eh yea, for da future i guess?

        /// <summary>
        /// "Full"
        /// </summary>
        public static LocalisableString DiscordPresenceFull => new TranslatableString("settings.drpc:full", "Full");

        /// <summary>
        /// "Off"
        /// </summary>
        public static LocalisableString DiscordPresenceOff => new TranslatableString("settings.drpc:off", "Off");
    }
}
