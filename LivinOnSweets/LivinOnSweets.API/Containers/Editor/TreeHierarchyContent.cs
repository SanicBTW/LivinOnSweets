using LivinOnSweets.API.Components;
using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Extensions;
using LivinOnSweets.API.Interfaces;
using LivinOnSweets.API.Overlays;
using LivinOnSweets.API.Sprites.Editor;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace LivinOnSweets.API.Containers.Editor
{
    // This should be a part of TreeHierarchy>TreePreview
    internal partial class TreeHierarchyContent : Container, IAccentColorReceiver
    {
        [Resolved]
        private EditorTreeVisualizer editorTreeVisualizer { get; set; }

        [Resolved]
        private Container<SlideContainer> editorSliders { get; set; }

        [Resolved]
        private AccentComponent accentComponent { get; set; }

        protected BindableColour4 PrimaryColor = new();
        protected BindableColour4 SecondaryColor = new();

        private SweetScrollContainer scrollContainer => (SweetScrollContainer)Parent!.Parent;

        protected Bindable<string> PreviewHeader;

        internal FillFlowContainer FlowContainer;
        internal FillFlowContainer<EditorButton> ToolBar;

        internal FillFlowContainer<TargetListEntry> TargetList;
        private SpriteText waitingText;

        private bool searchingCanvas;
        private BindableBool canvasSelectEnabled = new();

        public TreeHierarchyContent(Bindable<string> previewHeader)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding(16);

            PreviewHeader = previewHeader;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            FlowContainer = new FillFlowContainer()
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 4),
            };

            // Moved the tooltip container and the context menu container here
            // since when its inside an auto sized container it resizes the container and acts weird
            InternalChild = new EditorTooltipContainer()
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Child = new EditorContextMenuContainer()
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Child = FlowContainer
                }
            };

            loadToolBar();
            loadTargetList();
        }

        private void loadToolBar()
        {
            Box background;
            FlowContainer.Add(new Container()
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
                                Text = "choose target",
                                Action = chooseTarget
                            },
                            new EditorButton()
                            {
                                Text = "choose canvas",
                                // even though we are setting an action which should enable the button,
                                // we still want to bind the enabled bindable to our own to control the activation of the button
                                Action = chooseCanvas,
                                Enabled = { BindTarget = canvasSelectEnabled }
                            }
                        ]
                    },
                ]
            });

            PrimaryColor.BindValueChanged((ev) =>
            {
                foreach (EditorButton button in ToolBar)
                {
                    button.BackgroundColour = ev.NewValue;
                }
            }, true);

            SecondaryColor.BindValueChanged((ev) =>
            {
                background.Colour = ev.NewValue;

                foreach (EditorButton button in ToolBar)
                {
                    button.TextColor = ev.NewValue;
                }

            }, true);
        }

        private void loadTargetList()
        {
            Box background;
            FlowContainer.Add(new Container()
            {
                Masking = true,
                CornerRadius = 6,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children =
                [
                    background = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    TargetList = new FillFlowContainer<TargetListEntry>()
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(8),
                        Spacing = new Vector2(0, 4),
                    }
                ]
            });

            SecondaryColor.BindValueChanged((ev) =>
            {
                background.Colour = ev.NewValue;
            }, true);

            TargetList.Add(new TargetListEntry());
            TargetList.Add(new TargetListEntry());
        }

        private void addWaitingText()
        {
            Container parent = (Container)Parent!.Parent!.Parent;

            parent!.Add(waitingText = new SpriteText()
            {
                Text = "waiting for target selection",
                Font = new FontUsage(family: "DNFBitBit", size: 32F),
                Colour = SecondaryColor.Value,
                Alpha = 0f,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });

            PrimaryColor.BindValueChanged((ev) =>
            {
                waitingText.Colour = ev.NewValue;
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Populate to the children
            accentComponent.PropagateToChildren(this);

            addWaitingText();

            if (editorTreeVisualizer.Target != null)
            {
                scrollContainer!.AllowScroll();
                canvasSelectEnabled.Value = true;
            }
            else
            {
                scrollContainer!.BlockScroll();
                waitingText.FadeIn(200D, Easing.OutQuint);
            }

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
            if (searchingCanvas)
            {
                if (d == null)
                    return;

                popSlidersIn();
                searchingCanvas = false;
                // TODO: Cache the selected drawable for the canvas operations
            }
            else
            {
                if (d == null)
                {
                    PreviewHeader.SetDefault();
                    canvasSelectEnabled.Value = false;
                }
                else
                {
                    if (waitingText.IsPresent)
                    {
                        waitingText.FadeOut(200D, Easing.OutQuint);
                        scrollContainer.AllowScroll();
                    }

                    PreviewHeader.Value = "selected master container";
                    //PreviewHeader.Value = $"selected {d.ToString().ToLower()}";
                    canvasSelectEnabled.Value = true;
                    popSlidersIn();
                }
            }
        }

        private void chooseTarget()
        {
            popSlidersOut();
            editorTreeVisualizer.StartSearching();
        }

        private void chooseCanvas()
        {
            searchingCanvas = true;
            popSlidersOut();
            editorTreeVisualizer.StartSearching(editorTreeVisualizer.Target);
        }

        // unify the functions?
        private void popSlidersOut()
        {
            foreach (SlideContainer slider in editorSliders)
            {
                // we only want to move sliders that are currently visible and blocked from sliding out
                if (slider.IsHidden() && !slider.SlideBlock.Value)
                    continue;

                slider.BlockHoverSlide = true;
                slider.MoveToX(slider.OutOfBoundsPosition, slider.SlideDuration, Easing.OutQuint);
            }
        }

        private void popSlidersIn()
        {
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

        AccentBannerSide IAccentColorReceiver.AccentSide => AccentBannerSide.Left;

        void IAccentColorReceiver.PropagateAccents(BindableColour4[] colors)
        {
            PrimaryColor.BindTo(colors[1]);
            SecondaryColor.BindTo(colors[2]);
        }

        void IAccentColorReceiver.AccentsUpdated(double duration, Easing easing)
        {
            BindableColour4 newPrimary = accentComponent.GetAccent(this, AccentColorRole.Secondary);
            BindableColour4 newSecondary = accentComponent.GetAccent(this, AccentColorRole.Tertiary);

            this.TransformBindableTo(PrimaryColor, newPrimary.Value, duration, easing);
            this.TransformBindableTo(SecondaryColor, newSecondary.Value, duration, easing);
        }
    }
}
