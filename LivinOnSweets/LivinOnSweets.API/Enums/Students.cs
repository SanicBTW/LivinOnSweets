using System.ComponentModel;
// ReSharper disable InconsistentNaming

namespace LivinOnSweets.API.Enums
{
    // Descriptions target the most common suffix for the students images
    public enum Students
    {
        [Description("Natsu.png")]
        NATSU,

        [Description("Kazusa.png")]
        KAZUSA,

        [Description("Airi.png")]
        AIRI,

        [Description("Yoshimi.png")]
        YOSHIMI,

        RANDOM
    }
}
