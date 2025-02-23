using System.ComponentModel;
// ReSharper disable InconsistentNaming

namespace LivinOnSweets.API.Enums
{
    // Since the texture is prefixed with the entry, we can use string interpolation to form the texture path
    // MainMenu/Backgrounds/{entry}Slide.png
    public enum MainMenuEntry
    {
        [Description("Play")]
        PLAY,

        [Description("Options")]
        OPTION,

        [Description("Story")]
        STORY
    }
}
