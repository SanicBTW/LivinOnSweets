using AetherFramework;
using AetherFramework.Data;
using AetherFramework.Interfaces;
using LivinOnSweets.Editor.Containers;
using LivinOnSweets.Editor.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace LivinOnSweets.Editor.Entries;

internal partial class ModListWindow(bool listEnabledMods, Action closeAction) : VisibilityContainer
{
    [Resolved] private ModLoader modLoader { get; set; }

    protected override bool StartHidden => false;

    private EditorWindow window;

    [BackgroundDependencyLoader]
    private void load()
    {
        AutoSizeAxes = Axes.Both;

        ClickableContainer exitButton = new ClickableContainer()
        {
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
            Depth = -1,
            AutoSizeAxes = Axes.Both,
            Action = closeAction,
            Child = new SpriteIcon()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Icon = FontAwesome.Solid.WindowClose,
                Size = new Vector2(24),
            }
        };

        EditorWindowOption option = new EditorWindowOption("", "refresh list", refreshList);
        LoadComponentAsync(window = new EditorWindow("",
            $"{(listEnabledMods ? "enabled" : "disabled")} mods", option, this), Add);

        Add(exitButton);
    }

    protected override void LoadComplete()
    {
        State.Value = Visibility.Visible;
        window.ScrollContent.Spacing = new Vector2(0, 6);
        window.State.BindTo(State);

        base.LoadComplete();
        refreshList();
    }

    private void refreshList()
    {
        IEnumerable<IMod> targetMods = listEnabledMods ? modLoader.EnabledMods : modLoader.DisabledMods;
        foreach (IMod mod in targetMods)
            window.ScrollContent.Add(new ModCard(mod));
    }

    protected override void PopIn()
        => this.FadeIn(100D);

    protected override void PopOut()
        => this.FadeOut(100D);

    private partial class ModCard(IMod targetMod) : Card
    {
        private static readonly FontUsage default_font = new(family: "GyeonggiTitle", size: 16F);

        [BackgroundDependencyLoader]
        private void load()
        {
            Content.RelativeSizeAxes = Axes.X;
            Content.AutoSizeAxes = AutoSizeAxes = Axes.Y;

            SpriteText intentsText = createText("[none]").With(d => d.Font = d.Font.With(size: 10F));
            ModManifest manifest = targetMod.Manifest;

            const float vertical_margin = CARD_MARGIN * 1.5F;
            FillFlowContainer infoContainer = new FillFlowContainer()
            {
                Direction = FillDirection.Vertical,
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                X = CARD_MARGIN,
                Margin = new MarginPadding() { Left = CARD_MARGIN / 2, Top = vertical_margin, },
                Size = Size - new Vector2(CARD_MARGIN * 4, 0),
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(0, 4),
                Padding = new MarginPadding() { Bottom = vertical_margin },
                Children =
                [
                    createText(manifest.Name),
                    createText($"Made by {manifest.Author}").With(d => d.Font = d.Font.With(size: 14F)),
                    createText(manifest.Description).With(d => d.Font = d.Font.With(size: 10F)),
                    createSeparator(),
                    createText("intents").With(d => d.Font = d.Font.With(size: 14F)),
                    intentsText,
                    new SpriteText()
                    {
                        Font = default_font.With(size: 8F),
                        Alpha = 0.5F,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Text = $"v{manifest.Version}"
                    },
                ]
            };

            if (manifest.Intents.Count > 0)
                intentsText.Text = manifest.Intents.ToString();

            Add(infoContainer);
        }

        private SpriteText createText(string defaultText) => new()
        {
            Text = defaultText,
            RelativeSizeAxes = Axes.X,
            Font = default_font,
            MaxWidth = CARD_WIDTH,
        };

        private Box createSeparator() => new()
        {
            RelativeSizeAxes = Axes.X,
            Height = 2,
            Colour = Colour4.White,
        };
    }
}
