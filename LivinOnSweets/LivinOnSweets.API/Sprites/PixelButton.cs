using LivinOnSweets.API.Enums;
using LivinOnSweets.API.Stores;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK.Input;

namespace LivinOnSweets.API.Sprites
{
    // A button that uses the button spritesheets, since all the spritesheets are made like pixel art, why not Pixelbutton lmao
    public partial class PixelButton : Sprite
    {
        // 0 being the locale
        private const string button_path = "MainMenu/UI/Buttons/{0}";
        private const string fallback_locale = "en"; // if the texture isn't found on the target locale, fallback to this one

        private string targetImage;
        // Hold the idle and hover/pressed textures to switch when needed
        private Texture idleTexture;
        private Texture activeTexture;

        public Action Action;

        public PixelButton(PixelButtonAsset asset)
        {
            targetImage = asset.GetDescription();
        }

        [BackgroundDependencyLoader]
        private void load(LocalisationManager localeManager, AnimatedPixelArtTextureStore animPixStore)
        {
            ILocalisationStore localeStore = localeManager.CurrentParameters.Value.Store!;
            string locale = localeStore.EffectiveCulture.Name;
            string localisedPath = string.Format(button_path, locale);

            string texturePath = $"{localisedPath}/{targetImage}.png";

            // tries to retrieve the texture if null, fallbacks to the default locale
            Texture spritesheet = animPixStore.Get(texturePath);
            if (spritesheet == null)
            {
                texturePath = texturePath.Replace(locale, fallback_locale);
                spritesheet = animPixStore.Get(texturePath);
            }

            // since the underlying animated pixel art texture store uses a scale of 2, we need to manually set the scale of the texture to 1
            spritesheet.ScaleAdjust = 1;

            List<FrameData<Texture>> frames = animPixStore.GetFrames(texturePath, 0D, 1, 2);
            Texture = idleTexture = frames[0].Content;
            activeTexture = frames[1].Content;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            Texture = idleTexture;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            Texture = activeTexture;
            return true;
        }

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke();
            return true;
        }
    }
}
