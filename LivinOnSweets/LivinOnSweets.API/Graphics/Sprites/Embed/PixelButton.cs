using LivinOnSweets.API.Enums;
using LivinOnSweets.API.Skinning;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;

namespace LivinOnSweets.API.Graphics.Sprites.Embed
{
    // https://github.com/SanicBTW/LivinOnSweets/blob/master/LivinOnSweets/LivinOnSweets.API/Sprites/PixelButton.cs

    public partial class PixelButton(PixelButtonType btnType) : FramedButton
    {
        private const string default_path = "UserInterface/PixelButtons/{0}"; // until the localisation packs are not implemented, we will use this method
        private const string default_locale = "en"; // if the texture isn't found on the target locale, fallback to this one

        [BackgroundDependencyLoader]
        private void load(LocalisationManager localeManager, IResourcePackSource pack)
        {
            ILocalisationStore localeStore = localeManager.CurrentParameters.Value.Store!;
            string locale = localeStore.EffectiveCulture.Name;
            string localisedPath = string.Format(default_path, locale);

            string texturePath = $"{localisedPath}/{btnType.GetDescription()}";
            Texture texture = LoadTexture(texturePath, pack);
            if (texture == null)
            {
                texturePath = texturePath.Replace(locale, default_locale);
                texture = LoadTexture(texturePath, pack);
            }

            LoadFrames(texture);
        }
    }
}
