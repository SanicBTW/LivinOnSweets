using System.ComponentModel;

namespace LivinOnSweets.API.Enums
{
    // https://github.com/SanicBTW/LivinOnSweets/blob/master/LivinOnSweets/LivinOnSweets.API/Enums/PixelButtonAsset.cs

    public enum PixelButtonType
    {
        [Description("Confirm")]
        Confirm,

        [Description("Back")]
        Back,

        [Description("Enter")]
        Enter,

        [Description("Start")]
        Start,

        [Description("YConfirm")]
        YellowConfirm,
    }
}

