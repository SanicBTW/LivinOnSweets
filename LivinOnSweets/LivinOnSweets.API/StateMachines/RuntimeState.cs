namespace LivinOnSweets.API.StateMachines
{
    // Ressembles the old API fields

    /// <summary>
    /// Enum representing the state of the game runtime.
    /// </summary>
    public enum RuntimeState
    {
        Startup, // First screen
        InGame, // Focusing the game container
        EngineSettings, // Settings overlay or screen
        ClosePrompt, // Close popup (not implemented)
        Closing, // Close confirmed
    }
}
