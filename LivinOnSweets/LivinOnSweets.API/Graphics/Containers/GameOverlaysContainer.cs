using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace LivinOnSweets.API.Graphics.Containers
{
    // Quick wrapper class that unifies all the containers that act as an overlay, on 4 different depths
    public partial class GameOverlaysContainer : Container
    {
        private Container overlayContent;
        private Container rightFloatingOverlayContent;
        private Container leftFloatingOverlayContent;

        private Container topMostOverlayContent;

        public GameOverlaysContainer()
        {
            RelativeSizeAxes = Axes.Both;
            AddRange([
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = [
                        overlayContent = new Container { RelativeSizeAxes = Axes.Both },
                        rightFloatingOverlayContent = new Container { RelativeSizeAxes = Axes.Both },
                        leftFloatingOverlayContent = new Container { RelativeSizeAxes = Axes.Both }
                    ]
                },
                topMostOverlayContent = new Container { RelativeSizeAxes = Axes.Both, }
            ]);
        }

        public void AddOverlay(OverlayContainerTarget target, Drawable drawable) => getTarget(target).Add(drawable);

        public void RemoveOverlay(OverlayContainerTarget target, Drawable drawable, bool disposeImmediately = true) =>
            getTarget(target).Remove(drawable, disposeImmediately);

        private Container getTarget(OverlayContainerTarget target) => target switch
        {
            OverlayContainerTarget.Default => overlayContent,
            OverlayContainerTarget.RightContainer => rightFloatingOverlayContent,
            OverlayContainerTarget.LeftContainer => leftFloatingOverlayContent,
            OverlayContainerTarget.TopMost => topMostOverlayContent,
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
    }

    public enum OverlayContainerTarget
    {
        Default,
        RightContainer,
        LeftContainer,
        TopMost
    }
}
