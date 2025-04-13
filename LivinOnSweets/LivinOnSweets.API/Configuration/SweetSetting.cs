namespace LivinOnSweets.API.Configuration
{
    public enum SweetSetting
    {
        // Locale is handled by the framework

        // Graphics
        ShowFpsDisplay,
        IrodoriCanvasMv, // Shows the Music Video rather than the default background
        SongBackgroundParallax, // If the band room should parallax (even if the slightest bg movement disturbs you)

        // Gameplay
        DownScroll, // By default its true to follow the vanilla aspect
        ScrollSpeed, // Can be changed in the song select menu

        // Master (universal), BGM (music) and SE (effect) handled by the framework config
        // Audio
        VolumeInactive,
        AudioOffset,
        // Map specific, not an easter egg, implemented in vanilla
        IrodoriCanvasVoices, // Can be changed in the song select menu

        // Input settings are handled by the framework (mostly mouse ones, keybindings are managed by us)

        // Scaling (from lazer)
        Scaling,
        SafeAreaConsiderations,
        // I'm not using the Background stuff from lazer so we can remove the dim option

        ScalingSizeX,
        ScalingSizeY,

        ScalingPositionX,
        ScalingPositionY,

        UserInterfaceScale,

        // Misc - Screenshot (Copied from lazer tbh)
        ScreenshotFormat,

        // Misc - Intro
        SkipProjectDisclaimer, // Skips the disclaimer shown to the user upon startup

        // Misc - Discord RPC
        DiscordRichPresence,
    }
}
