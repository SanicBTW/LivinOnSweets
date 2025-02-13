using LivinOnSweets.API.Extensions;
using LivinOnSweets.API.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osuTK;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LivinOnSweets.API.Overlays
{
    // This is a port of the screenshot feature I quickly made for the DebugContainer
    public partial class ScreenshotOverlay : Container, IKeyBindingHandler<ManiaAction>
    {
        [Resolved]
        private IRenderer renderer { get; set; }

        private Box flash;
        private ScreenshotSprite lastScreenshot;

        public ScreenshotOverlay()
        {
            // DebugContainer is on -99
            Depth = -98;
            RelativeSizeAxes = Axes.Both;

            InternalChildren =
            [
                flash = new Box()
                {
                    Alpha = 0,
                    Colour = Colour4.White,
                    RelativeSizeAxes = Axes.Both,
                    Depth = -1,
                }
            ];
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            if (e.Action != ManiaAction.SCREENSHOT || e.Repeat)
                return false;

            if (lastScreenshot != null)
            {
                lastScreenshot.FinishTransforms();
                disposeSprite(lastScreenshot);
                lastScreenshot = null;
            }

            takeScreenshot();

            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }

        private void takeScreenshot()
        {
            ScreenshotSprite ss = new ScreenshotSprite(DrawSize);
            lastScreenshot = ss;
            Add(ss);

            flash.FadeInFromZero().FadeOut(350);
            ss.ScaleTo(Vector2.One, 550D, Easing.OutQuint).Then().OnComplete(_ =>
            {
                ss.Delay(2000D).MoveToY(-ss.DrawHeight, 850D, Easing.OutQuint).OnComplete(disposeSprite);
            });
        }

        private void disposeSprite(Sprite spr)
        {
            spr.Texture?.Dispose();
            Remove(spr, true);
        }

        private partial class ScreenshotSprite : Sprite
        {
            [Resolved]
            private Clipboard clipboard { get; set; }

            [Resolved]
            private IRenderer renderer { get; set; }

            // Save a reference to the screenshot pixels so if the screenshot is clicked
            // it doesnt have to take another one, changing  the content of the copied img
            private Image<Rgba32> image;

            public ScreenshotSprite(Vector2 drawSize, float scaleFactor = 4)
            {
                Size = drawSize / scaleFactor;
                Scale = new Vector2(scaleFactor);

                Anchor = Anchor.TopRight;
                Origin = Anchor.TopRight;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                // Save up a copy of the image because if we use it, it will get disposed as soon as
                // it gets uploaded to the texture data, so we allocating twice now :grin:
                // hey i should just look at osu lazer code? wtf am i doing lmao
                image = renderer.TakeScreenshotToImage();

                Texture = renderer.TakeScreenshotToTexture();
            }

            protected override bool OnClick(ClickEvent e)
            {
                clipboard.SetImage(image);
                return true;
            }
        }
    }
}
