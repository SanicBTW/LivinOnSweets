using LivinOnSweets.API.Attributes;
using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Containers.Editor;
using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Overlays;
using LivinOnSweets.API.Sprites.Editor;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace LivinOnSweets.API.EditorComponents
{
    [EditorImportOrder(0)]
    public partial class TreeHierarchy(EditorSideBar controller) : EditorEntry(controller, "tree hierarchy", EditorEntryContentAnimation.ANOTHER_VIEW)
    {
        protected override EntryPreview CreatePreview() => new TreePreview(Controller);

        internal partial class TreePreview(EditorSideBar controller) : EntryPreview(controller)
        {
            private SpriteText selectedObject;
            private Bindable<string> selectionText = new("select a master container");

            [BackgroundDependencyLoader]
            private void load()
            {
                PreviewContent.Add(selectedObject = new SpriteText()
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Font = new FontUsage(family: "DNFBitBit", size: 32F),
                    Margin = new MarginPadding(){ Left = Padding.Left / 2 },
                    Colour = Colour4.Black // Opposite as the background, to not need to wait for the accents to apply
                });

                selectionText.BindValueChanged((ev) =>
                {
                    selectedObject.Text = ev.NewValue;
                }, true);

                Controller.PrimaryColor.BindValueChanged((ev) =>
                {
                    selectedObject.Colour = ev.NewValue;
                });
            }

            protected override Container CreateSlideContent() => new TreeHierarchyContent(Controller, selectionText);
        }

        internal partial class TreeHierarchyContent : Container
        {
            [Resolved]
            private EditorTreeVisualizer editorTreeVisualizer { get; set; }

            [Resolved]
            private Container<SlideContainer> editorSliders { get; set; }

            protected EditorSideBar Controller;
            protected Bindable<string> PreviewHeader;

            internal FillFlowContainer<EditorButton> ToolBar;

            public TreeHierarchyContent(EditorSideBar controller, Bindable<string> previewHeader)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Padding = new MarginPadding(16);

                Controller = controller;
                PreviewHeader = previewHeader;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                loadToolBar();
            }

            private void loadToolBar()
            {
                Box background;
                AddInternal(new Container()
                {
                    Masking = true,
                    CornerRadius = 6,
                    AutoSizeAxes = Axes.Both,
                    Children =
                    [
                        background = new Box()
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        ToolBar = new FillFlowContainer<EditorButton>()
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Padding = new MarginPadding(8),
                            Spacing = new Vector2(4, 0),
                            Children =
                            [
                                new EditorButton()
                                {
                                    Text = "refresh tree",
                                },
                                new EditorButton()
                                {
                                    Text = "choose target",
                                    Action = chooseTarget
                                },
                                new EditorButton()
                                {
                                    Text = "choose canvas"
                                }
                            ]
                        }
                    ]
                });

                Controller.SecondaryColor.BindValueChanged((ev) =>
                {
                    background.Colour = ev.NewValue;

                    foreach (EditorButton button in ToolBar)
                    {
                        button.TextColor = ev.NewValue;
                    }

                }, true);

                Controller.PrimaryColor.BindValueChanged((ev) =>
                {
                    foreach (EditorButton button in ToolBar)
                    {
                        button.BackgroundColour = ev.NewValue;
                    }
                }, true);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                editorTreeVisualizer.OnTargetChanged += targetChanged;
            }

            protected override void Dispose(bool isDisposing)
            {
                editorTreeVisualizer.OnTargetChanged -= targetChanged;
                editorTreeVisualizer.Hide();

                base.Dispose(isDisposing);
            }

            private void targetChanged(Drawable d)
            {
                if (d == null)
                    PreviewHeader.SetDefault();
                else
                {
                    PreviewHeader.Value = $"selected {d.ToString().ToLower()}";

                    // only pop the sliders in when the target really changed
                    foreach (SlideContainer slider in editorSliders)
                    {
                        // only pop the slider if the slide block is true, since it means it was opened previously
                        // NOTE: this could lead to the properties panel to pop out if it was focused before hand, its a feature!
                        if (!slider.SlideBlock.Value)
                            continue;

                        slider.BlockHoverSlide = false;
                        slider.MoveToX(0, slider.SlideDuration, Easing.OutQuint);
                    }
                }
            }

            private void chooseTarget()
            {
                foreach (SlideContainer slider in editorSliders)
                {
                    // we only want to move sliders that are currently visible and blocked from sliding out
                    if (slider.IsHidden() && !slider.SlideBlock.Value)
                        continue;

                    slider.BlockHoverSlide = true;
                    slider.MoveToX(slider.OutOfBoundsPosition, slider.SlideDuration, Easing.OutQuint);
                }

                editorTreeVisualizer.StartSearching();
            }
        }
    }
}
