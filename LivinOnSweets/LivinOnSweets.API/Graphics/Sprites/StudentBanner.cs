using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Enums;
using LivinOnSweets.API.Skinning;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osuTK;

namespace LivinOnSweets.API.Graphics.Sprites
{
    // Just like the old code but with improvements and resource pack support
    public partial class StudentBanner : ResourcePackReloadableDrawable
    {
        [Resolved]
        private SweetConfigManager sweetConfig { get; set; }

        public bool IsLeft;

        private Students student;
        public Students Student
        {
            get => student;
            set
            {
                if (student == value)
                    return;

                student = value;
                refresh();
            }
        }

        public readonly BindableMarginPadding BindableMargin = new(); // to animate n shi

        private Sprite sprite = new()
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        };

        public StudentBanner() : this(Students.Random) { }

        public StudentBanner(Students target, bool isLeft = false)
        {
            // Access the internal variable to avoid calling the refresh function upon creation
            student = target;
            IsLeft = isLeft;
            InternalChild = sprite;

            AutoSizeAxes = Axes.Both;

            BindableMargin.BindValueChanged((ev) => Margin = ev.NewValue);
        }

        protected override void PackChanged(IResourcePackSource pack)
        {
            // only refresh the sprite when its not on the pool i guess it works?
            // it was crashing before cuz of trying to look for bg_4 (sakurako) on the older resource pack
            if (!IsInPool)
                refresh();
        }

        private void refresh()
        {
            bool isCustom = Student == Students.Custom;
            if (isCustom) // This should be resolved by scripting which is not implemented for now (18/5)
                throw new NotSupportedException();

            // Reset sizes
            sprite.Size = Vector2.Zero;

            int gameVer = (int)sweetConfig.Get<GameUpdateVersion>(SweetSetting.GameUpdate);
            bool hasStudent = Student != Students.Random;
            int targetBanner = (int)Student;
            if (!hasStudent)
            {
                if (gameVer >= (int)GameUpdateVersion.AntiqueSeraphim)
                {
                    // will implement it sometime since its just shuffling between sakurako and mine on both left and right sides
                    throw new NotImplementedException();
                }

                int rand = RNG.Next(0, 4); // 0 - 3
                bool isEven = int.IsEvenInteger(rand);
                targetBanner = isEven switch
                {
                    true when IsLeft => RNG.NextBool() ? (int)Students.Kazusa : (int)Students.Yoshimi,
                    false when !IsLeft => RNG.NextBool() ? (int)Students.Natsu : (int)Students.Airi,
                    _ => rand // fallback to the random number
                };
            }

            string image = $"bg_{targetBanner}.png";

            // Lets use the atlases since the banners fit inside of them
            Texture texture = CurrentPack.GetTexture($"Startup/Banners/{image}", WrapMode.None, WrapMode.None, false);
            texture.ScaleAdjust = 1.2F; // This is the original scale used in the og version of this
            sprite.Texture = texture;
        }
    }
}
