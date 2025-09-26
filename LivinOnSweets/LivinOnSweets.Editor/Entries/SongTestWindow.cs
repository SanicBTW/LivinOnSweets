using JetBrains.Annotations;
using LivinOnSweets.API.Audio;
using LivinOnSweets.API.Skinning;
using LivinOnSweets.Editor.Containers;
using LivinOnSweets.Editor.Enums;
using LivinOnSweets.Editor.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;
// ReSharper disable AccessToModifiedClosure

namespace LivinOnSweets.Editor.Entries;

internal partial class SongTestWindow() : ToolBarButton(FontAwesome.Solid.Music, ToolBarActionType.TOGGLEABLE)
{
    private EditorWindow songWindow;

    [Resolved] private EditorContainer editorView { get; set; }

    [Resolved] private ResourcePackManager packManager { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        EditorWindowOption option = new EditorWindowOption("", "refresh", refreshList);
        LoadComponentAsync(songWindow = new EditorWindow("", "song explorer", option), editorView.Add);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        songWindow.ScrollContent.Spacing = new Vector2(0, 6);

        packManager.SourceChanged += refreshList;
        refreshList();
    }

    protected override void Toggled(bool newState)
    {
        songWindow.ToggleVisibility();
    }

    private void refreshList()
    {
        songWindow.ScrollContent.Clear();

        foreach (string song in packManager.PackInfo.Songs.Available)
            songWindow.ScrollContent.Add(new SongCard(song));
    }

    private partial class SongCard(string targetSong) : Card
    {
        private static readonly FontUsage default_font = new(family: "GyeonggiTitle", size: 14F);

        [Resolved] private IResourcePackSource currentPack { get; set; }

        private Box progressBox;

        [CanBeNull] private ITrack track;
        private InfoTrackPreview trackDetails;
        private bool voiced;

        private FillFlowContainer<ClickableIcon> icons;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRange([
                progressBox = new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0,
                    Colour = EditorWindow.BackgroundColor.Darken(1F)
                },
                new SpriteText()
                {
                    Text = targetSong,
                    Font = default_font,
                    X = CARD_MARGIN,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Margin = new MarginPadding() { Left = CARD_MARGIN / 2, Top = CARD_MARGIN * 1.5F }
                },
                icons = new FillFlowContainer<ClickableIcon>()
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(12, 0),
                    Padding = new MarginPadding() { Right = CARD_MARGIN, Bottom = CARD_MARGIN },
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                }
            ]);

            trackDetails = new InfoTrackPreview(targetSong);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // since the metadata isnt implemented yet we will have to resort to nulls n bullshit
            ClickableIcon vocalsToggle = null;
            icons.Add(vocalsToggle = new ClickableIcon(FontAwesome.Solid.MicrophoneSlash, "Enable voiced tracks", () =>
            {
                voiced = !voiced;
                if (voiced)
                {
                    vocalsToggle!.ToolTip.Value = "Disable voiced tracks";
                    vocalsToggle!.Icon.Value = FontAwesome.Solid.Microphone;
                }
                else
                {
                    vocalsToggle!.ToolTip.Value = "Enable voiced tracks";
                    vocalsToggle!.Icon.Value = FontAwesome.Solid.MicrophoneSlash;
                }
            }));

            icons.Add(new ClickableIcon(FontAwesome.Regular.Clock, "Play the preview", () =>
            {
                trackDetails.SearchPreview(voiced);
                OnClick(null);
            }));

            icons.Add(new ClickableIcon(FontAwesome.Solid.Play, "Play the song", () =>
            {
                trackDetails.SearchWhole(voiced);
                OnClick(null);
            }));
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (track == null)
                return;

            if (!track.IsRunning)
                return;

            progressBox.Width = (float)(track.CurrentTime / track.Length);
        }

        protected override bool OnClick([CanBeNull] ClickEvent e)
        {
            if (trackDetails.LookupChanged)
            {
                if (track != null)
                {
                    this.TransformBindableTo(track.Volume, 0D, 500D).OnComplete(_ =>
                    {
                        disposeTrack();
                        OnClick(null);
                    });

                    return true;
                }
                else
                    gatherTrack();
            }

            if (track == null)
                return true;

            if (!track.IsRunning)
            {
                if (track.HasCompleted)
                {
                    track.Restart();
                    return true;
                }

                track.Start();
            }
            else
                track.Stop();

            return true;
        }

        private void gatherTrack()
        {
            ITrack gatheredTrack = currentPack.GetTrack(trackDetails);
            if (gatheredTrack == null)
            {
                this.FlashColour(Colour4.Red, 500D, Easing.OutQuint);
                return;
            }

            track = gatheredTrack;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            disposeTrack();
        }

        private void disposeTrack()
        {
            if (track == null)
                return;

            track.Stop();
            ((Track)track).Dispose();
            track = null;
        }
    }

    private partial class ClickableIcon : ClickableContainer, IHasTooltip
    {
        public Bindable<string> ToolTip { get; } = new();
        public Bindable<IconUsage> Icon { get; } = new();

        public ClickableIcon(IconUsage icon, string tooltip, Action action)
        {
            Icon.Value = icon;
            ToolTip.Value = tooltip;

            Action = action;
            AutoSizeAxes = Axes.Both;

            SpriteIcon sprIcon;
            Child = sprIcon = new SpriteIcon()
            {
                Icon = icon,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(22),
                Colour = Colour4.White
            };

            Icon.BindValueChanged((ev) => sprIcon.Icon = ev.NewValue);
        }

        public LocalisableString TooltipText => ToolTip.Value;
    }

    private partial class InfoTrackPreview(string song) : IAudioInfo
    {
        public bool LookupChanged { get; private set; }

        private string targetPath;

        public IEnumerable<string> LookupNames
        {
            get
            {
                LookupChanged = false;
                return [targetPath];
            }
        }

        public void SearchPreview(bool voiced = false)
        {
            LookupChanged = true;
            targetPath = $"Songs/{song}/{(voiced ? "preview_voices" : "preview")}.mp3";
        }

        public void SearchWhole(bool voiced = false)
        {
            LookupChanged = true;
            targetPath = $"Songs/{song}/{(voiced ? "voices" : "song")}.mp3";
        }

        private int volume = 100;

        public int Volume => volume;

        public void ChangeVolume(int newVolume)
        {
            volume = newVolume;
        }
    }
}
