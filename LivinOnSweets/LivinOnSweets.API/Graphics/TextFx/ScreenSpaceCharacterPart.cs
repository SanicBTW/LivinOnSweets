using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace LivinOnSweets.API.Graphics.TextFx
{
    public struct ScreenSpaceCharacterPart
    {
        /// <summary>
        /// The screen-space quad for the character to be drawn in.
        /// </summary>
        public Quad DrawQuad;

        /// <summary>
        /// Extra padding for the character's texture.
        /// </summary>
        public Vector2 InflationPercentage;

        /// <summary>
        /// The texture to draw the character with.
        /// </summary>
        public Texture Texture;
    }
}
