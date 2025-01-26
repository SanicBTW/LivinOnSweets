using LivinOnSweets.API.Overlays;
using LivinOnSweets.API.Sprites.Editor;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace LivinOnSweets.API.Containers.Editor
{
    // This should be a part of TreeHierarchy>TreePreview
    internal partial class TreeHierarchyContent : Container
    {
        [Resolved]
        private EditorTreeVisualizer editorTreeVisualizer { get; set; }

        [Resolved]
        private Container<SlideContainer> editorSliders { get; set; }

        protected EditorSideBar Controller;
        protected Bindable<string> PreviewHeader;

        internal FillFlowContainer<EditorButton> ToolBar;

        private bool searchingCanvas = false;
        private BindableBool canvasSelectEnabled = new();

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

            if (editorTreeVisualizer.Target != null)
                canvasSelectEnabled.Value = true;

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
    }
}
