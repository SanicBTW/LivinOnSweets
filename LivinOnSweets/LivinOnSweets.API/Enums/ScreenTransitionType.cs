// ReSharper disable InconsistentNaming

using osu.Framework.Graphics;

namespace LivinOnSweets.API.Enums
{
    public abstract class ScreenTransitionType
    {
        public class FADE(double fadeInDuration = 800D, double fadeOutDuration = 800D, Colour4? color = null, Easing easing = Easing.OutQuint) : ScreenTransitionType
        {
            public readonly double FadeInDuration = fadeInDuration;
            public readonly double FadeOutDuration = fadeOutDuration;
            public readonly Colour4 Color = color ?? Colour4.Black;
            public readonly Easing Easing = easing;
        }

        public class SPRITE_ANIMATED : ScreenTransitionType { }
    }
}
