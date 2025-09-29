using JetBrains.Annotations;
using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Skinning;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace LivinOnSweets.API.Graphics.Sprites
{
    // Old code with improvements, adding support to the new resource pack system
    public partial class StartupDisc : ResourcePackReloadableDrawable
    {
        [CanBeNull] private Container cd;

        [Resolved]
        private SweetConfigManager sweetConfig { get; set; }

        /// <summary>
        /// If set to false, the disc will be visible upon load, otherwise it will load outside of screen, having to call <see cref="Slide"/> or set the Y position manually.
        /// </summary>
        public bool PositionOutsideView = true;

        public bool IsSpinning { get; protected set; }

        public StartupDisc()
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.TopLeft;
            Origin = Anchor.Centre;
        }

        // Starts spinning the disc on the previous rotatiob
        private void start(double duration = 8000D)
        {
            if (IsSpinning)
                return;

            cd.Spin(duration, RotationDirection.Clockwise, cd!.Rotation);
            IsSpinning = true;
        }

        // Stops the loop function by forcing another transform
        private void stop()
        {
            if (!IsSpinning)
                return;

            cd.RotateTo(cd!.Rotation);
            IsSpinning = false;
        }

        public void Slide(bool isTransIn = true, double duration = 2400D)
        {
            float dest = isTransIn ? 0 : -cd!.DrawHeight / 2;

            this.MoveToY(dest, duration, Easing.OutQuint);

            if (isTransIn)
                start();
            else
                stop();
        }

        // this will stop the current cd spin, recreate the child inside the container then spin again
        // make this animated?
        protected override void PackChanged(IResourcePackSource pack)
        {
            bool wasVisible = IsSpinning; // we track the is spinning since it can only be set through slide which hides the disc either way
            stop();

            int gameUpdate = (int)sweetConfig.Get<GameUpdateVersion>(SweetSetting.GameUpdate);

            cd ??= new Container
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            cd.Child = new CdSprite(gameUpdate);
            if (InternalChildren.Count == 0)
                InternalChild = cd;

            MarginPadding curMargin = Margin;
            curMargin.Left = cd.Child.DrawWidth;

            // I don't really like this but uhh whatever it works, until the layout update I won't be tweaking this anymore hopefully
            if (gameUpdate < (int)GameUpdateVersion.AntiqueSeraphim)
            {
                curMargin.Left += 24;
                curMargin.Top = -10;
            }
            else
            {
                curMargin.Left /= 1.1F;
                curMargin.Top = -40;
            }
            Margin = curMargin;

            if (!wasVisible && PositionOutsideView) // this should happen whenever the cd is not playing or if its the first run
                Y = -cd.DrawHeight / 2;

            if (wasVisible)
                Y = 0; // reset the pos in case its visible

            start();
        }

        // Custom sprite which modifies the texture inflation to make the antique seraphim cd texture look correctly
        // instead of having cut off shadow edges (kinda lame fix but its better than the raw one)
        private partial class CdSprite(int gameUpdate) : Sprite
        {
            private readonly int gameUpdate = gameUpdate;

            protected override DrawNode CreateDrawNode() => new CdDrawNode(this);

            [BackgroundDependencyLoader]
            private void load(IResourcePackSource pack)
            {
                bool notAntiqueSeraphim = gameUpdate < (int)GameUpdateVersion.AntiqueSeraphim;

                // using an atlas on the antique seraphim cd breaks the target fix (see cd sprite below)
                Texture texture = pack.GetTexture("Startup/UI/CD", WrapMode.None, WrapMode.None, false);
                texture.ScaleAdjust = notAntiqueSeraphim ? 1.1F : 1.2F;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Texture = texture;
                FillAspectRatio = 1;
                FillMode = FillMode.Fit;
            }

            private partial class CdDrawNode(CdSprite source) : SpriteDrawNode(source)
            {
                private CdSprite source => (CdSprite)Source;

                private int gameUpdate;

                public override void ApplyState()
                {
                    base.ApplyState();
                    gameUpdate = source.gameUpdate;
                }

                protected override void Blit(IRenderer renderer)
                {
                    if (DrawRectangle.Width == 0 || DrawRectangle.Height == 0)
                        return;

                    // if the version is greater than 1 it means we are on antique seraphim which has issues with the texture
                    Vector2 inflation = gameUpdate > (int)GameUpdateVersion.LivinOnSweets ? new Vector2(0.002F) : new Vector2(InflationAmount.X / DrawRectangle.Width, InflationAmount.Y / DrawRectangle.Height);

                    renderer.DrawQuad(Texture, ScreenSpaceDrawQuad, DrawColourInfo.Colour, null, null,
                        inflation,
                        null, TextureCoords);
                }
            }
        }
    }
}
