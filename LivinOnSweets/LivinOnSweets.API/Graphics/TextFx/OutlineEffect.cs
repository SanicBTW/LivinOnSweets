using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osuTK;

namespace LivinOnSweets.API.Graphics.TextFx
{
    public class OutlineEffect(ColourInfo outlineColour, float outlineSize) : ITextEffect
    {
        // Draws the text 8 times in all directions, using these offsets to move the draw quad
        private static readonly Vector2[] outline_offsets =
        [
            new(-1, -1), new(0, -1), new(1, -1),
            new(-1,  0),             new(1,  0),
            new(-1,  1), new(0,  1), new(1,  1)
        ];

        private ColourInfo outlineColour = outlineColour;
        private float outlineSize = outlineSize;

        void ITextEffect.Update(params dynamic[] updated)
        {
            outlineColour = updated[0];
            outlineSize = updated[1];
        }

        void ITextEffect.Apply(IRenderer renderer, ScreenSpaceCharacterPart part, ColourInfo colour)
        {
            Vector2 inflationPerc = part.InflationPercentage / outlineSize;
            foreach (Vector2 offset in outline_offsets)
            {
                Quad offsetQuad = new Quad(
                    part.DrawQuad.TopLeft + (offset * outlineSize),
                    part.DrawQuad.TopRight + (offset * outlineSize),
                    part.DrawQuad.BottomLeft + (offset * outlineSize),
                    part.DrawQuad.BottomRight + (offset * outlineSize)
                );

                renderer.DrawQuad(
                    part.Texture,
                    offsetQuad,
                    outlineColour,
                    inflationPercentage: inflationPerc);
            }
        }
    }
}
