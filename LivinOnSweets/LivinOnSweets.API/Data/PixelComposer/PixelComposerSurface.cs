using Newtonsoft.Json;

namespace LivinOnSweets.API.Data.PixelComposer
{
    /// <summary>
    /// The structure of the <c>surface</c> object inside <see cref="PixelComposerFrame"/>.
    ///
    /// <remarks>Structure from Pixel Composer 1.17.5</remarks>
    /// </summary>
    public struct PixelComposerSurface
    {
        public int Format { get; set; }
        public string Surface { get; set; }

        [JsonProperty("w")]
        public int Width { get; set; }

        [JsonProperty("h")]
        public int Height { get; set; }
    }
}
