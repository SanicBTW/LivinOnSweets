namespace LivinOnSweets.API.StateMachines
{
    /// <summary>
    /// Enum representing the state of the gameplay container.
    /// </summary>
    public enum GameplayState
    {
        NotReady, // Instance is not created
        Ready, // Instance IS created and is running, on the menus

        StorySelect, // Selecting a story chapter
        ReadingChapter, // Reading a story chapter

        SongSelect, // Selecting a song
        PlayingSong, // Playing a song (duh)
        SongPaused, // Displaying the pause menu

        GameOptions, // Options container
    }
}
