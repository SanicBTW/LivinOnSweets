using System.ComponentModel;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming

namespace LivinOnSweets.API.Localisation
{
    // https://github.com/ppy/osu/blob/master/osu.Game/Localisation/Language.cs
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public enum Language
    {
        [Description(@"english")]
        en,

        // Not officially supported by the og version but since I'm spanish, why not support it?
        [Description(@"español")]
        es,

        [Description(@"한국어")]
        ko,

        // Apparently this is traditional chinese = zh_hant / zh_tw
        [Description(@"繁體中文（台灣）")]
        tc,

        [Description(@"ไทย")]
        th
    }
}
