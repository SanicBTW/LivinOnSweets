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
using osuTK;

namespace LivinOnSweets.API.Overlays
{
    // This is a port of the screenshot feature I quickly made for the DebugContainer
    public partial class ScreenshotOverlay : Container, IKeyBindingHandler<ManiaAction>
    {
        [Resolved]
        private IRenderer renderer { get; set; }

        private Box flash;
        private Sprite lastScreenshot;

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
            Sprite ss = new Sprite()
            {
                // The size is 1280x720 / 4
                Size = new Vector2(320, 180),
                Texture = renderer.TakeScreenshotToTexture(),
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                // Scale it to 1280x720
                Scale = new Vector2(4)
            };

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
    }
}
