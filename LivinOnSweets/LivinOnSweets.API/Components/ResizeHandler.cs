
// Stored in components but in reality its only used to listen for resizes being passed down from the dependency container

using System.Drawing;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace LivinOnSweets.API.Components
{
    // TODO: Get this working properly and use it?
    public class ResizeHandler
    {
        private Bindable<Vector2> backingVector = new(Vector2.Zero);
        private List<(DrawSizePreservingFillContainer, Axes)> targets = [];

        public Vector2 WorkingSize => backingVector.Value;

        public ResizeHandler()
        {
            backingVector.BindValueChanged(handleResize);
        }

        private void handleResize(ValueChangedEvent<Vector2> ev)
        {
            List<int> removalIdxs = [];

            foreach (var target in targets)
            {
                DrawSizePreservingFillContainer container = target.Item1;

                // bruh
                if (!container.IsAlive)
                {
                    removalIdxs.Add(targets.IndexOf(target));
                    continue;
                }

                Vector2 targetSize = container.TargetDrawSize;
                switch (target.Item2)
                {
                    case Axes.Both:
                        targetSize = ev.NewValue;
                        break;

                    case Axes.X:
                        float ratioX = Math.Min(1, targetSize.X / ev.NewValue.X);
                        targetSize = new Vector2(targetSize.X, ev.NewValue.Y);
                        break;

                    case Axes.Y:
                        float ratioY = targetSize.Y / ev.NewValue.Y;
                        targetSize = new Vector2(ev.NewValue.X, targetSize.Y * ratioY);
                        break;

                    case Axes.None:
                    default:
                        return;
                }

                container.Size = targetSize;
            }

            foreach (int idx in removalIdxs)
                targets.RemoveAt(idx);
        }

        public DrawSizePreservingFillContainer Bind(DrawSizePreservingFillContainer container, Axes targetAxes = Axes.None)
        {
            if (targetAxes == 0)
                return container;

            targets.Add((container, targetAxes));
            return container;
        }

        public void UpdateSize(float width, float height) => backingVector.Value = new Vector2(width, height);

        public void UpdateSize(Size size) => backingVector.Value = new Vector2(size.Width, size.Height);
    }
}

