using LivinOnSweets.API.Stores;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;

namespace LivinOnSweets.API.Sprites
{
    // The Airi animation on the load screen
    public partial class AiriVibe : TextureAnimation
    {
        public AiriVibe()
        {
            Anchor = Anchor.CentreRight;
            Origin = Anchor.CentreRight;
            // Looks good enough tbh, might change later if I get obsessed with it :grin:
            Margin = new MarginPadding()
            {
                Right = 40,
                Bottom = 53
            };
        }

        [BackgroundDependencyLoader]
        private void load(MainMenuStore mmStore)
        {
            AddFrames(mmStore.GetFrames("MainMenu/Characters/Vibing.png", 1000, 1, 2));
        }
    }
}
