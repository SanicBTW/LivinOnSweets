using LivinOnSweets.API.Components;
using LivinOnSweets.API.Enums;
using LivinOnSweets.API.Extensions;
using LivinOnSweets.API.Interfaces;
using LivinOnSweets.API.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace LivinOnSweets.Game.Tests.Visual
{
    // Some of the content is inherited partially from TestSceneBannerAccent
    public partial class TestSceneAccentComponent : LivinOnSweetsTestScene
    {
        [Resolved]
        private AccentComponent accentComponent { get; set; }

        private StudentBanner banner;

        public TestSceneAccentComponent()
        {
            Add(new TestAccentBox());

            AddStep("set natsu", () => reloadBanner(Students.NATSU));
            AddStep("set kazusa", () => reloadBanner(Students.KAZUSA));
            AddStep("set airi", () => reloadBanner(Students.AIRI));
            AddStep("set yoshimi", () => reloadBanner(Students.YOSHIMI));
            AddStep("get accent colors (constant)", getAccentColorsConstant);
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
                Origin = Anchor.Centre,
                Margin = new MarginPadding() { Right = 150 }
            });
        }

        private void getAccentColorsConstant()
        {
            accentComponent.Populate(banner, AccentBannerSide.Left);
        }

        private partial class TestAccentBox : CompositeDrawable, IAccentColorReceiver
        {
            [Resolved]
            private AccentComponent accentComponent { get; set;}

            private Box box;
            private BindableColour4 primaryAccent;

            public TestAccentBox()
            {
                Size = new Vector2(60);
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Margin = new MarginPadding() { Left = 150 };
                InternalChild = box = new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.White,
                };
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                accentComponent.PropagateInto(this);
            }

            AccentBannerSide IAccentColorReceiver.AccentSide => AccentBannerSide.Left;

            void IAccentColorReceiver.AccentsUpdated(double duration, Easing easing)
            {
                BindableColour4 newValue = accentComponent.GetAccent(this, AccentColorRole.Primary);
                this.TransformBindableTo(primaryAccent, newValue.Value, duration, easing);
            }

            void IAccentColorReceiver.PropagateAccents(BindableColour4[] colors)
            {
                primaryAccent = colors[0];
                primaryAccent.BindValueChanged((ev) => box.Colour = ev.NewValue, true);
            }
        }
    }
}
