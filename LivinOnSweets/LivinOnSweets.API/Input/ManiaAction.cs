namespace LivinOnSweets.API.Input
{
    // https://github.com/SanicBTW/FunkinSharp/blob/legacy/FunkinSharp/FunkinSharp.Game/Funkin/FunkinKeybinds.cs#L195

    // This is something FunkinSharp doesn't do, here we set NOTE_LEFT starting on the value 0, since 0 is the starting value of the
    // enums, we make the first entry (CONFIRM) start at the end of the NOTES section (0,1,2,3 so 4 is after the notes section)
    // this is to make indexing the lanes faster and avoid casting or sum
    public enum ManiaAction
    {
        // GLOBAL
        CONFIRM = 4,
        BACK,
        REFRESH, // DEBUG

        // VOLUME
        VOLUME_UP,
        VOLUME_DOWN,
        VOLUME_MUTE,

        // UI
        UI_LEFT,
        UI_DOWN,
        UI_UP,
        UI_RIGHT,

        // NOTES
        NOTE_LEFT = 0,
        NOTE_DOWN,
        NOTE_UP,
        NOTE_RIGHT
    }
}
