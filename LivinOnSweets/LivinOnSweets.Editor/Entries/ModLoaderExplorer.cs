using System.Reflection;
using AetherFramework;
using LivinOnSweets.Editor.Containers;
using LivinOnSweets.Editor.Enums;
using LivinOnSweets.Editor.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace LivinOnSweets.Editor.Entries;

internal partial class ModLoaderExplorer() : ToolBarButton(FontAwesome.Solid.File, ToolBarActionType.TOGGLEABLE)
{
    [Resolved] private EditorContainer editorView { get; set; }
    [Resolved] private ModLoader modLoader { get; set; }

    private EditorWindow loaderWindow;
    private SpriteText enabledText;
    private SpriteText disabledText;

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
        base.LoadComplete();

        Assembly aetherAssembly = Assembly.GetAssembly(typeof(ModLoader))!;
        string displayVersion = aetherAssembly.GetName().Version!.ToString();

        AssemblyInformationalVersionAttribute detailedAetherVer = aetherAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (detailedAetherVer != null)
        {
            string verAttr = detailedAetherVer.InformationalVersion;
            displayVersion = verAttr.Substring(0, verAttr.IndexOf('+') + 6);
        }

        loaderWindow.ScrollContent.AddRange([
            createText("Aether version"),
            createText(displayVersion),
            createText(modLoader.ConfigurationProvider),
            createText($"loaded mods {modLoader.LoadedMods.Count()}"),
            enabledText = createText("enabled mods ?"),
            disabledText = createText("disabled mods ?"),
        ]);
    }

    protected override void UpdateAfterChildren()
    {
        // god its using LINQ OMGFGFGFGFGG
        enabledText.Text = $"enabled mods {modLoader.EnabledMods.Count()}";
        disabledText.Text = $"disabled mods {modLoader.DisabledMods.Count()}";
    }

    private SpriteText createText(string defaultText) => new()
    {
        Text = defaultText,
        Font = new FontUsage(family: "GyeonggiTitle", size: 16F),
        Padding = new MarginPadding(4),
        MaxWidth = loaderWindow.ScrollContent.DrawWidth
    };
}
