using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Skinning;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace LivinOnSweets.API.Graphics.Sprites.Startup
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
            Texture tex = pack.GetTexture("Startup/UI/Background", WrapMode.None, WrapMode.None, false, true);
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
