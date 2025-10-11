using osu.Framework.Graphics.Sprites;

namespace LivinOnSweets.API.Graphics.Sprites
{
    // from https://github.com/SanicBTW/LivinOnSweets/blob/master/LivinOnSweets/LivinOnSweets.API/Sprites/SlideElement.cs
    // Used inside SlideContainer, holds necessary variables to work within the transitions made in slide container
    public partial class SlideElement : Sprite
    {
        public float StartingX { get; }
        public float StartingY { get; } // we keep a reference just in case yknow

        public readonly int Direction;
        public readonly double Delay;

        public SlideElement(float startingX, float yPos, int direction, double delay)
        {
            StartingX = X = startingX;
            StartingY = Y = yPos;

            Direction = direction;
            Delay = delay;
        }
    }
}
