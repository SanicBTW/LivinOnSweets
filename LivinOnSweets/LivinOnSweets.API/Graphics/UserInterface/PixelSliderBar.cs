using System.Numerics;
using LivinOnSweets.API.Skinning;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;

namespace LivinOnSweets.API.Graphics.UserInterface
{
    public partial class PixelSliderBar<T> : SliderBar<T>
        where T : struct, INumber<T>, IMinMaxValue<T>
    {
        private Sprite handle;

        [BackgroundDependencyLoader]
        private void load(IResourcePackSource pack)
        {
            AutoSizeAxes = Axes.Both;

            Texture barTex = pack.GetTexture("OptionsModal/SlideBar", WrapMode.None, WrapMode.None, false, false, TextureFilteringMode.Nearest);
            Texture handleTex = pack.GetTexture("OptionsModal/SlideHandle", WrapMode.None, WrapMode.None, false, false, TextureFilteringMode.Nearest);
            barTex.ScaleAdjust = handleTex.ScaleAdjust = 1;

            Children =
            [
                new Sprite()
                {
                    Texture = barTex,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                handle = new Sprite()
                {
                    Texture = handleTex,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.X,
                }
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateFocus();
        }

        protected override void OnFocus(FocusEvent e)
        {
            updateFocus();
            base.OnFocus(e);
        }

        protected override void OnFocusLost(FocusLostEvent e)
        {
            updateFocus();
            base.OnFocusLost(e);
        }

        private void updateFocus()
        {
            handle.FadeTo(HasFocus ? 1F : 0.75F, 300D, Easing.OutQuint);
        }

        protected override void UpdateValue(float value)
        {
            handle.MoveToX(value, 300D, Easing.OutQuint);
        }
    }
}
