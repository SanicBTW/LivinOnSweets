using System;
using System.Collections.Generic;
using System.Linq;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Graphics;
using LivinOnSweets.API.Graphics.UserInterface;
using LivinOnSweets.API.Graphics.Containers;
using LivinOnSweets.API.Overlays;
using LivinOnSweets.API.Screens;
using LivinOnSweets.Game.Screens;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;

namespace LivinOnSweets.Game
{
    public partial class LivinOnSweetsGame : LivinOnSweetsGameBase, IOverlayManager
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

        /// <summary>
        /// Whether overlays should be able to be opened game-wide. Value is sourced from the current active screen.
        /// </summary>
        public readonly IBindable<OverlayActivation> OverlayActivationMode = new Bindable<OverlayActivation>();

        private readonly List<SweetFocusedOverlayContainer> focusedOverlays = [];
        private readonly List<OverlayContainer> externalOverlays = [];
        private readonly List<OverlayContainer> visibleBlockingOverlays = [];

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

            OverlayActivationMode.ValueChanged += mode =>
            {
                if (mode.NewValue != OverlayActivation.All) CloseAllOverlays();
            };

            ScreenStack.Push(new PreloadingScreen());
        }

        protected override Container CreateScalingContainer() =>
            new ScalingContainer(ScalingMode.Everything, ScalingContainerTargetDrawSize);

        #region IOverlayManager

        // TODO: Should be bound to the screen pushed
        IBindable<OverlayActivation> IOverlayManager.OverlayActivationMode => OverlayActivationMode;

        private void updateBlockingOverlayFade() =>
            ScreenContainer.FadeColour(visibleBlockingOverlays.Any() ? Color4.Black : Color4.White, 500, Easing.OutQuint);

        IDisposable IOverlayManager.RegisterBlockingOverlay(OverlayContainer overlayContainer)
        {
            if (overlayContainer.Parent != null)
                throw new ArgumentException($@"Overlays registered via {nameof(IOverlayManager.RegisterBlockingOverlay)} should not be added to the scene graph.");

            if (externalOverlays.Contains(overlayContainer))
                throw new ArgumentException($@"{overlayContainer} has already been registered via {nameof(IOverlayManager.RegisterBlockingOverlay)} once.");

            externalOverlays.Add(overlayContainer);
            OverlaysContainer.AddOverlay(OverlayContainerTarget.Default, overlayContainer);

            if (overlayContainer is SweetFocusedOverlayContainer focusedOverlayContainer)
                focusedOverlays.Add(focusedOverlayContainer);

            return new InvokeOnDisposal(() => unregisterBlockingOverlay(overlayContainer));
        }

        void IOverlayManager.ShowBlockingOverlay(OverlayContainer overlay)
        {
            if (!visibleBlockingOverlays.Contains(overlay))
                visibleBlockingOverlays.Add(overlay);
            updateBlockingOverlayFade();
        }

        void IOverlayManager.HideBlockingOverlay(OverlayContainer overlay) => Schedule(() =>
        {
            visibleBlockingOverlays.Remove(overlay);
            updateBlockingOverlayFade();
        });

        /// <summary>
        /// Unregisters a blocking <see cref="OverlayContainer"/> that was not created by <see cref="LivinOnSweetsGame"/> itself.
        /// </summary>
        private void unregisterBlockingOverlay(OverlayContainer overlayContainer) => Schedule(() =>
        {
            externalOverlays.Remove(overlayContainer);

            if (overlayContainer is SweetFocusedOverlayContainer focusedOverlayContainer)
                focusedOverlays.Remove(focusedOverlayContainer);

            overlayContainer.Expire();
        });

        /// <summary>
        /// Close all game-wide overlays.
        /// </summary>
        public void CloseAllOverlays()
        {
            foreach (var overlay in focusedOverlays)
                overlay.Hide();
        }

        #endregion

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
