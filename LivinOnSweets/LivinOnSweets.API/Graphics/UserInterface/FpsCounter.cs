using LivinOnSweets.API.Configuration;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osuTK;

namespace LivinOnSweets.API.Graphics.UserInterface
{
    // Another copy of lazer but customized lmao
    public partial class FpsCounter : VisibilityContainer
    {
        private readonly BindableBool showFpsDisplay = new(true);

        private const double min_time_between_updates = 10;
        private const double spike_time_ms = 20;
        private const float idle_background_alpha = 0.4f;
        private const double transition_duration = 500D;

        private double displayedFpsCount;
        private double displayedFrameTime;
        private bool isDisplayed;

        private double aimDrawFps;
        private double aimUpdateFps;

        private double lastUpdate;
        private ThrottledFrameClock drawClock = null!;
        private ThrottledFrameClock updateClock = null!;
        private ThrottledFrameClock inputClock = null!;

        /// <summary>
        /// The last time value where the display was required (due to a significant change or hovering).
        /// </summary>
        private double lastDisplayRequiredTime;

        private Container mainContent;
        private Container background;
        private Container counters;

        private SpriteText fpsText;

        public FpsCounter()
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Origin = Anchor.BottomLeft;
            Margin = new MarginPadding(14);
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host, SweetConfigManager config)
        {
            config.BindWith(SweetSetting.ShowFpsDisplay, showFpsDisplay);

            FontUsage defaultFont = new FontUsage(family: "DNFBitBit", size: 16);

            InternalChild = mainContent = new Container
            {
                Alpha = 0,
                Height = 26,
                Children =
                [
                    background = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        CornerRadius = 5,
                        CornerExponent = 5f,
                        Masking = true,
                        Alpha = idle_background_alpha,
                        Child = new Box { Colour = Colour4.Black, RelativeSizeAxes = Axes.Both }
                    },
                    counters = new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Margin = new MarginPadding(6),
                        Children =
                        [
                            fpsText = new SpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = defaultFont,
                                Spacing = new Vector2(-1),
                            }
                        ]
                    }
                ]
            };

            drawClock = host.DrawThread.Clock;
            updateClock = host.UpdateThread.Clock;
            inputClock = host.InputThread.Clock;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            requestDisplay();

            showFpsDisplay.BindValueChanged(showFps =>
            {
                State.Value = showFps.NewValue ? Visibility.Visible : Visibility.Hidden;
                if (showFps.NewValue)
                    requestDisplay();
            }, true);

            State.BindValueChanged(state => showFpsDisplay.Value = state.NewValue == Visibility.Visible);
        }

        // I got extremely lazy, my apologies
        protected override void Update()
        {
            base.Update();

            double elapsedDrawFrameTime = drawClock.ElapsedFrameTime;
            double elapsedUpdateFrameTime = updateClock.ElapsedFrameTime;

            // If the game goes into a suspended state (ie. debugger attached or backgrounded on a mobile device)
            // we want to ignore really long periods of no processing.
            if (elapsedUpdateFrameTime > 10000)
                return;

            mainContent.Width = Math.Max(mainContent.Width, counters.DrawWidth + counters.Margin.Right);

            // Handle the case where the window has become inactive or the user changed the
            // frame limiter (we want to show the FPS as it's changing, even if it isn't an outlier).
            bool aimRatesChanged = updateAimFps();

            bool hasUpdateSpike = displayedFrameTime < spike_time_ms && elapsedUpdateFrameTime > spike_time_ms;
            // use elapsed frame time rather then FramesPerSecond to better catch stutter frames.
            bool hasDrawSpike = displayedFpsCount > (1000 / spike_time_ms) && elapsedDrawFrameTime > spike_time_ms;

            const float damp_time = 100;

            displayedFrameTime = Interpolation.DampContinuously(displayedFrameTime, elapsedUpdateFrameTime, hasUpdateSpike ? 0 : damp_time, elapsedUpdateFrameTime);

            if (hasDrawSpike)
                // show spike time using raw elapsed value, to account for `FramesPerSecond` being so averaged spike frames don't show.
                displayedFpsCount = 1000 / elapsedDrawFrameTime;
            else
                displayedFpsCount = Interpolation.DampContinuously(displayedFpsCount, drawClock.FramesPerSecond, damp_time, Time.Elapsed);

            if (Time.Current - lastUpdate > min_time_between_updates)
            {
                updateFpsDisplay();

                lastUpdate = Time.Current;
            }

            bool hasSignificantChanges = aimRatesChanged
                                            || hasDrawSpike
                                            || hasUpdateSpike
                                            || displayedFpsCount < aimDrawFps * 0.8
                                            || 1000 / displayedFrameTime < aimUpdateFps * 0.8;

            if (hasSignificantChanges)
                requestDisplay();
            else if (isDisplayed && Time.Current - lastDisplayRequiredTime > 2000 && !IsHovered)
            {
                mainContent.FadeTo(0.7f, transition_duration, Easing.OutQuint);
                isDisplayed = false;
            }
        }

        private void requestDisplay()
        {
            lastDisplayRequiredTime = Time.Current;

            if (isDisplayed) return;

            mainContent.FadeTo(1, transition_duration, Easing.OutQuint);
            isDisplayed = true;
        }

        private void updateFpsDisplay()
        {
            fpsText.Text = $"{displayedFpsCount:#,0}fps {displayedFrameTime:#,0}ms";
        }

        private bool updateAimFps()
        {
            if (updateClock.Throttling)
            {
                double newAimDrawFps = drawClock.MaximumUpdateHz;
                double newAimUpdateFps = updateClock.MaximumUpdateHz;

                if (Precision.AlmostEquals(aimDrawFps, newAimDrawFps) && Precision.AlmostEquals(aimUpdateFps, newAimUpdateFps)) return false;

                aimDrawFps = newAimDrawFps;
                aimUpdateFps = newAimUpdateFps;
            }
            else
            {
                double newAimFps = inputClock.MaximumUpdateHz;

                if (Precision.AlmostEquals(aimDrawFps, newAimFps) && Precision.AlmostEquals(aimUpdateFps, newAimFps)) return false;

                aimUpdateFps = aimDrawFps = newAimFps;
            }

            return true;
        }

        protected override void PopIn() => this.MoveToX(0, transition_duration, Easing.OutQuint);

        protected override void PopOut() => this.MoveToX(-(DrawWidth + Margin.TotalHorizontal), transition_duration, Easing.OutQuint);

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeTo(1, transition_duration);
            requestDisplay();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeTo(idle_background_alpha, transition_duration);
            requestDisplay();
            base.OnHoverLost(e);
        }
    }
}
