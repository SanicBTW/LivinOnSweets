using LivinOnSweets.API.Components;
using LivinOnSweets.API.Data;
using LivinOnSweets.API.Graphics.Containers;
using LivinOnSweets.API.Skinning;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;

namespace LivinOnSweets.API.Graphics.Sprites
{
    public partial class CharacterParallaxBackground(MenuEntryInfo.EntryInfo info, ref Container<SlideElement> targetContainer) : SlideContainer
    {
        [Resolved] private LoadManager loadManager { get; set; }

        public string Id => info.Id.ToLowerInvariant(); // we forcing to lower to uhhh avoid casing problems on scripting?

        // polymorphic
        protected override Container<SlideElement> Content { get; } = targetContainer;
        private readonly List<SlideElement> slideElements = [];

        public bool FinishedTransform => Precision.AlmostEquals(LatestTransformEndTime - Time.Current, 0);

        [BackgroundDependencyLoader]
        private void load(IResourcePackSource pack)
        {
            RelativeSizeAxes = Axes.Both;

            RemoveInternal(Elements, true);

            Texture bgTexture = pack.GetTexture($"MainMenu/{info.Background}", default, default, false, true);
            bgTexture.ScaleAdjust = 1;
            loadManager.Register(new Sprite()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Texture = bgTexture,
                Name = info.Id.ToLowerInvariant(),
                Depth = 99
            }, t => Schedule(AddInternal, t));

            AddCharacters(pack);
            AddExtras(pack);
            AddLabel(pack);
        }

        protected virtual void AddCharacters(IResourcePackSource pack)
        {
            foreach (MenuEntryInfo.CharacterInfo charInfo in info.Characters)
            {
                MenuEntryInfo.TextureUploadInfo texUp = charInfo.TextureUpload;
                Texture charTex = pack.GetTexture($"MainMenu/{charInfo.Texture}", MenuEntryInfo.WrapModeInfo.Parse(texUp.WrapMode.WrapHorizontal),
                    MenuEntryInfo.WrapModeInfo.Parse(texUp.WrapMode.WrapVertical), texUp.UseAtlas, texUp.ManualMipmaps, MenuEntryInfo.TextureUploadInfo.ParseFilteringMode(texUp.FilteringMode));
                charTex.ScaleAdjust = texUp.ScaleAdjust;

                SlideElement el = AddElement(charTex, charInfo.X, charInfo.SlideLeft ? 1 : -1,
                    charInfo.Depth, autoAdd: false);
                el.Name = charInfo.Student;
                loadManager.Register(el, addForSlide);
            }
        }

        protected virtual void AddExtras(IResourcePackSource pack)
        {
            foreach (MenuEntryInfo.SlideSprite effInfo in info.Extras)
            {
                MenuEntryInfo.TextureUploadInfo texUp = effInfo.TextureUpload;
                Texture fxTex = pack.GetTexture($"MainMenu/{effInfo.Texture}", MenuEntryInfo.WrapModeInfo.Parse(texUp.WrapMode.WrapHorizontal),
                    MenuEntryInfo.WrapModeInfo.Parse(texUp.WrapMode.WrapVertical), texUp.UseAtlas, texUp.ManualMipmaps, MenuEntryInfo.TextureUploadInfo.ParseFilteringMode(texUp.FilteringMode));
                fxTex.ScaleAdjust = texUp.ScaleAdjust;

                SlideElement el = AddElement(fxTex, effInfo.X, effInfo.SlideMult,
                    effInfo.Depth, startY: effInfo.Y, blending: MenuEntryInfo.CustomBlendingInfo.ParseBlending(effInfo.Blending), autoAdd: false);
                el.Name = effInfo.Id;
                el.Origin = el.Anchor = MenuEntryInfo.TomlSprite.ParseAnchor(effInfo.Anchor);
                loadManager.Register(el, addForSlide);
            }
        }

        protected virtual void AddLabel(IResourcePackSource pack)
        {
            Texture fgTex = pack.GetTexture($"MainMenu/{info.Label.ForegroundTexture}");
            Texture shadowTex = pack.GetTexture($"MainMenu/{info.Label.ShadowTexture}");
            fgTex.ScaleAdjust = shadowTex.ScaleAdjust = 1;

            // if requested i will make the start Y a variable in the info metadata, for now its gonna stay like this
            Anchor labelAnchor = MenuEntryInfo.TomlSprite.ParseAnchor(info.Label.Anchor);
            SlideElement fgEl = AddElement(fgTex, 0, 0, info.Label.Depths[0], startY: 30, autoAdd: false);
            fgEl.Anchor = fgEl.Origin = labelAnchor;
            fgEl.Name = $"Foreground Shadow z{-fgEl.Depth}";
            loadManager.Register(fgEl, addForSlide);

            for (int i = 1; i <= info.Label.Shadows; i++)
            {
                const double base_delay = 50D;
                double endDelay = base_delay + (base_delay * i);

                SlideElement shadow = AddElement(shadowTex, fgEl.X + info.Label.ShadowOffset, 0, info.Label.Depths[i],
                    startY: fgEl.Y + info.Label.ShadowOffset, delay: endDelay, autoAdd: false);
                shadow.Anchor = shadow.Origin = labelAnchor;
                shadow.Name = $"Background Shadow z{-fgEl.Depth}";
                loadManager.Register(shadow, addForSlide);
            }
        }

        protected override void FadeInElements(float factor) => FadeInElements(factor, slideElements);

        protected override void FadeOutElements(float factor) => FadeOutElements(factor, slideElements);

        public override void Show()
        {
            Alpha = 1;
            foreach (SlideElement element in slideElements)
                element.Alpha = 1;
        }

        public override void Hide()
        {
            foreach (SlideElement element in slideElements)
                element.Alpha = 0;
            Alpha = 0;
        }

        private void addForSlide(SlideElement element)
        {
            Add(element);
            slideElements.Add(element);
        }
    }
}
