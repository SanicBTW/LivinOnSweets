using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osuTK;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LivinOnSweets.API.Extensions
{
    // Not really an extension but some helper methods for the renderer, since we need the renderer to create the texture
    // Ported over from FlxGridOverlay lmaooo
    public static class GridOverlayExtensions
    {
        public static Texture CreateGrid(this IRenderer renderer, Vector2 cellSize, Vector2 texSize, bool alternate, Colour4 color1,
            Colour4 color2)
        {
            Texture grid = renderer.CreateTexture((int)texSize.X, (int)texSize.Y, true, wrapModeS: WrapMode.Repeat, wrapModeT: WrapMode.Repeat);
            Image<Rgba32> pixels = new Image<Rgba32>(grid.Width, grid.Height);

            Colour4 rowColor = color1;
            Colour4 lastColor = color1;

            int y = 0;

            while (y < pixels.Height)
            {
                if (y > 0 && lastColor == rowColor && alternate)
                    lastColor = (lastColor == color1) ? color2 : color1;
                else if (y > 0 && lastColor != rowColor && !alternate)
                    lastColor = (lastColor == color2) ? color1 : color2;

                int x = 0;

                while (x < pixels.Width)
                {
                    if (x == 0)
                        rowColor = lastColor;

                    fillGridPixel(pixels, x, y, cellSize, lastColor);

                    lastColor = (lastColor == color1) ? color2 : color1;

                    x += (int)cellSize.X;
                }

                y += (int)cellSize.Y;
            }

            grid.SetData(new TextureUpload(pixels));
            return grid;
        }

        private static void fillGridPixel(Image<Rgba32> pixels, int x, int y, Vector2 cellSize, Colour4 fillColor)
        {
            for (int cX = 0; cX < cellSize.X; cX++)
            {
                for (int cY = 0; cY < cellSize.Y; cY++)
                {
                    int sX = x + cX; // x is the startin cell pos, pX is the pixel position
                    int sY = y + cY;
                    pixels[sX, sY] = new Rgba32(fillColor.R, fillColor.G, fillColor.B, fillColor.A); // not using ToRGBA since it breaks black color n shi
                }
            }
        }
    }
}
