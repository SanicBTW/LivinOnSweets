using LivinOnSweets.API.Components;
using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Containers.Editor;
using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Extensions;
using LivinOnSweets.API.Interfaces;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;

namespace LivinOnSweets.API.Overlays
{
    [Cached]
    internal partial class EditorContainer : OverlayContainer
    {
        // propagate this into the children to be able to add more sliders into the editor
        [Cached]
        public readonly Container<SlideContainer> Sliders;

        [Cached]
        public readonly EditorTreeVisualizer TreeVisualizer;

        private SlideContainer sideBar;
        private SlideContainer propertiesPanel;

        private EditorLeftReceiver leftSideColorizer;
        private EditorRightReceiver rightSideColorizer;
        // private FillFlowContainer palette;

        public EditorContainer()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                leftSideColorizer = new EditorLeftReceiver(),
                rightSideColorizer = new EditorRightReceiver(),
                new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black,
                    Alpha = 0.35f
                },
                TreeVisualizer = new EditorTreeVisualizer(),
                Sliders = new Container<SlideContainer>()
                {
                    RelativeSizeAxes = Axes.Both,
                    Children =
                    [
                        sideBar = new SlideContainer(true)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.Y,
                            Width = 450,
                        },
                        propertiesPanel = new SlideContainer(false)
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            RelativeSizeAxes = Axes.Y,
                            Width = 450,
                        },
                    ]
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Box sideBg;
            SweetScrollContainer scroller;
            sideBar.Children = new Drawable[]
            {
                sideBg = new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black,
                },
                scroller = new SweetScrollContainer()
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    ClampExtension = 10,
                    Child = new EditorSideBar(sideBar)
                }
            };

            Box propsBg;
            SpriteText wipText;
            propertiesPanel.Child = new Container()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    propsBg = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.Black
                    },
                    wipText = new SpriteText()
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = new FontUsage(family: "DNFBitBit", size: 48F),
                        Text = "Work in progress"
                    }
                }
            };

            leftSideColorizer.Bind(sideBg, scroller, sideBar.PanelNudge);
            rightSideColorizer.Bind(propsBg, wipText, propertiesPanel.PanelNudge);
        }

        protected override void PopIn() => this.FadeIn(500D, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(500D, Easing.OutQuint);

        // Resets the slide blocks when clicked outside
        protected override bool OnMouseDown(MouseDownEvent e)
        {
            foreach (SlideContainer slider in Sliders)
            {
                if (slider.ClickOutClosesContainer && slider.IsVisible() && slider.SlideBlock.Value)
                {
                    slider.SlideBlock.Value = false;
                    slider.MoveToX(slider.OutOfBoundsPosition, slider.SlideDuration, Easing.OutQuint);
                }
            }

            return base.OnMouseDown(e);
        }

        private abstract partial class EditorAccentReceiver : Component, IAccentColorReceiver
        {
            [Resolved]
            protected AccentComponent AccentComponent { get; private set; }

            protected bool Bound;

            private AccentBannerSide accentSide;
            protected BindableColour4 Primary;
            protected BindableColour4 Secondary;
            protected BindableColour4 Tertiary;

            public EditorAccentReceiver(AccentBannerSide targetSide)
            {
                accentSide = targetSide;
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

        private partial class EditorLeftReceiver() : EditorAccentReceiver(AccentBannerSide.Left)
        {

            protected Box Background;
            protected SweetScrollContainer Scroller;
            protected SlideContainer.Nudge PanelNudge;

            public void Bind(Box background, SweetScrollContainer scroller, SlideContainer.Nudge panelNudge)
            {
                Background = background;
                Scroller = scroller;
                PanelNudge = panelNudge;
            }

            private protected override void BindAccents()
            {
                Primary.BindValueChanged((ev) =>
                {
                    Background.Colour = ev.NewValue;
                });

                Scroller.ScrollBarColour.BindTo(Secondary);
                PanelNudge.NudgeColor.BindTo(Tertiary);
            }
        }

        private partial class EditorRightReceiver() : EditorAccentReceiver(AccentBannerSide.Right)
        {
            protected Box Background;
            protected SpriteText WipText;
            protected SlideContainer.Nudge PanelNudge;

            public void Bind(Box background, SpriteText wipText, SlideContainer.Nudge panelNudge)
            {
                Background = background;
                WipText = wipText;
                PanelNudge = panelNudge;
            }

            private protected override void BindAccents()
            {
                Primary.BindValueChanged((ev) =>
                {
                    Background.Colour = ev.NewValue;
                });

                Tertiary.BindValueChanged((ev) =>
                {
                    WipText.Colour = ev.NewValue;
                });

                PanelNudge.NudgeColor.BindTo(Tertiary);
            }
        }
    }
}
