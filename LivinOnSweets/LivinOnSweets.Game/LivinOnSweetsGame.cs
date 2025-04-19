using LivinOnSweets.API.Components;
using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Graphics;
using LivinOnSweets.API.Graphics.UserInterface;
using LivinOnSweets.API.Screens;
using LivinOnSweets.Game.Screens;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osuTK;

namespace LivinOnSweets.Game
{
    public partial class LivinOnSweetsGame : LivinOnSweetsGameBase
    {
        private Bindable<bool> applySafeAreaConsiderations;
        private Bindable<float> uiScale;

        /// <summary>
        /// Adjust the globally applied <see cref="DrawSizePreservingFillContainer.TargetDrawSize"/> in every <see cref="ScalingContainer"/>.
        /// Useful for changing how the game handles different aspect ratios.
        /// </summary>
        public virtual Bindable<Vector2> ScalingContainerTargetDrawSize { get; } = new(new Vector2(1280, 720));

        protected SweetScreenStack ScreenStack { get; set; }
        protected ScalingContainer ScreenContainer { get; private set; }
        [Cached] protected GameOverlaysContainer OverlaysContainer { get; private set; } = new();

        [BackgroundDependencyLoader]
        private void load()
        {
            uiScale = SweetConfig.GetBindable<float>(SweetSetting.UserInterfaceScale);

            applySafeAreaConsiderations = SweetConfig.GetBindable<bool>(SweetSetting.SafeAreaConsiderations);
            applySafeAreaConsiderations.BindValueChanged(apply => SafeAreaContainer.SafeAreaOverrideEdges = apply.NewValue ? SafeAreaOverrideEdges : Edges.All, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // https://github.com/ppy/osu/blob/master/osu.Game/OsuGame.cs#L905
            LoadLocales();

            AddRange([
                new GlobalManiaActionReceiver(),
                ScreenContainer = new ScalingContainer(ScalingMode.ExcludeOverlays, ScalingContainerTargetDrawSize)
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = ScreenStack = new SweetScreenStack { RelativeSizeAxes = Axes.Both, },
                },
                OverlaysContainer
            ]);

            // Can't access STL (Single Thread Load) through DPI, so we access the protected variable from inheritance,
            // for descendants, it should be available already
            SingleThreadLoad.ScheduleLoad(new FpsCounter(), d => OverlaysContainer.AddOverlay(OverlayContainerTarget.TopMost, d));
            SingleThreadLoad.ScheduleLoad(new ScreenshotManager(), d => OverlaysContainer.AddOverlay(OverlayContainerTarget.TopMost, d));

            ScreenStack.Push(new PreloadingScreen());
        }

        protected override Container CreateScalingContainer() =>
            new ScalingContainer(ScalingMode.Everything, ScalingContainerTargetDrawSize);

        public override bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            const float adjustment_increment = 0.05f;

            switch (e.Action)
            {
                case PlatformAction.ZoomIn:
                    uiScale.Value += adjustment_increment;
                    return true;

                case PlatformAction.ZoomOut:
                    uiScale.Value -= adjustment_increment;
                    return true;

                case PlatformAction.ZoomDefault:
                    uiScale.SetDefault();
                    return true;
            }

            return base.OnPressed(e);
        }
    }
}
