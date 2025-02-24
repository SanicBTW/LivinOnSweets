using System.ComponentModel;
// ReSharper disable InconsistentNaming

namespace LivinOnSweets.API.Enums
{
    // Lists the available options for the pixel button sprites
    // The descriptions provide the image to retrieve, the path will be built in PixelButton
    public enum PixelButtonAsset
    {
        [Description("Confirm")]
        CONFIRM,

        [Description("Back")]
        BACK,

        [Description("Enter")]
        ENTER,

        [Description("Start")]
        START,

        [Description("YConfirm")]
        YELLOW_CONFIRM,
    }
}
