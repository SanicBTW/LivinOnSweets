using LivinOnSweets.API.Components;
using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Extensions;
using LivinOnSweets.API.Interfaces;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace LivinOnSweets.Editor.Components;

internal abstract partial class AccentReceiver : Component, IAccentColorReceiver
{
    [Resolved] protected AccentComponent AccentComponent { get; private set; }

    protected bool Bound { get; private set; }

    private AccentBannerSide accentSide;
    protected BindableColour4 Primary;
    protected BindableColour4 Secondary;
    protected BindableColour4 Tertiary;

    public AccentReceiver(AccentBannerSide side)
    {
        accentSide = side;
    }

    AccentBannerSide IAccentColorReceiver.AccentSide => accentSide;

    void IAccentColorReceiver.PropagateAccents(BindableColour4[] colors)
    {
        Primary = colors[0];
        Secondary = colors[1];
        Tertiary = colors[2];
    }

    void IAccentColorReceiver.AccentsUpdated(double duration, Easing easing)
    {
        if (!Bound)
        {
            BindAccents();
            Bound = true;
        }

        BindableColour4 newPrimary = AccentComponent.GetAccent(this, AccentColorRole.Primary);
        BindableColour4 newSecondary = AccentComponent.GetAccent(this, AccentColorRole.Secondary);
        BindableColour4 newTertiary = AccentComponent.GetAccent(this, AccentColorRole.Tertiary);

        this.TransformBindableTo(Primary, newPrimary.Value, duration, easing);
        this.TransformBindableTo(Secondary, newSecondary.Value, duration, easing);
        this.TransformBindableTo(Tertiary, newTertiary.Value, duration, easing);
    }

    private protected abstract void BindAccents();
}
