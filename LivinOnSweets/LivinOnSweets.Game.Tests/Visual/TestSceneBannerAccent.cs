using System;
using System.Threading;
using System.Threading.Tasks;
using LivinOnSweets.API.Enum;
using LivinOnSweets.API.StartupObjects;
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
        private bool finished = false;

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
            resetToken();
            wrapBlockingCall(() => callStore());
        }

        private void getAccentColorsRandom()
        {
            resetToken();
            wrapBlockingCall(() => callStore(true));
        }

        private void callStore(bool random = false)
        {
            // It will most likely match the length of the boxes available
            Colour4[] colors = accentStore.GetDominantColors(banner.ImageName, bannerAccents.Count, random);

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
    }
}
