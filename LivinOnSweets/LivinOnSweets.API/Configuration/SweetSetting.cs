namespace LivinOnSweets.API.Configuration
{
    // TODO: Document
    public enum SweetSetting
    {
        // Locale is handled by the framework

        // Graphics
        ShowFpsDisplay,
        IrodoriCanvasMv, // Shows the Irodori Canvas Music Video rather than the default background
        TomodachiStepMv, // NOT IMPLEMENTED MISSING V3 CONTENT / Shows the Tomodachi Step Music Video
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
        TomodachiStepVoices, // Can be changed in the song select menu

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
        SaveScreenshots,
        HideOverlaysOnScreenshot,
        ScreenshotFormat,

        // Misc - Intro
        SkipProjectDisclaimer, // Skips the disclaimer shown to the user upon startup

        // Misc - Discord RPC
        DiscordRichPresence,

        // Misc - Game Update Version
        GameUpdate, // Depending on the selected version, some aspects of the game could change
    }
}
