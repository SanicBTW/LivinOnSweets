using LivinOnSweets.API.Enums;
using LivinOnSweets.API.Stores;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace LivinOnSweets.API.Sprites
{
    public partial class CakeClearance : Sprite
    {
        private const string texture_path = "MainMenu/UI/Select/Cakes.png";

        private readonly SongClearanceStat targetStat;

        public CakeClearance(SongClearanceStat stat)
        {
            targetStat = stat;
        }

        // TODO: Load the song store with the clearance stat n shi
        [BackgroundDependencyLoader]
        private void load(AnimatedPixelArtTextureStore animPixStore)
        {
            Texture texture = animPixStore.Get(texture_path);
            texture.ScaleAdjust = 1;

            bool hasIt = true;
            List<FrameData<Texture>> frames = animPixStore.GetFrames(texture_path, 0, 1, 4);

            int frameIndex = (int)targetStat + (hasIt ? 1 : 0);
            Texture = frames[frameIndex].Content;
        }
    }
}
