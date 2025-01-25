using System.Collections.Generic;
using System.Linq;
using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Data;
using LivinOnSweets.API.Sprites.UI;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Framework.Timing;
using osu.Framework.Utils;

namespace LivinOnSweets.Game.StartScreens
{
    // Used to preload resources for the first run, kinda similar to Loader from osu!lazer
    // Literally https://github.com/ppy/osu/blob/master/osu.Game/Screens/Loader.cs
    public partial class PreloaderScreen : SweetScreen
    {
        private SweetScreen nextScreen;
        private ShaderPrecompiler precompiler;
        private StutterComponent stutterChecker;

        private LoadingSpinner spinner;
        private ScheduledDelegate spinnerShow;

        public PreloaderScreen()
        {
            ValidForResume = false;
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            EditorColours.Reset();

            LoadComponentAsync(stutterChecker = CreateStutterChecker(), AddInternal);

            LoadComponentAsync(precompiler = CreateShaderPrecompiler(), AddInternal);

            LoadComponentAsync(nextScreen = CreateNextScreen());

            LoadComponentAsync(spinner = new LoadingSpinner(), _ =>
            {
                AddInternal(spinner);
                spinnerShow = Scheduler.AddDelayed(spinner.Show, 200);
            });

            loadedCheck();
        }

        private void loadedCheck()
        {
            if (nextScreen?.LoadState != LoadState.Ready || !precompiler.FinishedCompiling || !stutterChecker.IsStable)
            {
                Schedule(loadedCheck);
                return;
            }

            spinnerShow?.Cancel();

            if (spinner.State.Value == Visibility.Visible)
            {
                spinner.Hide();
                Scheduler.AddDelayed(() => ScreenStack.Push(nextScreen), LoadingSpinner.TRANSITION_DURATION * 2);
            }
            else
                ScreenStack.Push(nextScreen);
        }

        protected virtual SweetScreen CreateNextScreen() => new StartupScreen();

        protected virtual ShaderPrecompiler CreateShaderPrecompiler() => new();

        protected virtual StutterComponent CreateStutterChecker() => new();

        // Literally https://github.com/ppy/osu/blob/master/osu.Game/Screens/Loader.cs#L117
        public partial class ShaderPrecompiler : Component
        {
            private readonly List<IShader> loadTargets = [];
            protected virtual bool AllLoaded => loadTargets.All(s => s.IsLoaded);

            public bool FinishedCompiling { get; protected set; }

            [BackgroundDependencyLoader]
            private void load(ShaderManager manager)
            {
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE));
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.BLUR));
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_3, FragmentShaderDescriptor.TEXTURE));

                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, @"FastCircle"));
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, @"CircularProgress"));
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, @"SaturationSelectorBackground"));
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, @"HueSelectorBackground"));
            }

            protected override void Update()
            {
                base.Update();

                if (AllLoaded)
                {
                    FinishedCompiling = true;
                    Expire();
                }
            }
        }

        // component used to check draw/update spikes before being able to continue, this is to stabilize the game and reproduce animations properly
        // calcs coming from https://github.com/SanicBTW/FunkinSharp/blob/legacy/FunkinSharp/FunkinSharp.Game/Core/Overlays/FPSOverlay.cs
        // but def i should improve this code ngl
        public partial class StutterComponent : Component
        {
            private double displayedFpsCount;
            private double displayedFrameTime;
            private double spikeTimeMs = 20;

            private double maxTime = 2500D;
            private double timer = -1000D; // start at -1s to get some grace time after the game refreshed

            private ThrottledFrameClock drawClock;
            private ThrottledFrameClock updateClock;

            public bool IsStable { get; protected set; }

            [BackgroundDependencyLoader]
            private void load(GameHost gameHost)
            {
                drawClock = gameHost.DrawThread.Clock;
                updateClock = gameHost.UpdateThread.Clock;
            }

            protected override void Update()
            {
                base.Update();

                double elapsedDrawFrameTime = drawClock.ElapsedFrameTime;
                double elapsedUpdateFrameTime = updateClock.ElapsedFrameTime;

                // If the game goes into a suspended state (ie. debugger attached or backgrounded on a mobile device)
                // we want to ignore really long periods of no processing.
                if (elapsedUpdateFrameTime > 10000)
                    return;

                bool hasUpdateSpike = displayedFrameTime < spikeTimeMs && elapsedUpdateFrameTime > spikeTimeMs;
                bool hasDrawSpike = displayedFpsCount > (1000 / spikeTimeMs) && elapsedDrawFrameTime > spikeTimeMs;

                const float damp_time = 100;

                displayedFrameTime = Interpolation.DampContinuously(displayedFrameTime, elapsedUpdateFrameTime, hasUpdateSpike ? 0 : damp_time, elapsedUpdateFrameTime);

                if (hasDrawSpike)
                    // show spike time using raw elapsed value, to account for `FramesPerSecond` being so averaged spike frames don't show.
                    displayedFpsCount = 1000 / elapsedDrawFrameTime;
                else
                    displayedFpsCount = Interpolation.DampContinuously(displayedFpsCount, drawClock.FramesPerSecond, damp_time, Time.Elapsed);

                // if the fps are below 15 and the last update frame time is above 1k it means something bad is going on
                bool lowFps = displayedFpsCount <= 15;
                bool highFrameTime = displayedFrameTime >= 1000;

                if (hasUpdateSpike || hasDrawSpike || lowFps || highFrameTime)
                {
                    maxTime += 70D;
                    timer = 0;
                }

                timer += elapsedDrawFrameTime;
                IsStable = timer >= maxTime;
            }
        }
    }
}
