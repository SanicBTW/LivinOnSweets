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
    }
}
