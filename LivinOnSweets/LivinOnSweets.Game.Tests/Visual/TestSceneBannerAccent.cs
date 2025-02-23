using System;
using System.Threading;
using System.Threading.Tasks;
using LivinOnSweets.API.Enums;
using LivinOnSweets.API.Sprites;
using LivinOnSweets.API.Stores;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osuTK;

namespace LivinOnSweets.Game.Tests.Visual
{
    // TODO! Fix cancellations not cancelling the task (ironic)
    [TestFixture]
    public partial class TestSceneBannerAccent : LivinOnSweetsTestScene
    {
        [Resolved]
        private AccentStore accentStore { get; set; }

        private StudentBanner banner;
        private FillFlowContainer<Box> bannerAccents;
        private CancellationTokenSource cancellationTokenSource = new();
        private bool finished;

        public TestSceneBannerAccent()
        {
            Add(bannerAccents = new FillFlowContainer<Box>()
            {
                AutoSizeAxes = Axes.X,
                Height = 60,
                Spacing = new Vector2(4),
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                Margin = new MarginPadding()
                {
                    Bottom = 14
                }
            });

            for (int i = 0; i < 3; i++)
            {
                bannerAccents.Add(new Box()
                {
                    Size = new Vector2(60),
                });
            }

            AddStep("set natsu", () => reloadBanner(Students.NATSU));
            AddStep("set kazusa", () => reloadBanner(Students.KAZUSA));
            AddStep("set airi", () => reloadBanner(Students.AIRI));
            AddStep("set yoshimi", () => reloadBanner(Students.YOSHIMI));
            AddStep("get accent colors (constant)", getAccentColorsConstant);
            AddStep("get accent colors (random)", getAccentColorsRandom);
            AddStep("get single accent color (constant)", getAccentSingleConstant);
            AddStep("get single accent color (random)", getAccentSingleRandom);
        }

        private void reloadBanner(Students student)
        {
            // On first load, banner is null
            if (banner != null)
                Remove(banner, true);

            Add(banner = new StudentBanner(student)
            {
                Scale = new Vector2(1.2f),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }

        private void getAccentColorsConstant()
        {
            resetBoxes();
            resetToken();
            wrapBlockingCall(() => callStore(false, bannerAccents.Count));
        }

        private void getAccentColorsRandom()
        {
            resetBoxes();
            resetToken();
            wrapBlockingCall(() => callStore(true, bannerAccents.Count));
        }

        private void getAccentSingleConstant()
        {
            resetBoxes();
            resetToken();
            wrapBlockingCall(() => callStore());
        }

        private void getAccentSingleRandom()
        {
            resetBoxes();
            resetToken();
            wrapBlockingCall(() => callStore(true));
        }

        private void callStore(bool random = false, int amount = 1)
        {
            // It will most likely match the length of the boxes available
            Colour4[] colors = accentStore.GetDominantColors(banner.ImageName, amount, random);

            // Have to schedule the mutation of sprites inside the update thread, in this context, we are in a foreign thread
            Schedule(() =>
            {
                for (var i = 0; i < colors.Length; i++)
                {
                    bannerAccents[i].FadeColour(colors[i], 1000D, Easing.OutQuint);
                }
            });
        }

        private void resetToken()
        {
            if (finished)
                return;

            Logger.Log("Cancelled the token, refreshing");

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();

            cancellationTokenSource = new CancellationTokenSource();
        }

        private void wrapBlockingCall(Action task)
        {
            finished = false;
            Task.Run(() =>
            {
                task();
                finished = true;
            }, cancellationTokenSource.Token);
        }

        private void resetBoxes()
        {
            foreach (Box box in bannerAccents)
            {
                box.Colour = Colour4.White;
            }
        }
    }
}
