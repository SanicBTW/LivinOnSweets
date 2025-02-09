using System.ComponentModel;

namespace LivinOnSweets.API.Enum
{
    // Why dont I make string concat? I uhh idk, lets do this for now
    public enum MainMenuEntry
    {
        [Description("MainMenu/Backgrounds/PlaySlide.png")]
        PLAY,

        [Description("MainMenu/Backgrounds/OptionsSlide.png")]
        OPTION,

        [Description("MainMenu/Backgrounds/StorySlide.png")]
        STORY
    }
}
