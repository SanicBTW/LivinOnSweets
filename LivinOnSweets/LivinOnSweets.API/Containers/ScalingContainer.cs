using LivinOnSweets.API.Configuration;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input;
using osu.Framework.Layout;
using osu.Framework.Platform;
using osuTK;

namespace LivinOnSweets.API.Containers
{
    // Ported over from https://github.com/ppy/osu/blob/master/osu.Game/Graphics/Containers/ScalingContainer.cs
    public partial class ScalingContainer : Container
    {
        internal const float TRANSITION_DURATION = 500;
        private const float corner_radius = 10;

        protected override Container<Drawable> Content => content;
        private readonly Container content;

        private Bindable<float> sizeX = null!;
        private Bindable<float> sizeY = null!;
        private Bindable<float> posX = null!;
        private Bindable<float> posY = null!;

        private Bindable<bool> applySafeAreaPadding = null!;
        private Bindable<MarginPadding> safeAreaPadding = null!;

        private readonly ScalingMode? targetMode;
        private Bindable<ScalingMode> scalingMode = null!;

        private readonly SizeableAlwaysInputContainer sizableContainer;

        private RectangleF? customRect;
        private bool customRectIsRelativePosition;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        /// <summary>
        /// Set a custom position and scale which overrides any user specification.
        /// </summary>
        /// <param name="rect">A rectangle with positional and sizing information for this container to conform to. <c>null</c> will clear the custom rect and revert to user settings.</param>
        /// <param name="relativePosition">Whether the position portion of the provided rect is in relative coordinate space or not.</param>
        public void SetCustomRect(RectangleF? rect, bool relativePosition = false)
        {
            customRect = rect;
            customRectIsRelativePosition = relativePosition;

            if (IsLoaded) Scheduler.AddOnce(updateSize);
        }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="targetMode">The mode which this container should be handling. Handles all modes if null.</param>
        /// <param name="targetDrawSize">The bindable which will define the <see cref="DrawSizePreservingFillContainer.TargetDrawSize"/> used internally.</param>
        public ScalingContainer(ScalingMode? targetMode = null, Bindable<Vector2> targetDrawSize = null)
        {
            this.targetMode = targetMode;
            RelativeSizeAxes = Axes.Both;

            InternalChild = sizableContainer = new SizeableAlwaysInputContainer(targetMode == ScalingMode.Everything)
            {
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.Both,
                CornerRadius = corner_radius,
                Child = content = new ScalingDrawSizePreservingFillContainer(targetMode != ScalingMode.Gameplay, targetDrawSize)
            };
        }

        [BackgroundDependencyLoader]
        private void load(SweetConfigManager config, ISafeArea safeArea)
        {
            scalingMode = config.GetBindable<ScalingMode>(SweetSetting.Scaling);
            scalingMode.ValueChanged += _ => Scheduler.AddOnce(updateSize);

            sizeX = config.GetBindable<float>(SweetSetting.ScalingSizeX);
            sizeX.ValueChanged += _ => Scheduler.AddOnce(updateSize);

            sizeY = config.GetBindable<float>(SweetSetting.ScalingSizeY);
            sizeY.ValueChanged += _ => Scheduler.AddOnce(updateSize);

            posX = config.GetBindable<float>(SweetSetting.ScalingPositionX);
            posX.ValueChanged += _ => Scheduler.AddOnce(updateSize);

            posY = config.GetBindable<float>(SweetSetting.ScalingPositionY);
            posY.ValueChanged += _ => Scheduler.AddOnce(updateSize);

            applySafeAreaPadding = config.GetBindable<bool>(SweetSetting.SafeAreaConsiderations);
            applySafeAreaPadding.BindValueChanged(_ => Scheduler.AddOnce(updateSize));

            safeAreaPadding = safeArea.SafeAreaPadding.GetBoundCopy();
            safeAreaPadding.BindValueChanged(_ => Scheduler.AddOnce(updateSize));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateSize();
            sizableContainer.FinishTransforms();
        }

