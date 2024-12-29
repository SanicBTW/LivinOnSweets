namespace LivinOnSweets.API.Enum
{
    // Represents the current state INSIDE the gameplay container
    public enum GameplayState
    {
        UNINITIALIZED, // Instance is not created, intro not played
        INITIALIZED, // Instance IS created, player is in the menus
        STORY_MODE,
        SONG_SELECT,
        GAME_OPTIONS
    }
}