using System.ComponentModel;

namespace LivinOnSweets.API.Configuration
{
    // Its cool to let the users play a different version from the mainstream one
    // Though it requires more work and cases to handle, its still a really cool customization option
    public enum GameUpdateVersion
    {
        // The description denotes the Resource Pack ID
        // First release
        [Description("sugar_rush")]
        SugarRush,

        // Event finished, added vocals and changed the logo
        [Description("livin_on_sweets")]
        LivinOnSweets,

        // The idol event
        [Description("antique_seraphim")]
        AntiqueSeraphim,

        // Even finished, added vocals and added a song
        [Description("extra_antique_seraphim")]
        ExtraAntiqueSeraphim,
    }
}
