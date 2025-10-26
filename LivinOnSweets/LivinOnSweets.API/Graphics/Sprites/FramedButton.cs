using LivinOnSweets.API.Data;
using LivinOnSweets.API.Skinning;
using LivinOnSweets.API.Utils;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;

// ReSharper disable MemberCanBePrivate.Global

namespace LivinOnSweets.API.Graphics.Sprites
{
    // I wonder if I should make it a real frame based animation or just the basic one that switches between 2 frames
    // https://github.com/SanicBTW/LivinOnSweets/blob/master/LivinOnSweets/LivinOnSweets.API/Sprites/PixelButton.cs

    public partial class FramedButton(string sheet = null) : Sprite
    {
        protected List<FrameData<Texture>> Frames = [];

        private Action action;

        public Action Action
        {
            get => action;
            set
            {
                action = value;
                Enabled.Value = action != null;
            }
        }

        public readonly BindableBool Enabled = new(true);

        // should fire a funny event on every load function (in aether) to be able to override the load call
        [BackgroundDependencyLoader]
        private void load(IResourcePackSource pack)
        {
            if (sheet == null)
                return;

            Texture texture = LoadTexture(sheet, pack);
            LoadFrames(texture);
        }

        protected virtual Texture LoadTexture(string texturePath, IResourcePackSource pack)
        {
            MenuEntryInfo.TextureUploadInfo texUploadInfo = RetrieveTextureOptions();
            Texture texture = pack.GetTexture(texturePath,
                MenuEntryInfo.WrapModeInfo.Parse(texUploadInfo.WrapMode.WrapHorizontal), MenuEntryInfo.WrapModeInfo.Parse(texUploadInfo.WrapMode.WrapVertical),
                texUploadInfo.UseAtlas, texUploadInfo.ManualMipmaps, MenuEntryInfo.TextureUploadInfo.ParseFilteringMode(texUploadInfo.FilteringMode));

            if (texture == null)
                return null; // should warn

            texture.ScaleAdjust = texUploadInfo.ScaleAdjust;
            return texture;
        }

        protected virtual void LoadFrames(Texture texture, int defaultFrame = 0)
        {
            // most likely to be spread horizontally, should add params or an object or something
            Frames = SpritesheetParser.GetFrames(texture, 0D, 1, 2);
            Texture = Frames[defaultFrame].Content;
        }

        protected override void LoadComplete()
        {
            Enabled.BindValueChanged(_ => UpdateState(), true);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (!Enabled.Value) return;
            SetFrame(0);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (!Enabled.Value) return false;
            SetFrame(1);
            return true;
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (Enabled.Value)
                Action?.Invoke();
            return true;
        }

        protected virtual void UpdateState()
        {
            if (Enabled.Value)
                SetFrame(0);
            else
                SetFrame(Frames.Count - 1);
        }

        protected virtual void SetFrame(int frameIndex)
        {
            if (Frames.Count <= 0 || Texture == null) return;

            frameIndex = Math.Clamp(frameIndex, 0, Frames.Count - 1);
            Texture = Frames[frameIndex].Content;
        }

        // should move this or call an event which asks for the texture options and returns the one based on the given type, dunno
        protected virtual MenuEntryInfo.TextureUploadInfo RetrieveTextureOptions() => new()
        {
            UseAtlas = false,
            FilteringMode = "nearest",
            ScaleAdjust = 1,
        };
    }
}
