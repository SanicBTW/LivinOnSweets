using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.Interfaces;
using LivinOnSweets.API.Sprites;
using LivinOnSweets.API.Stores;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;

namespace LivinOnSweets.API.Components
{
    public partial class AccentComponent : Component
    {
        [Resolved]
        private AccentStore accentStore { get; set; }

        // Wont be available until the load was completed
#if DEBUG
        private Container targetContainer =>
            (Parent is Container p && p.Count > 2) ? p : ((Container)Parent!).Children.ElementAtOrDefault(1) as ManiaActionContainer ??
                                                            throw new InvalidOperationException("Unexpected hierarchy");
#else
        private ManiaActionContainer targetContainer => ((Container)Parent!).Children.ElementAtOrDefault(1) as ManiaActionContainer ??
                                                            throw new InvalidOperationException("Unexpected hierarchy");
#endif

        // Will be mapped by Left/Right, keyed dictionary will be mapped like Prim/Sec/Terti then it will return the color
        private Dictionary<AccentBannerSide, Dictionary<AccentColorRole, BindableColour4>> sideColors = [];

        // For synchronization when propagating
        private object objLock = new();

        // Fade Color Duration
        public double ColorChangeDuration = 1200D;

        // Fade Colour Easing
        public Easing ColorEasing = Easing.OutQuint;

        public void Populate(StudentBanner banner, AccentBannerSide side)
        {
            ensureSide(side, out Dictionary<AccentColorRole, BindableColour4> colors);

            // Run on a different thread
            Task.Run(() =>
            {
                Colour4[] accents = accentStore.GetDominantColors(banner.ImageName);
                for (int i = 0; i < accents.Length; i++)
                {
                    AccentColorRole role = (AccentColorRole)i;
                    Colour4 accent = accents[i];

                    if (colors.TryGetValue(role, out BindableColour4 boundColor))
                        boundColor.Value = accent;
                    else
                    {
                        boundColor = new BindableColour4(accent);
                        colors[role] = boundColor;
                    }
                }

                // Schedule the UI update
                if (IsLoaded)
                    Schedule(propagateChanges);
            });
        }

        private void propagateChanges()
        {
            if (Parent == null)
                return;

            lock (objLock)
            {
                foreach (IAccentColorReceiver receiver in targetContainer.ChildrenOfType<IAccentColorReceiver>())
                    PropagateInto(receiver, false);
            }
        }

        // Run this when loading OR finished loading an object which has the interface
        // The auto propagation only happens to those objects that are currently in the update tree
        // Those objects that aren't present on the tree at the moment of propagation will have to
        // manually trigger the propagation into the container
        public void PropagateInto(IAccentColorReceiver receiver, bool manual = true)
        {
            if (!manual)
            {
                Schedule(() => receiver.AccentsUpdated(ColorChangeDuration, ColorEasing));
                return;
            }

            ensureSide(receiver.AccentSide, out Dictionary<AccentColorRole, BindableColour4> bannerColors);

            BindableColour4[] colors = [
                GetBindable(bannerColors, AccentColorRole.Primary),
                GetBindable(bannerColors, AccentColorRole.Secondary),
                GetBindable(bannerColors, AccentColorRole.Tertiary)
            ];

            Schedule(() => receiver.PropagateAccents([..colors.Select(c => (BindableColour4)c.GetUnboundCopy())]));
        }

        public BindableColour4 GetBindable(AccentBannerSide side, AccentColorRole role)
        {
            ensureSide(side, out Dictionary<AccentColorRole, BindableColour4> bannerColors);
            ensureRole(bannerColors, role, out BindableColour4 accent);
            return accent;
        }

        public BindableColour4 GetBindable(Dictionary<AccentColorRole, BindableColour4> source, AccentColorRole role)
        {
            ensureRole(source, role, out BindableColour4 accent);
            return accent;
        }

        private void ensureSide(AccentBannerSide side, out Dictionary<AccentColorRole, BindableColour4> bannerColors)
        {
            if (!sideColors.TryGetValue(side, out bannerColors))
            {
                bannerColors = new Dictionary<AccentColorRole, BindableColour4>();
                sideColors[side] = bannerColors;
            }
        }

        private void ensureRole(Dictionary<AccentColorRole, BindableColour4> source, AccentColorRole role, out BindableColour4 accent)
        {
            if (!source.TryGetValue(role, out accent))
            {
                accent = new BindableColour4();
                source[role] = accent;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            propagateChanges();
        }
    }
}
