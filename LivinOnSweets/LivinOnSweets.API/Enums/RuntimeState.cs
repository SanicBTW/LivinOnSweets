// ReSharper disable InconsistentNaming
namespace LivinOnSweets.API.Enums
{
    // Represents the state of the StartupScreen to handle some operations
    public enum RuntimeState
    {
        STARTUP, // First screen
        IN_GAME, // Inside game container
        ENGINE_SETTINGS, // Settings container
        CLOSE_PROMPT, // Close popup
        CLOSING, // Close confirmed
    }
}
