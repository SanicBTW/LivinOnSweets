using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osuTK;
using osuTK.Graphics;

namespace LivinOnSweets.API.Graphics.TextFx
{
    public class ShadowEffect(ColourInfo shadowColour, Vector2 shadowOffset) : ITextEffect
    {
        private ColourInfo shadowColour = shadowColour;
        private Vector2 shadowOffset = shadowOffset;
        private ColourInfo finalShadowColour;

        void ITextEffect.Update(params dynamic[] updated)
        {
            shadowColour = updated[0];
            shadowOffset = updated[1];
        }

        void ITextEffect.Recalculate(ColourInfo colour)
        {
            Color4 avgColour = colour.AverageColour;
            float shadowAlpha = MathF.Pow(Math.Max(Math.Max(avgColour.R, avgColour.G), avgColour.B), 2);

            finalShadowColour = colour;
            finalShadowColour.ApplyChild(shadowColour.MultiplyAlpha(shadowAlpha));
        }

        void ITextEffect.Apply(IRenderer renderer, ScreenSpaceCharacterPart part, ColourInfo colour)
        {
            renderer.DrawQuad(
                part.Texture,
                new Quad(
                    part.DrawQuad.TopLeft + shadowOffset,
                    part.DrawQuad.TopRight + shadowOffset,
                    part.DrawQuad.BottomLeft + shadowOffset,
                    part.DrawQuad.BottomRight + shadowOffset),
                finalShadowColour,
                inflationPercentage: part.InflationPercentage);
        }
    }
}
