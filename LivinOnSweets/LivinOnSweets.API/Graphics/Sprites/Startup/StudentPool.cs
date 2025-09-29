using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Enums;
using LivinOnSweets.API.Skinning;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osuTK;

namespace LivinOnSweets.API.Graphics.Sprites.Startup
{
    // a little bit overworked actually but it works good lolz!
    // this used to hold the banners itself but thanks to scaling and positioning and shi i moved to just let it manage the pool itself and target a container to add the banners to
    /// <summary>
    /// A <see cref="ResourcePackReloadableDrawable"/> with a pooling fashion to automatically position and retrieve the needed <see cref="StudentBanner"/>'s.
    /// </summary>
    public partial class StudentPool : ResourcePackReloadableDrawable
    {
        [Resolved] private SweetConfigManager sweetConfig { get; set; }

        private Bindable<GameUpdateVersion> gameVersion = new();

        private FillFlowContainer targetContainer;
        private FillFlowContainer extraVerticalFlow; // holds the banners of >= antique seraphim vertically
        private readonly BindableMarginPadding verticalFlowMargin = new(new MarginPadding() { Left = -138 });
        private DrawablePool<StudentBanner> studPool;

        public int CountAvailable => studPool.CountAvailable;
        public int CountInUse => studPool.CountInUse;
        public int CountExcessConstructed => studPool.CountExcessConstructed;

        public bool IsHidden { get; private set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            sweetConfig.BindWith(SweetSetting.GameUpdate, gameVersion);

            // 2 banners for < antique seraphim
            // 3 banners for >= antique seraphim
            // we allocating x banners based on the game version for good measure but it will always be maxed at 5
            InternalChild = studPool = new DrawablePool<StudentBanner>(0, 5);
            extraVerticalFlow = new FillFlowContainer()
            {
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,

                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, -118),
                Depth = 1,
            };

            verticalFlowMargin.BindValueChanged((ev) => extraVerticalFlow.Margin = ev.NewValue, true);
        }

        // refresh the pool when changing packs so the banners have an immediate effect (i believe?)
        protected override void PackChanged(IResourcePackSource newPack) => refreshPool();

        private void refreshPool()
        {
            int gameVer = (int)gameVersion.Value;

            if (CountInUse > CountAvailable) // probably like 2 out of 0 or 2 out of 1 or 3 out of 0
            {
                // clean up the possible sprites by returning to the pool then populating back
                cleanBanners(targetContainer);

                if (extraVerticalFlow.IsAlive)
                    cleanBanners(extraVerticalFlow);
            }
            else
            {
                // we are not setting the initial pool size through the argument since it will load natsu by default and maybe we dont want that
                int initPoolSize = gameVer >= (int)GameUpdateVersion.AntiqueSeraphim ? 3 : 2;

                // i should make an option to allow the customization of the banners, for now we going og and stick with kazusa and airi
                int startingIndex = gameVer >= (int)GameUpdateVersion.AntiqueSeraphim ? 4 : 1;
                for (int i = 0; i < initPoolSize; i++)
                {
                    int studentIndex = startingIndex + i;
                    studPool.Get((stud) => stud.Student = (Students)studentIndex).Return();
                }
            }

            populateLayout();
        }

        private void populateLayout()
        {
            int gameVer = (int)gameVersion.Value;

            // must have 3 banners brototype, maybe a little forced but its the best i can think of right now
            if (gameVer >= (int)GameUpdateVersion.AntiqueSeraphim)
            {
                // the is left flag is used for the randomness, not for positioning lol!!
                // we setting the default value to have something to revert to easily
                StudentBanner leftBanner = getBanner(Students.Sakurako, true);
                leftBanner.BindableMargin.Default = new MarginPadding { Right = -105, Top = 55 };
                leftBanner.BindableMargin.SetDefault();

                StudentBanner topRightBanner = getBanner(Students.Mari);

                StudentBanner btmRightBanner = getBanner(Students.Mine);
                btmRightBanner.Depth = 2;

                Schedule(() =>
                {
                    targetContainer.Insert(-1, leftBanner);
                    extraVerticalFlow.AddRange([topRightBanner, btmRightBanner]);

                    if (!targetContainer.Contains(extraVerticalFlow))
                        targetContainer.Add(extraVerticalFlow);
                });
            }
            else
            {
                // back to the negative margin magic...
                StudentBanner leftBanner = getBanner(Students.Kazusa, true);
                leftBanner.BindableMargin.Default = new MarginPadding()
                {
                    Right = -150,
                    Bottom = 10,
                };
                leftBanner.BindableMargin.SetDefault();

                StudentBanner rightBanner = getBanner(Students.Airi);
                rightBanner.BindableMargin.Default = new MarginPadding()
                {
                    Left = -118,
                    Bottom = 20,
                };
                rightBanner.BindableMargin.SetDefault();

                Schedule(() =>
                {
                    if (targetContainer.Contains(extraVerticalFlow))
                        targetContainer.Remove(extraVerticalFlow, false);

                    targetContainer.Insert(-1, leftBanner);
                    targetContainer.Add(rightBanner);
                });
            }
        }