        // Removed the everything check, since we are not using the background stuff (maybe we should??)
        private void updateSize()
        {
            RectangleF targetRect = new RectangleF(Vector2.Zero, Vector2.One);

            if (customRect != null)
            {
                sizableContainer.RelativePositionAxes = customRectIsRelativePosition ? Axes.Both : Axes.None;

                targetRect = customRect.Value;
            }
            else if (targetMode == null || scalingMode.Value == targetMode)
            {
                sizableContainer.RelativePositionAxes = Axes.Both;

                Vector2 scale = new Vector2(sizeX.Value, sizeY.Value);
                Vector2 pos = new Vector2(posX.Value, posY.Value) * (Vector2.One - scale);

                targetRect = new RectangleF(pos, scale);
            }

            bool requiresMasking = targetRect.Size != Vector2.One
                                   // For the top level scaling container, for now we apply masking if safe areas are in use.
                                   // In the future this can likely be removed as more of the actual UI supports overflowing into the safe areas.
                                    || (targetMode == ScalingMode.Everything && (applySafeAreaPadding.Value && safeAreaPadding.Value.Total != Vector2.Zero));

            if (requiresMasking)
                sizableContainer.Masking = true;

            sizableContainer.MoveTo(targetRect.Location, TRANSITION_DURATION, Easing.OutQuart);
            sizableContainer.ResizeTo(targetRect.Size, TRANSITION_DURATION, Easing.OutQuart);

            // Of note, this will not work great in the case of nested ScalingContainers where multiple are applying corner radius.
            // Masking and corner radius should likely only be applied at one point in the full game stack to fix this.
            // An example of how this can occur is when the skin editor is visible and the game screen scaling is set to "Everything".
            sizableContainer.TransformTo(nameof(CornerRadius), requiresMasking ? corner_radius : 0, TRANSITION_DURATION, requiresMasking ? Easing.OutQuart : Easing.None)
                            .OnComplete(_ => { sizableContainer.Masking = requiresMasking; });
        }

        // https://github.com/ppy/osu/blob/master/osu.Game/Graphics/Containers/ScalingContainer.cs#L86
        public partial class ScalingDrawSizePreservingFillContainer : DrawSizePreservingFillContainer
        {
            private readonly bool applyUiScale;
            private Bindable<float> uiScale;

            protected float CurrentScale { get; private init; } = 1;

            public ScalingDrawSizePreservingFillContainer(bool applyUiScale, Bindable<Vector2> targetDrawSize = null)
            {
                this.applyUiScale = applyUiScale;
                targetDrawSize?.BindValueChanged(v => TargetDrawSize = v.NewValue, true);
            }

            [BackgroundDependencyLoader]
            private void load(SweetConfigManager sweetConf)
            {
                if (!applyUiScale) return;

                uiScale = sweetConf.GetBindable<float>(SweetSetting.UserInterfaceScale);
                uiScale.BindValueChanged(args => this.TransformTo(nameof(CurrentScale), args.NewValue, TRANSITION_DURATION, Easing.OutQuart), true);
            }

            protected override void Update()
            {
                Scale = new Vector2(CurrentScale);
                Size = new Vector2(1 / CurrentScale);

                base.Update();
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;
        }

        // Ported over from https://github.com/ppy/osu/blob/master/osu.Game/Graphics/Containers/ScalingContainer.cs#L237
        private partial class SizeableAlwaysInputContainer : Container
        {
            [Resolved]
            private GameHost host { get; set; }

            [Resolved]
            private ISafeArea safeArea { get; set; }

            [Resolved]
            private SweetConfigManager config { get; set; }

            private readonly bool confineHostCursor;
            private readonly LayoutValue cursorRectCache = new(Invalidation.RequiredParentSizeToFit);

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            /// <summary>
            /// Container used for sizing/positioning purposes in <see cref="ScalingContainer"/>. Always receives mouse input.
            /// </summary>
            /// <param name="confineHostCursor">Whether to confine the host cursor to the draw area of this container.</param>
            /// <remarks>Cursor confinement will abide by the <see cref="ConfineMouseMode"/> setting.</remarks>
            public SizeableAlwaysInputContainer(bool confineHostCursor)
            {
                RelativeSizeAxes = Axes.Both;
                this.confineHostCursor = confineHostCursor;

                if (confineHostCursor)
                    AddLayout(cursorRectCache);
            }

            protected override void Update()
            {
                base.Update();

                if (!confineHostCursor || cursorRectCache.IsValid) return;

                updateHostCursorConfineRect();
                cursorRectCache.Validate();
            }

            private void updateHostCursorConfineRect()
            {
                if (host.Window == null) return;

                bool coversWholeScreen = Size == Vector2.One && (!config.Get<bool>(SweetSetting.SafeAreaConsiderations) || safeArea.SafeAreaPadding.Value.Total == Vector2.Zero);
                host.Window.CursorConfineRect = coversWholeScreen ? null : ToScreenSpace(DrawRectangle).AABBFloat;
            }
        }
    }
}
