namespace LivinOnSweets.API.Data.PixelComposer
{
    /// <summary>
    /// The structure of a JSON object inside a Pixel Composer Animation structure
    ///
    /// <remarks>Structure from Pixel Composer 1.17.5</remarks>
    /// </summary>
    public struct PixelComposerFrame()
    {
        public int Blend { get; set; } = 0;
        public float Rotation { get; set; } = 0;
        public float[] Scale { get; set; } = [];
        public float[] Size { get; set; } = [];
        public float Alpha { get; set; } = 0;
        public PixelComposerSurface Surface { get; set; } = default;
        public float[] Position { get; set; } = [];
    }
}
