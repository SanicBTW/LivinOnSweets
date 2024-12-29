using LivinOnSweets.API.StartupObjects;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace LivinOnSweets.API.Extensions
{
    public static class StudentBannerExtensions
    {
        // Instead of using the X position I'm saving the set offset through "SetupSlideIn" so we can use it to move the banner constantly without having to do too much work
        public static TransformSequence<StudentBanner> SlideIn(this StudentBanner banner, double duration) => banner.MoveToOffset(new osuTK.Vector2(
            -banner.AnimOffset.X, // this is crazy work ngl, went from banner.ApplyMult(banner.ApplyMult(banner.X) to -X, I realized too late I was doing the same thing lmfao
            0), duration, Easing.OutQuint);

        public static TransformSequence<StudentBanner> SlideOut(this StudentBanner banner, double duration) => banner.MoveToOffset(new osuTK.Vector2(
            banner.AnimOffset.X,
            0), duration, Easing.OutQuint);
    }
}
