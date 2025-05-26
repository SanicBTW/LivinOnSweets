using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Skinning;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace LivinOnSweets.API.Graphics.Sprites
{
    // this should scale in proportion of the scaling set
    public partial class StartupBackground : ResourcePackReloadableDrawable
    {
        [Resolved]
        private SweetConfigManager sweetConfig { get; set; }

        public StartupBackground()
        {
            Anchor = Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Alpha = 0;
        }

        protected override void PackChanged(IResourcePackSource pack)
        {
            // simple check to see if we running below antique seraphim, kinda redundant check since the resource
            // pack might change but not the game version you get the thing
            int gameVer = (int)sweetConfig.Get<GameUpdateVersion>(SweetSetting.GameUpdate);

            // we running below antique seraphim
            string bgTex = (gameVer < (int)GameUpdateVersion.AntiqueSeraphim) ? "Startup/UI/Background.png" : "Startup/UI/Background.jpg"; // ironic right?
            // stretching might happen but its probably the best looking approach, will revisit soon surely
            InternalChild = new Sprite
            {
                Texture = pack.GetTexture(bgTex, WrapMode.None, WrapMode.None, false, true),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Width = 1.5F,
                //RelativeSizeAxes = Axes.Both,
                //Size = Vector2.One,
                // FillMode = FillMode.Fill,
            };

            this.FadeIn(500D);
        }
    }

}
