using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace LivinOnSweets.API.Containers
{
    // https://github.com/SanicBTW/FunkinSharp/blob/legacy/FunkinSharp/FunkinSharp.Game/Core/Containers/ClippedContainer.cs

    public partial class ClippedContainer : ClippedContainer<Drawable> { }

    public partial class ClippedContainer<T> : Container<T>
        where T : Drawable
    {
        protected override Container<T> Content => ClippedContent;

        protected readonly Container<T> ClippedContent;

        public new bool Masking
        {
            get => Content == null || Content.Masking;
            set
            {
                if (Content != null)
                    Content.Masking = value;
            }
        }

        public ClippedContainer()
        {
            Name = "Clipper Mask";
            Masking = true;

            AddInternal(ClippedContent = new Container<T>
            {
                Name = "Clipper Content",
                RelativeSizeAxes = Axes.Both,
                Masking = true
            });
        }

        public void ClipAnchor(Anchor anchor) => Content.Anchor = anchor;

        public void ClipOrigin(Anchor origin) => Content.Origin = origin;
    }
}
