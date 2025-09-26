using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Skinning;
using LivinOnSweets.Editor.Containers;
using LivinOnSweets.Editor.Enums;
using LivinOnSweets.Editor.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osuTK;

namespace LivinOnSweets.Editor.Entries;

internal partial class ResourcePackExplorer() : ToolBarButton(FontAwesome.Solid.Archive, ToolBarActionType.TOGGLEABLE)
{
    private EditorWindow explorerWindow;

    [Resolved] private EditorContainer editorView { get; set; }

    [Resolved] private ResourcePackManager packManager { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        EditorWindowOption option = new("", "nothing");
        LoadComponentAsync(explorerWindow = new EditorWindow("", "resource pack explorer", option), editorView.Add);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        explorerWindow.ScrollContent.Spacing = new Vector2(0, 6);

        foreach (ResourcePack pack in packManager.LoadedPacks)
            explorerWindow.ScrollContent.Add(new ResPackCard(pack));

        // bruh
        packManager.SourceChanged += updateSelectedPack;
        updateSelectedPack(); // execute once
    }

    protected override void Toggled(bool newState)
    {
        explorerWindow.ToggleVisibility();
    }

    private void updateSelectedPack()
    {
        IEnumerable<ResPackCard> cards = explorerWindow.ScrollContent.ChildrenOfType<ResPackCard>();
        string newId = packManager.PackInfo.Metadata.Id;
        foreach (ResPackCard card in cards)
            card.FadeColour(card.Pack.PackInfo.Metadata.Id != newId ? Colour4.White : Colour4.Gray, 150D, Easing.OutQuint);
    }

    // too lazy to backtrack to clickablecontainer -> card so instead we handle it ourselves
    private partial class ResPackCard(ResourcePack targetPack) : Card, IHasTooltip
    {
        private static readonly FontUsage default_font = new(family: "GyeonggiTitle", size: 14F);

        [Resolved] private SweetConfigManager sweetConfig { get; set; }

        public readonly ResourcePack Pack = targetPack;

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new FillFlowContainer<SpriteText>()
            {
                Name = "details container",
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
                        Text = Pack.PackInfo.Metadata.Name,
                        RelativeSizeAxes = Axes.X,
                        Font = default_font,
                    },
                    new SpriteText()
                    {
                        Text = Pack.PackInfo.Metadata.Description,
                        RelativeSizeAxes = Axes.X,
                        Font = default_font.With(size: 10F),
                    }
                ]
            });
        }

        protected override bool OnClick(ClickEvent e)
        {
            // check resourcepackmanager for the explanation behind
            int packIndex = ResourcePackManager.OFFICIAL_RESOURCE_PACKS.ToList().IndexOf(Pack.PackInfo.Metadata.Id);
            if (packIndex == -1)
            {
                sweetConfig.SetValue(SweetSetting.GameUpdate, GameUpdateVersion.SugarRush); // setting back to the first version to avoid issues (not implemented)
                sweetConfig.SetValue(SweetSetting.ResourcePack, Pack.PackInfo.Metadata.Id);
                return true;
            }

            GameUpdateVersion convVer = (GameUpdateVersion)packIndex;
            sweetConfig.SetValue(SweetSetting.GameUpdate, convVer);
            return true;
        }

        public LocalisableString TooltipText => Pack.PackInfo.Metadata.Id;
    }
}
