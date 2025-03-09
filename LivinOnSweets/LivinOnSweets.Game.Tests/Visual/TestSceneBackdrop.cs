using LivinOnSweets.API.Sprites;
using LivinOnSweets.API.Stores;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osuTK;

namespace LivinOnSweets.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneBackdrop : LivinOnSweetsTestScene
    {
        private Backdrop backdrop;

        [BackgroundDependencyLoader]
        private void load(MainMenuStore mmStore)
        {
            Add(backdrop = new Backdrop()
            {
                Texture = mmStore.Get("MainMenu/Patterns/Wireframe.png")
            });

            AddStep("start", backdrop.Start);
            AddStep("freeze", backdrop.Freeze);

            AddStep("green", () =>
            {
                backdrop.Texture = mmStore.Get("MainMenu/Patterns/Green.png");
            });

            AddStep("blue", () =>
            {
                backdrop.Texture = mmStore.Get("MainMenu/Patterns/Blue.png");
            });

            AddStep("purple", () =>
            {
                backdrop.Texture = mmStore.Get("MainMenu/Patterns/Purple.png");
            });

            AddSliderStep("scroll speed x", -1f, 1f, 1f, (f) => transformScrollSpeed(f, false));
            AddSliderStep("scroll speed y", -1f, 1f, 1f, (f) => transformScrollSpeed(f, true));
        }

        private void transformScrollSpeed(float f, bool isY)
        {
            Vector2 curSpeed = backdrop.ScrollSpeed.Value;
            if (isY)
                curSpeed.Y = f;
            else
                curSpeed.X = f;

            this.TransformBindableTo(backdrop.ScrollSpeed, curSpeed);
        }
    }
}
