using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Skinning;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;

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
        }

        protected override void PackChanged(IResourcePackSource pack)
        {
            // simple check to see if we running below antique seraphim, kinda redundant check since the resource
            // pack might change but not the game version you get the thing
            int gameVer = (int)sweetConfig.Get<GameUpdateVersion>(SweetSetting.GameUpdate);

            // we running below antique seraphim
            string bgTex = (gameVer < (int)GameUpdateVersion.AntiqueSeraphim) ? "Startup/UI/Background.png" : "Startup/UI/Background.jpg"; // ironic right?
            Texture tex = pack.GetTexture(bgTex, WrapMode.None, WrapMode.None, false, true);
            tex.ScaleAdjust = 4;

            // stretching might happen but its probably the best looking approach, will revisit soon surely
            // i think its still working the same way blame my brain for it
            InternalChild = new Sprite
            {
                Texture = tex,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = Vector2.One,
                FillMode = FillMode.Fill,
            };
        }
    }

}
