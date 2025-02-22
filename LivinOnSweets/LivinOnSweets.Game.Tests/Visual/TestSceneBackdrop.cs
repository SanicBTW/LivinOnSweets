using LivinOnSweets.API.Sprites;
using LivinOnSweets.API.Stores;
using NUnit.Framework;
using osu.Framework.Allocation;

namespace LivinOnSweets.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneBackdrop : LivinOnSweetsTestScene
    {
        [BackgroundDependencyLoader]
        private void load(MainMenuStore mmStore)
        {
            Backdrop backdrop = new Backdrop()
            {
                Texture = mmStore.Get("MainMenu/Patterns/Wireframe.png")
            };
            Add(backdrop);

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
        }
    }
}
