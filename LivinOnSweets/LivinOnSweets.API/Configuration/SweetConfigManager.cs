using osu.Framework;
using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace LivinOnSweets.API.Configuration
{
    // Kinda copied from osu!lazer and FunkinSharp
    // TODO: Implement tracked settings
    public class SweetConfigManager(Storage storage) : IniConfigManager<SweetSetting>(storage)
    {
        protected override string Filename => "v1_sweetconfig.ini";

        protected override void InitialiseDefaults()
        {
            // Graphics
            SetDefault(SweetSetting.ShowFpsDisplay, false);
            SetDefault(SweetSetting.IrodoriCanvasMv, false);
            SetDefault(SweetSetting.TomodachiStepMv, false);
            SetDefault(SweetSetting.SongBackgroundParallax, true);

            // Gameplay
            SetDefault(SweetSetting.DownScroll, true);
            SetDefault(SweetSetting.ScrollSpeed, 1, 0.5, 10, 0.5);

            // Audio
            SetDefault(SweetSetting.VolumeInactive, 0.25, 0, 1, 0.01);
            SetDefault(SweetSetting.AudioOffset, 0, -2.0, 2.0, 0.1);
            SetDefault(SweetSetting.IrodoriCanvasVoices, false);
            SetDefault(SweetSetting.TomodachiStepVoices, false);

            // Scaling
            SetDefault(SweetSetting.Scaling, ScalingMode.Off);
            SetDefault(SweetSetting.SafeAreaConsiderations, true);

            SetDefault(SweetSetting.ScalingSizeX, 0.8f, 0.2f, 1f, 0.01f);
            SetDefault(SweetSetting.ScalingSizeY, 0.8f, 0.2f, 1f, 0.01f);

            SetDefault(SweetSetting.ScalingPositionX, 0.5f, 0f, 1f, 0.01f);
            SetDefault(SweetSetting.ScalingPositionY, 0.5f, 0f, 1f, 0.01f);

            SetDefault(SweetSetting.UserInterfaceScale, 1f, 0.8f, 1.6f, 0.01f);

            // Misc - Screenshot
            // Overall mobile shouldn't be able to do screenshots IN GAME, so we flagging the saving only for desktop
            SetDefault(SweetSetting.SaveScreenshots, !RuntimeInfo.IsMobile);
            SetDefault(SweetSetting.HideOverlaysOnScreenshot, false);
            SetDefault(SweetSetting.ScreenshotFormat, ScreenshotFormat.Jpg);

            // Misc - Intro
            SetDefault(SweetSetting.SkipProjectDisclaimer, false);

            // Misc - Discord RPC
            SetDefault(SweetSetting.DiscordRichPresence, DiscordPresenceMode.Full);

            // Misc - Game Update Version
            // Progress of each update: v1 (the game was built on top of this): 40%, v2: 5%, v3: 0%
            SetDefault(SweetSetting.GameUpdate, GameUpdateVersion.EventRelease);
        }
    }
}
