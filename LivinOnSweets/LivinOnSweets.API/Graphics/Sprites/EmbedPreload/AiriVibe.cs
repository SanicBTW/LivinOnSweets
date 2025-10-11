using LivinOnSweets.API.Skinning;
using LivinOnSweets.API.Utils;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Textures;

namespace LivinOnSweets.API.Graphics.Sprites.EmbedPreload
{
    public partial class AiriVibe : TextureAnimation
    {
        [BackgroundDependencyLoader]
        private void load(IResourcePackSource pack)
        {
            Anchor = Origin = Anchor.CentreRight;
            // Looks good enough tbh, might change later if I get obsessed with it :grin:
            Margin = new MarginPadding()
            {
                Right = 40,
                Bottom = 53
            };

            Texture texture = pack.GetTexture("GamePreload/Vibing", default, default, false, true, TextureFilteringMode.Nearest);
            texture.ScaleAdjust = 1;
            AddFrames(SpritesheetParser.GetFrames(texture, 1000D, columns: 2));
        }
    }
}
