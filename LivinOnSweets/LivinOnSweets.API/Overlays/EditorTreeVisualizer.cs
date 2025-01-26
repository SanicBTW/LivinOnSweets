// https://github.com/ppy/osu-framework/blob/master/osu.Framework/Graphics/Visualisation/DrawVisualiser.cs

using System.Reflection;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Visualisation;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Lists;
using osu.Framework.Screens;
using osu.Framework.Utils;

namespace LivinOnSweets.API.Overlays
{

    internal partial class EditorTreeVisualizer : OverlayContainer, IRequireHighFrequencyMousePosition
    {
        private static readonly Dictionary<Type, bool> is_type_valid_target_cache = new();

        [Resolved]
        private ScreenStack gameScreenStack { get; set; }

        private readonly InfoOverlay overlay;

        // This controls where the search is gonna take place
        private Drawable searchTarget;
        public Drawable SearchTarget
        {
            get => searchTarget ?? gameScreenStack;
            internal set => searchTarget = value;
        }

        public bool Searching { get; private set; }

        internal event Action<Drawable> OnTargetChanged;
        private Drawable target;
        public Drawable Target
        {
            get => target;
            set
            {
                if (target != null)
                {
                    // Target Visualiser shi
                }

                target = value;
                OnTargetChanged?.Invoke(target);

                if (target != null)
                {
                    // Target Visualiser shi
                }
            }
        }

        private InputManager inputManager;
        private Drawable cursorTarget;
        protected override bool BlockPositionalInput => Searching;

        public EditorTreeVisualizer()
        {
            RelativeSizeAxes = Axes.Both;

            Children =
            [
                overlay = new InfoOverlay()
            ];
        }

        protected override void Update()
        {
            base.Update();

            if (Searching)
            {
                updateCursorTarget();
                overlay.Target = cursorTarget;
            }
            /*
            Drawable hoveredDrawable = inputManager.HoveredDrawables.FirstOrDefault();
            if (hoveredDrawable != null)
                infoOverlay.Target = hoveredDrawable;*/
        }


        private void updateCursorTarget()
        {
            Drawable drawableTarget = null;
            CompositeDrawable compositeTarget = null;
            Quad? maskingQuad = null;

            findTarget(SearchTarget);

            cursorTarget = drawableTarget ?? compositeTarget;

            void findTarget(Drawable drawable)
            {
                if (drawable.HasProxy)
                    return;

                while (drawable.IsProxy)
                    drawable = getOriginalDrawable(drawable);

                if (drawable == this || drawable is Component)
                    return;

                if (!drawable.IsPresent)
                    return;

                if (drawable.AlwaysPresent && Precision.AlmostEquals(drawable.Alpha, 0f))
                    return;

                if (drawable is CompositeDrawable composite)
                {
                    Quad? oldMaskingQuad = maskingQuad;

                    if (composite.Masking || composite is BufferedContainer)
                        maskingQuad = composite.ScreenSpaceDrawQuad;

                    IReadOnlyList<Drawable> internalAliveChildren = getInternalAliveChildren(composite);
                    for (int i = internalAliveChildren.Count - 1; i >= 0; i--)
                    {
                        findTarget(internalAliveChildren[i]);

                        if (drawableTarget != null)
                            return;
                    }

                    maskingQuad = oldMaskingQuad;

                    if (!validForTarget(composite))
                        return;

                    compositeTarget ??= composite;

                    if (!composite.Masking)
                        return;

                    if ((composite.BorderThickness > 0 && composite.BorderColour.MaxAlpha > 0)
                        || (composite.EdgeEffect.Type != EdgeEffectType.None && composite.EdgeEffect.Radius > 0 && composite.EdgeEffect.Colour.Alpha > 0))
                    {
                        drawableTarget = composite;
                    }
                }
                else
                {
                    if (!validForTarget(drawable))
                        return;

                    drawableTarget = drawable;
                }
            }

            bool validForTarget(Drawable drawable)
            {
                if (!drawable.ScreenSpaceDrawQuad.Contains(inputManager.CurrentState.Mouse.Position)
                    || maskingQuad?.Contains(inputManager.CurrentState.Mouse.Position) == false)
                {
                    return false;
                }

                Type type = drawable.GetType();

                if (is_type_valid_target_cache.TryGetValue(type, out bool valid))
                    return valid;

                valid = type.GetMethod(nameof(CreateDrawNode), BindingFlags.Instance | BindingFlags.NonPublic)?.DeclaringType != typeof(Drawable);

                valid &= !type.GetCustomAttributes<DrawVisualiserHiddenAttribute>(true).Any();

                return is_type_valid_target_cache[type] = valid;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
        }

        internal void StartSearching([CanBeNull] Drawable searchIn = null)
        {
            Show();
            Searching = true;
            SearchTarget = searchIn;
            Target = null;
        }

        protected override bool Handle(UIEvent e) => Searching;

        protected override bool OnClick(ClickEvent e)
        {
            if (Searching)
            {
                Target = cursorTarget?.Parent;

                if (Target != null)
                {
                    overlay.Target = null;

                    Searching = false;
                    return true;
                }
            }

            return base.OnClick(e);
        }

        protected override void PopIn()
        {
            this.FadeIn(100);
        }

        protected override void PopOut()
        {
            this.FadeOut(100);
        }

        // have to resort to reflection because "Original" is internal
        private Drawable getOriginalDrawable(Drawable proxy)
        {
            Drawable original;
            Type drawType = typeof(Drawable);

            PropertyInfo originalField = drawType.GetProperty("Original", BindingFlags.Instance | BindingFlags.NonPublic);
            if (originalField == null)
                return null;

            if (!originalField.CanRead)
                return null;

            MethodInfo originalGetMethod = originalField.GetMethod;
            original = (Drawable)originalGetMethod!.Invoke(proxy, []);

            return original;
        }

        // more reflection because that shi internal brah :broken_heart:
        private IReadOnlyList<Drawable> getInternalAliveChildren(Drawable source)
        {
            IReadOnlyList<Drawable> internalAliveChildren = new SortedList<Drawable>();
            Type drawType = typeof(CompositeDrawable);

            PropertyInfo iAChildren =
                drawType.GetProperty("AliveInternalChildren", BindingFlags.Instance | BindingFlags.NonPublic);
            if (iAChildren == null)
                return internalAliveChildren;

            if (!iAChildren.CanRead)
                return internalAliveChildren;

            MethodInfo getMethod = iAChildren.GetMethod;
            internalAliveChildren = (IReadOnlyList<Drawable>)getMethod!.Invoke(source, []);

            return internalAliveChildren;
        }
    }
}
