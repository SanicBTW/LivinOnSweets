using System.Reflection;
using AetherFramework;
using AetherFramework.Configuration;
using JetBrains.Annotations;
using LivinOnSweets.Editor.Containers;
using LivinOnSweets.Editor.Enums;
using LivinOnSweets.Editor.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osuTK;

namespace LivinOnSweets.Editor.Entries;

internal partial class ModLoaderExplorer() : ToolBarButton(FontAwesome.Solid.Snowflake, ToolBarActionType.TOGGLEABLE)
{
    private static readonly FontUsage default_font = new(family: "GyeonggiTitle", size: 16F);

    [Resolved] private EditorContainer editorView { get; set; }

    private EditorWindow loaderWindow;

    [BackgroundDependencyLoader]
    private void load()
    {
        // aether framework lacks the ability to refresh/remove the assemblies loaded into the domain
        // so for now its not gonna do anything until i figure something out
        EditorWindowOption option = new EditorWindowOption("", "refresh mods");
        LoadComponentAsync(loaderWindow = new EditorWindow("", "mod loader", option), editorView.Add);
    }

    protected override void Toggled(bool newState) => loaderWindow.ToggleVisibility();

    protected override void LoadComplete()
    {
        loaderWindow.ScrollContent.Spacing = new Vector2(0, 6);

        loaderWindow.ScrollContent.AddRange([
            new VersionCard(),
            new ConfigCard(),
            new LoadedModsCard(),
        ]);
    }

    private partial class VersionCard : Card
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Content.RelativeSizeAxes = Axes.X;
            Content.AutoSizeAxes = AutoSizeAxes = Axes.Y;

            Assembly aetherAssembly = Assembly.GetAssembly(typeof(ModLoader))!;
            string displayVersion = aetherAssembly.GetName().Version!.ToString();

            // its most likely to have it actually
            AssemblyInformationalVersionAttribute detailedAetherVer = aetherAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            string fullVersionDisplay = detailedAetherVer!.InformationalVersion;

            int commitIndex = fullVersionDisplay.IndexOf('+');
            string preReleaseTag = "[none]";

            int minIndex = fullVersionDisplay.IndexOf('-');
            if (minIndex != -1)
                preReleaseTag = fullVersionDisplay[(minIndex + 1)..commitIndex];

            // ive seen other packages and it still contains the commit hash too soo
            string commitHash = fullVersionDisplay[(commitIndex + 1)..];

            // hey dummy, instead of copy pasting ts in every card (ts variable comes from ModListWindow card) maybe put it globally inside the card???
            const float vertical_margin = CARD_MARGIN * 1.5F;
            Add(new FillFlowContainer()
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
                    createText("aether framework version"),
                    createText(displayVersion).With(d => d.Font = d.Font.With(size: 12F)),
                    createSeparator(),
                    createText("prerelease"),
                    createText(preReleaseTag).With(d => d.Font = d.Font.With(size: 12F)),
                    createSeparator(),
                    createText("commit"),
                    createText(commitHash).With(d => d.Font = d.Font.With(size: 12F)), // should make it clickable???
                ]
            });
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

    private partial class ConfigCard : Card
    {
        [CanBeNull] private PresetConfigFile presetConfig; // its most likely to be preset config but just in case of messing up sometime
        private SpriteText curPreset;

        [BackgroundDependencyLoader]
        private void load(ModLoader modLoader)
        {
            ConfigFile config = modLoader.Configuration.Load();

            Content.RelativeSizeAxes = Axes.X;
            Content.AutoSizeAxes = AutoSizeAxes = Axes.Y;

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
                    createText("configuration provider in use"),
                    createText(modLoader.Configuration.ProviderName).With(d => d.Font = d.Font.With(size: 12F)),
                    createSeparator(),
                    createText("configuration file type"),
                    createText(config.GetType().Name).With(d => d.Font = d.Font.With(size: 12F)),
                ]
            };

            // I should list the available presets n stuff but we wont do that for now
            if (config is PresetConfigFile ctPresetConfig)
            {
                presetConfig = ctPresetConfig;

                infoContainer.AddRange([
                    createSeparator(),
                    createText("current preset active"),
                    curPreset = createText(ctPresetConfig.CurrentPreset).With(d => d.Font = d.Font.With(size: 12F)),
                ]);
            }

            Add(infoContainer);
        }

        protected override void UpdateAfterChildren()
        {
            if (presetConfig == null)
                return;

            curPreset.Text = presetConfig.CurrentPreset;
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

    private partial class LoadedModsCard : Card
    {
        [Resolved] private EditorContainer editorView { get; set; }

        private ConfigFile config;
        [CanBeNull] private ModListWindow lastWindow;
        private bool lastSelection;

        [BackgroundDependencyLoader]
        private void load(ModLoader modLoader, GameHost host)
        {
            // on the best cases this will return the already loaded backer config
            config = modLoader.Configuration.Load();

            Add(new FillFlowContainer()
            {
                Direction = FillDirection.Vertical,
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                X = CARD_MARGIN,
                Margin = new MarginPadding() { Left = CARD_MARGIN / 2, Top = CARD_MARGIN * 1.5F, },
                Size = Size - new Vector2(CARD_MARGIN * 4, 0),
                Spacing = new Vector2(0, 2),
                Children =
                [
                    new SpriteText()
                    {
                        Text = $"mods loaded {config.EnabledMods.Count + config.DisabledMods.Count}", // smart ass
                        RelativeSizeAxes = Axes.X,
                        Font = default_font
                    },
                    new ModsTextHitZone(true) { Action = () => moveToView(true) },
                    new ModsTextHitZone(false) { Action = () => moveToView(false) },
                    new ClickableText("open mods folder") { Action = () => host.Storage.GetStorageForDirectory("mods").PresentFileExternally("") }
                ]
            });
        }

        private void moveToView(bool listEnabledMods)
        {
            if (lastWindow != null)
            {
                if (lastSelection == listEnabledMods)
                {
                    lastWindow.ToggleVisibility();
                    return;
                }

                closeView();
            }

            lastSelection = listEnabledMods;
            LoadComponentAsync(lastWindow = new ModListWindow(listEnabledMods, closeView), editorView.Add);
        }

        private void closeView()
        {
            if (lastWindow == null)
                return;

            lastWindow.Hide();

            double hideTime = lastWindow.LatestTransformEndTime - lastWindow.TransformStartTime;
            Scheduler.AddDelayed((win) =>
            {
                editorView.Remove(win, true);
                lastWindow = null;
            }, lastWindow, hideTime);
        }

        private partial class ClickableText(string text) : ClickableContainer
        {
            protected readonly SpriteText ShowText = new(){ Text = text, RelativeSizeAxes = Axes.X, Font = default_font.With(size: 14F) };

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Add(ShowText);
            }

            protected override bool OnHover(HoverEvent e)
            {
                ShowText.FadeTo(0.7F, 400D);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                ShowText.FadeTo(1F, 400D);
            }
        }

        private partial class ModsTextHitZone(bool showsEnabled) : ClickableText(showsEnabled ? "enabled mods" : "disabled mods")
        {
            private ConfigFile config;
            private string calcPrefix;

            [BackgroundDependencyLoader]
            private void load(ModLoader modLoader)
            {
                config = modLoader.Configuration.Load();
                calcPrefix = ShowText.Text.ToString();
            }

            protected override void UpdateAfterChildren()
            {
                int count = showsEnabled ? config.EnabledMods.Count : config.DisabledMods.Count;
                ShowText.Text = $"{calcPrefix} {count}";
            }
        }
    }
}
