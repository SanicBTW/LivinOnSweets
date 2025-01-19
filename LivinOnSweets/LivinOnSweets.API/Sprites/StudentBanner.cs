using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Extensions;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Utils;
using osuTK;

namespace LivinOnSweets.API.Sprites
{
    // TODO: Add a config entry to let the user manually select the student banner
    // TODO: Add JSON support for the positioning (margin, anchor, origin)
    public partial class StudentBanner : CompositeDrawable
    {
        protected bool IsLeft;
        protected Students Student;
        public Vector2 AnimOffset { get; protected set; } = Vector2.Zero;
        public string ImageName { get; protected set; }

        public StudentBanner(Students target = Students.RANDOM, bool isLeft = false)
        {
            IsLeft = isLeft;
            Student = target;

            Scale = new Vector2(1.6f);
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            bool hasStudent = Student != Students.RANDOM;
            int targetBanner = hasStudent ? (int)Student : RNG.Next(0, 4);
            if (!hasStudent)
            {
                bool isEven = int.IsEvenInteger(targetBanner);
                targetBanner = isEven switch
                {
                    true when IsLeft => RNG.NextBool() ? (int)Students.KAZUSA : (int)Students.YOSHIMI,
                    false when !IsLeft => RNG.NextBool() ? (int)Students.NATSU : (int)Students.AIRI,
                    _ => targetBanner
                };
            }

            string image = $"bg_{targetBanner}.png";

            InternalChild = new Sprite()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Texture = textures.Get(ImageName = $"Startup/Banners/{image}"),
            };
        }

        // Setting up the slide transition should be instant
        public TransformSequence<StudentBanner> SetupSlideIn(float pos) => this.MoveToOffset(AnimOffset = new Vector2(ApplyMult(pos), 0));

        // we start with the slide, and then the fading, this is because when calling FadeIn it will throw sum on the SlideIn because I'm only accepting StudentBanner, not a generic
        public void FadeNSlideIn(double fadeDuration, double slideDuration) => this.SlideIn(slideDuration).FadeIn(fadeDuration);

        public void FadeNSlideOut(double fadeDuration, double slideDuration) => this.SlideOut(slideDuration).FadeOut(fadeDuration);

        // quick wrapper
        public void FadeNSlideWrap(bool transIn, double fadeDuration, double slideDuration)
        {
            if (transIn)
                FadeNSlideIn(fadeDuration, slideDuration);
            else
                FadeNSlideOut(fadeDuration, slideDuration);
        }

        // Helper
        public float ApplyMult(float pos) => pos * (IsLeft ? 1 : -1);
    }
}
