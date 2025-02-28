using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace LivinOnSweets.API.DrawNodes
{
    // Easy draw node which only flips the screen space draw quad
    public partial class FlipDrawNode : SpriteDrawNode
    {
        private bool flipHorizontal;
        private bool flipVertical;

        public FlipDrawNode(Sprite source, bool flipHorizontal, bool flipVertical) : base(source)
        {
            this.flipHorizontal = flipHorizontal;
            this.flipVertical = flipVertical;
        }

        protected override void Blit(IRenderer renderer)
        {
            if (DrawRectangle.Width == 0 || DrawRectangle.Height == 0)
                return;

            Quad renderQuad = GetRenderQuad();
            renderer.DrawQuad(Texture, renderQuad,
                DrawColourInfo.Colour, null, null,
                GetInflationPct(), null, TextureCoords);
        }

        // Code ported from FunkinSharp G2 API 1.0b
        protected virtual Quad GetRenderQuad()
        {
            // Nothing is flipped just return the default quad without accessing anything else
            if (!flipHorizontal && !flipVertical)
                return ScreenSpaceDrawQuad;

            // From https://github.com/SanicBTW/FunkinSharp/blob/legacy/FunkinSharp/FunkinSharp.Game/Core/ReAnimationSystem/ReAnimatedSpriteNode.cs

            Vector2 topLeft = ScreenSpaceDrawQuad.TopLeft;
            Vector2 topRight = ScreenSpaceDrawQuad.TopRight;
            Vector2 bottomLeft = ScreenSpaceDrawQuad.BottomLeft;
            Vector2 bottomRight = ScreenSpaceDrawQuad.BottomRight;

            if (flipHorizontal)
            {
                (topRight.X, topLeft.X) = (topLeft.X, topRight.X);
                (bottomRight.X, bottomLeft.X) = (bottomLeft.X, bottomRight.X);
            }

            if (flipVertical)
            {
                (bottomLeft.Y, topLeft.Y) = (topLeft.Y, bottomLeft.Y);
                (bottomRight.Y, topRight.Y) = (topRight.Y, bottomRight.Y);
            }

            return new Quad(
                topLeft, topRight,
                bottomLeft, bottomRight
            );
        }

        protected virtual Vector2 GetInflationPct() => new(InflationAmount.X / DrawRectangle.Width, InflationAmount.Y / DrawRectangle.Height);
    }

}
