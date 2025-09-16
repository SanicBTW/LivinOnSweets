using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;

namespace LivinOnSweets.API.Utils
{
    public static class SpritesheetParser
    {
        // quick wrapper to get frames on a grid animation system
        public static List<FrameData<Texture>> GetFrames(Texture texture, double frameDuration, int rows = 1,
            int columns = 1)
        {
            int frameWidth = (int)float.Floor(texture.DisplayWidth / columns);
            int frameHeight = (int)float.Floor(texture.DisplayHeight / rows);

            List<FrameData<Texture>> frames = [];

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    RectangleI rect = new(x * frameWidth, y * frameHeight, frameWidth, frameHeight);
                    TextureRegion frame = new TextureRegion(texture, rect, texture.WrapModeS, texture.WrapModeT);

                    frames.Add(new FrameData<Texture>
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
