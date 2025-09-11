using LivinOnSweets.API.Graphics;
using LivinOnSweets.API.Input;

namespace LivinOnSweets.API.Configuration
{
    // Copied from SessionStatics
    public enum SessionSetting
    {
        /// <summary>
        /// Whether the last positional input received was a touch input.
        /// Used in touchscreen detection scenarios (<see cref="TouchInputInterceptor"/>).
        /// </summary>
        TouchInputActive,

        /// <summary>
        /// Whether there is currently a screenshot being shown.
        /// Used inside <see cref="ScreenshotManager.ScreenshotSprite"/> to wait <see cref="ScreenshotManager.ScreenshotSprite.frames_to_wait"/> to hide the previous screenshot.
        /// </summary>
        ShowingScreenshot,

        /// <summary>
        /// Whether the cursor is visible or not when taking a screenshot.
        /// </summary>
        ScreenshotCursorVisibility,
    }
}
