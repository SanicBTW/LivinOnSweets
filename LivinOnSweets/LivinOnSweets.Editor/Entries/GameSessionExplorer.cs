using System.Reflection;
using JetBrains.Annotations;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Graphics.Containers;
using LivinOnSweets.Editor.Containers;
using LivinOnSweets.Editor.Enums;
using LivinOnSweets.Editor.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Lists;

namespace LivinOnSweets.Editor.Entries;

internal partial class GameSessionExplorer() : ToolBarButton(FontAwesome.Solid.Gamepad, ToolBarActionType.TOGGLEABLE)
{
    private static readonly FieldInfo owner_type_field =
        typeof(GameSession).GetField("ownerType", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo bidirectional_field = typeof(GameSession)
        .GetField("bidirectional", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly PropertyInfo bindings_field = typeof(Bindable<GameView>)
        .GetProperty("Bindings", BindingFlags.Instance | BindingFlags.NonPublic);

    private EditorWindow gameSeshWindow;

    [CanBeNull] private Bindable<Type> ownerType;
    private Bindable<GameView> bidirectional;
    private LockedWeakList<Bindable<GameView>> bindingsBidir;

    private SpriteText ownerText;
    private SpriteText ownershipLockedText;
    private SpriteText inputEnabledText;
    private SpriteText bidirectionalText;

    [Resolved] private EditorContainer editorView { get; set; }
    [Resolved] private GameSession gameSession { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        // avoid manually triggering the schedule
        EditorWindowOption option = new EditorWindowOption("", "nothing", null);
        LoadComponentAsync(gameSeshWindow = new EditorWindow("", "game sesh viewer", option), editorView.Add);
        refreshReferences();
    }

    protected override void Toggled(bool newState) => gameSeshWindow.ToggleVisibility();

    protected override void LoadComplete()
    {
        base.LoadComplete();

        gameSeshWindow.ScrollContent.AddRange([
            ownerText = createText("owner ?"),
            ownershipLockedText = createText("ownership locked ?"),
            bidirectionalText = createText("bidirectional bindings ?"),
            inputEnabledText = createText("input enabled ?"),
        ]);

        refreshReferences();
    }

    protected override void UpdateAfterChildren()
    {
        ownerText.Text = $"owner {(ownerType!.Value != null ? ownerType.Value.Name : "")}";
        ownershipLockedText.Text = $"ownership locked {gameSession.OwnershipLocked.Value}";
        bidirectionalText.Text = $"bidirectional bindings {(bindingsBidir.Count())}"; // linq go brr
        inputEnabledText.Text = $"input enabled {gameSession.InputEnabled.Value}";
    }

    private void refreshReferences()
    {
        // pretty sure these wont change (reassign)
        ownerType = (Bindable<Type>)owner_type_field.GetValue(gameSession);
        bidirectional = (Bindable<GameView>)bidirectional_field.GetValue(gameSession);
        bindingsBidir = (LockedWeakList<Bindable<GameView>>)bindings_field.GetValue(bidirectional);
    }

    private SpriteText createText(string defaultText) => new()
    {
        Text = defaultText,
        Font = new FontUsage(family: "GyeonggiTitle", size: 16F),
        Padding = new MarginPadding(4)
    };
}
