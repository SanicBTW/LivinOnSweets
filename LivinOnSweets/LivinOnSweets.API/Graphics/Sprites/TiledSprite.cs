using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace LivinOnSweets.API.Graphics.Sprites
{
    // Fashion sprite of all times!!!!
    /// <summary>
    /// Pretty unsafe <see cref="Sprite"/> which overrides the blitting Texture Rectangle and avoids invalidations.
    /// <remarks>When loading up a <see cref="Texture"/> make sure both <see cref="WrapMode"/>'s are set to <see cref="WrapMode.Repeat"/></remarks>
    /// </summary>
    public partial class TiledSprite : Sprite
    {
        private RectangleF tileTextureRect;
        private bool textureRectDirty = true;

        // errr i should name it another way
        public RectangleF TileTextureRect
        {
            get => tileTextureRect;
            set
            {
                if (tileTextureRect == value)
                    return;

                tileTextureRect = value;
                textureRectDirty = true;
            }
        }

        protected override void LoadComplete()
        {
            TextureRelativeSizeAxes = Axes.None;
            TileTextureRect = TextureRectangle;
        }

        protected override DrawNode CreateDrawNode() => new TiledSpriteDrawNode(this);

        // bruh bruh bruh
        protected class TiledSpriteDrawNode(TiledSprite source) : SpriteDrawNode(source)
        {
            public new TiledSprite Source => (TiledSprite)base.Source;

            private RectangleF textureRectangle = new(0, 0, 0, 0);

            protected override void Blit(IRenderer renderer)
            {
                if (DrawRectangle.Width == 0 || DrawRectangle.Height == 0)
                    return;

                if (Source.textureRectDirty)
                {
                    calculateTexCoords();
                    Source.textureRectDirty = false;
                }

                renderer.DrawQuad(Texture, ScreenSpaceDrawQuad, DrawColourInfo.Colour, null, null,
                    new Vector2(InflationAmount.X / DrawRectangle.Width, InflationAmount.Y / DrawRectangle.Height),
                    null, textureRectangle);
            }

            // is there anyway to cache this?
            private void calculateTexCoords()
            {
                textureRectangle = Source.DrawRectangle.RelativeIn(Source.TileTextureRect);
                if (Texture != null)
                    textureRectangle *= new Vector2(Texture.DisplayWidth, Texture.DisplayHeight);
            }
        }
    }

}
