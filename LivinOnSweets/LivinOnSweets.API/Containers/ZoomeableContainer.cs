using osuTK;

namespace LivinOnSweets.API.Containers
{
    // https://github.com/SanicBTW/FunkinSharp/blob/legacy/FunkinSharp/FunkinSharp.Game/Core/Containers/Camera.cs
    // Maybe it doesn't fit the old Camera code but it acts(?) like I want to:
    // Keep a container as the absolute size and then be able to rescale the content of it without modifying the parent
    public partial class ZoomeableContainer : ClippedContainer
    {
        private float zoom = 1f;

        public float Zoom
        {
            get => zoom;
            set
            {
                zoom = value;
                Content.Scale = new Vector2(zoom);
            }
        }

        public ZoomeableContainer(bool shouldClip = true)
        {
            Masking = shouldClip;
        }
    }
}
