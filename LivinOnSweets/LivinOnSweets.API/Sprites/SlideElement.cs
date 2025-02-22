using osu.Framework.Graphics.Sprites;

namespace LivinOnSweets.API.Sprites
{
    // Used inside SlideContainer, holds necessary variables to work within the transitions made in slide container
    public partial class SlideElement : Sprite
    {
        public float StartingX => startingX;
        private float startingX;

        public readonly int Direction;
        public readonly double Delay;

        public SlideElement(float startingX, int direction, double delay)
        {
            this.startingX = X = startingX;

            Direction = direction;
            Delay = delay;
        }
    }
}