        public override void Show()
        {
            IEnumerable<StudentBanner> banners = targetContainer.OfType<StudentBanner>().ToArray();

            if (extraVerticalFlow.IsAlive)
            {
                extraVerticalFlow.FadeTo(1, 800)
                    .TransformBindableTo(verticalFlowMargin, verticalFlowMargin.Default, 1000, Easing.OutQuint);
            }

            foreach (StudentBanner banner in banners)
            {
                banner.FadeTo(1, 800)
                    .TransformBindableTo(banner.BindableMargin, banner.BindableMargin.Default, 1000, Easing.OutQuint);
            }

            IsHidden = false;
        }

        public override void Hide()
        {
            StudentBanner[] banners = targetContainer.OfType<StudentBanner>().ToArray();
            if (banners.Length == 0)
            {
                ScheduleAfterChildren(Hide); // schedule after children since it will probably have everything loaded
                return;
            }

            // this will only move the margin of the whole container cuz we funny like that
            if (targetContainer.Contains(extraVerticalFlow))
            {
                extraVerticalFlow.FadeTo(0, 800)
                    .TransformBindableTo(verticalFlowMargin, new MarginPadding() { Left = -extraVerticalFlow.DrawWidth }, 1000, Easing.OutQuint);
            }

            foreach (StudentBanner banner in banners)
            {
                MarginPadding outOfBounds = banner.Margin;
                outOfBounds.Left = banner.IsLeft ? 0 : -banner.DrawWidth;
                outOfBounds.Right = banner.IsLeft ? -banner.DrawWidth : 0;

                banner.FadeTo(0, 800)
                    .TransformBindableTo(banner.BindableMargin, outOfBounds, 1000, Easing.OutQuint);
            }

            IsHidden = true;
        }

        public void FadeBannersTo(float alpha, double duration = 0D, Easing easing = Easing.None)
        {
            StudentBanner[] banners = targetContainer.OfType<StudentBanner>().ToArray();

            if (targetContainer.Contains(extraVerticalFlow))
                extraVerticalFlow.FadeTo(alpha, duration, easing);

            foreach (StudentBanner banner in banners)
                banner.FadeTo(alpha, duration, easing);
        }

        public void SetTargetContainer(FillFlowContainer target) => targetContainer = target;

        private StudentBanner getBanner(Students student, bool isLeft = false)
        {
            return studPool.Get((stud) =>
            {
                stud.Student = student;
                stud.IsLeft = isLeft;
                stud.Anchor = stud.Origin = Anchor.Centre;
                stud.Depth = 1;

                // reset both margins to avoid reusing old values
                stud.BindableMargin.Value = new MarginPadding(0);
                stud.BindableMargin.Default = new MarginPadding(0);

                // if the student is below 4 (sakurako) then it means we aint running antique seraphim
                // this is an alternative to retrieving the game update or type shi
                float scale = student < Students.Sakurako ? 1 : 1.15F;
                stud.Scale = new Vector2(scale);
            });
        }

        private static void cleanBanners(FillFlowContainer target)
        {
            IEnumerable<StudentBanner> banners = target.OfType<StudentBanner>().ToArray();
            foreach (StudentBanner banner in banners)
                target.Remove(banner, false);
        }
    }
}
