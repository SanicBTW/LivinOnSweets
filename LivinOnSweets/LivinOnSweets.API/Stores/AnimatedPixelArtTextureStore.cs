using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;

namespace LivinOnSweets.API.Stores
{
    // Wrapper for PixelArtTextureStore which adds the capability to parse grid animations
    public class AnimatedPixelArtTextureStore(IRenderer renderer, IResourceStore<TextureUpload> store = null, bool useAtlas = true, bool manualMipmaps = false, float scaleAdjust = 2) : PixelArtTextureStore(renderer, store, useAtlas, manualMipmaps, scaleAdjust)
    {
        // quick wrapper to get frames on a grid animation system, an easy example is Airi inside "Vibing.png"
        public List<FrameData<Texture>> GetFrames(string name, double frameDuration, int rows = 1, int columns = 1)
        {
            Texture texture = Get(name);
            if (texture == null)
                return [];

            int frameWidth = texture.Width / columns;
            int frameHeight = texture.Height / rows;

            List<FrameData<Texture>> frames = [];

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    RectangleI rect = new(x * frameWidth, y * frameHeight, frameWidth, frameHeight);
                    TextureRegion frame = new TextureRegion(texture, rect, texture.WrapModeS, texture.WrapModeT);

                    frames.Add(new FrameData<Texture>()
                    {
                        Content = frame,
                        Duration = frameDuration
                    });
                }
            }

            return frames;
        }
    }
}
