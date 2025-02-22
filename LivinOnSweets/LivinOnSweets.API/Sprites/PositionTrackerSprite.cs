using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace LivinOnSweets.API.Sprites
{
    public partial class PositionTrackerSprite : Sprite
    {
        private Drawable follow;
        private double followDelay;
        private Easing followEase;
        private Func<Vector2, Vector2> posTransformer;

        public PositionTrackerSprite(Drawable target, double delay, Easing ease = Easing.None, [CanBeNull] Func<Vector2, Vector2> positionTransform = null)
        {
            follow = target;
            followDelay = delay;
            followEase = ease;
            posTransformer = positionTransform ?? (v => v);
        }

        protected override void Update()
        {
            base.Update();

            if (follow == null)
                return;

            Vector2 position = posTransformer(follow.Position);
            if (followDelay > 0)
                this.MoveTo(position, followDelay, followEase);
            else
                Position = position;
        }
    }
}
