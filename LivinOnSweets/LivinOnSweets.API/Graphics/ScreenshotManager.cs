using JetBrains.Annotations;
using LivinOnSweets.API.Audio;
using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Graphics.Containers;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.Skinning;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osuTK;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LivinOnSweets.API.Graphics
{
    // Bruh at this point I'm just copying over osu!lazers folder structure :sob:
    // Using osu!lazer code mixed with mine from the master LivinOnSweets branch
    // The reason why this isn't a component like lazer, its because I use some funny effects for the screenshot
    // (just a simple flash) so we need to use a container to avoid having to manipulate other containers too frequently
    public partial class ScreenshotManager : CompositeDrawable, IKeyBindingHandler<ManiaAction>, IHandleGlobalKeyboardInput
    {
        private static readonly object filename_reservation_lock = new();
        private static volatile int screenshotTasks;

        private readonly BindableBool cursorVisibility = new(true);

        /// <summary>
        /// Changed when screenshots are being or have finished being taken, to control whether cursors should be visible.
        /// If cursors should not be visible, cursors have 3 frames to hide themselves.
        /// </summary>
        public IBindable<bool> CursorVisibility => cursorVisibility;

        [Resolved]
        private SessionConfig sessionConfig { get; set; }

        private Storage screenshotsStorage;
        [CanBeNull] private ISample shutter;

        private readonly Box flash;
        private ScreenshotSprite lastScreenshot;

        public ScreenshotManager()
        {
            RelativeSizeAxes = Axes.Both;
            AddInternal(flash = new Box
            {
                Alpha = 0,
                Colour = Colour4.White,
                RelativeSizeAxes = Axes.Both,
                Depth = -1
            });
        }

        [BackgroundDependencyLoader]
        private void load(Storage storage, ResourcePackManager packManager)
        {
            screenshotsStorage = storage.GetStorageForDirectory("screenshots");

            // ui related stuff is under the sugar rush resource pack
            ResourcePack sugarPack = packManager.GetPackById(ResourcePackManager.OFFICIAL_RESOURCE_PACKS[0]);
            if (sugarPack == null)
            {
                packManager.Logger.Add($"Failed to retrieve {ResourcePackManager.OFFICIAL_RESOURCE_PACKS[0]}, did it get loaded correctly?");
                return;
            }

            shutter = sugarPack.GetSample(new ShutterAudioInfo());

            sessionConfig.BindWith(SessionSetting.ScreenshotCursorVisibility, cursorVisibility);
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            if (e.Action != ManiaAction.SCREENSHOT || e.Repeat)
                return false;

            if (lastScreenshot != null)
            {
                flash.FinishTransforms();
                lastScreenshot.FinishTransforms();
                disposeSprite(lastScreenshot);
                lastScreenshot = null;
            }

            shutter?.Play();
            ScheduleAfterChildren(takeScreenshot);

            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }

        private void takeScreenshot()
        {
            ScreenshotSprite ss = new ScreenshotSprite(DrawSize, ref screenshotsStorage);
            lastScreenshot = ss;
            AddInternal(ss);

            sessionConfig.SetValue(SessionSetting.ShowingScreenshot, true);

            // Delayed the animations to give the screenshot some time, if run too soon, the flash might hide everything in the screen
            flash.Delay(100).FadeInFromZero().FadeOut(350);
            ss.Delay(100).ScaleTo(Vector2.One, 550D, Easing.OutQuint).Then().OnComplete(_ =>
            {
                ss.Delay(2000D).MoveToY(-(ss.DrawHeight + ss.Margin.TotalVertical), 850D, Easing.OutQuint).OnComplete(spr =>
                {
                    sessionConfig.SetValue(SessionSetting.ShowingScreenshot, false);
                    disposeSprite(spr);
                });
            });
        }

        private void disposeSprite(ScreenshotSprite spr)
        {
            spr.Texture?.Dispose();
            RemoveInternal(spr, true);
        }

        private partial class ShutterAudioInfo : IAudioInfo
        {
            IEnumerable<string> IAudioInfo.LookupNames => ["UserInterface/Samples/shutter.mp3"];

            int IAudioInfo.Volume => 100;
        }

        private partial class ScreenshotSprite : CompositeDrawable
        {
            // We need to wait for at most 3 draw nodes to be drawn, following which we can be assured at least one DrawNode has been generated/drawn with the set value
            private const int frames_to_wait = 3;

            private const int jpeg_quality = 92;

            [Resolved] private GameHost host { get; set; }

            [Resolved] private Clipboard clipboard { get; set; }

            [Resolved] private IRenderer renderer { get; set; }

            [Resolved] private SweetConfigManager config { get; set; }

            [Resolved] private SessionConfig sessionConfig { get; set; }

            [Resolved] private GameOverlaysContainer overlays { get; set; }

            private readonly Sprite spr;

            private BindableBool cursorVisibility = new(true);

            [CanBeNull]
            public Texture Texture
            {
                get => spr.Texture;
                private set => spr.Texture = value;
            }

            // Its resolved on the manager and passed in as a ref
            private readonly Storage storage;

            public ScreenshotSprite(Vector2 drawSize, ref Storage storage, float scaleFactor = 4)
            {
                this.storage = storage;

                Size = drawSize / scaleFactor;
                Scale = new Vector2(scaleFactor);

                Anchor = Origin = Anchor.TopRight;
                Margin = new MarginPadding(8);

                InternalChild = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    BorderThickness = 2.5F,
                    // Used a tool to get the dominant color in the image, I could bind the color to use the accent of something in the screen, could be nice ngl
                    BorderColour = Colour4.FromHex("#719bab"),
                    CornerRadius = 12,
                    Masking = true,
                    Child = spr = new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                sessionConfig.BindWith(SessionSetting.ScreenshotCursorVisibility, cursorVisibility);
                takeScreenshotAsync();
            }

            private void takeScreenshotAsync() => Task.Run(async () =>
            {
                Interlocked.Increment(ref screenshotTasks);

                try
                {
                    // Hides the overlays and the last screenshot if showing it currently
                    waitToHide();

                    // Get the screenshot pixels
                    Image<Rgba32> image = await host.TakeScreenshotAsync().ConfigureAwait(false);
                    if (image == null) return;

                    // Rescale if it targets everything
                    if (config.Get<ScalingMode>(SweetSetting.Scaling) == ScalingMode.Everything)
                        rescaleImage(image);

                    // Set the image on the clipboard
                    clipboard.SetImage(image);

                    // If saving screenshots is enabled save it
                    if (config.Get<bool>(SweetSetting.SaveScreenshots))
                        saveImage(image);

                    // This has to be the last step since uploading the image pixels marks them as disposed
                    scheduleOnUpdate(img => uploadScreenshot(img), image);
                }
                finally
                {
                    if (Interlocked.Decrement(ref screenshotTasks) == 0)
                        showHidden();
                }
            });

            private void waitToHide()
            {
                // Hide overlays if needed
                bool hideOverlays = config.Get<bool>(SweetSetting.HideOverlaysOnScreenshot);

                if (hideOverlays)
                    scheduleOnUpdate(overlays.Hide);

                bool captureCursor = config.Get<bool>(SweetSetting.ScreenshotCaptureCursor);

                // Nothing to wait for if not showing anything on top of the content to hide
                bool showingScreenshot = sessionConfig.Get<bool>(SessionSetting.ShowingScreenshot);
                if (!showingScreenshot && !hideOverlays && captureCursor)
                    return;

                if (!captureCursor)
                    cursorVisibility.Value = false;

                int framesWaited = 0;

                using ManualResetEventSlim framesWaitedEvent = new ManualResetEventSlim(false);
                ScheduledDelegate waitDelegate = host.DrawThread.Scheduler.AddDelayed(() =>
                {
                    if (framesWaited++ >= frames_to_wait)
                        // ReSharper disable once AccessToDisposedClosure
                        framesWaitedEvent.Set();
                }, 10, true);

                // I'm not using FireAndForget (its an extension) so this can crash the game
                if (!framesWaitedEvent.Wait(1000))
                    throw new TimeoutException("Screenshot data did not arrive in a timely fashion");

                waitDelegate.Cancel();
            }

            private void showHidden()
            {
                bool hideOverlays = config.Get<bool>(SweetSetting.HideOverlaysOnScreenshot);
                if (hideOverlays)
                    scheduleOnUpdate(overlays.Show);

                bool captureCursor = config.Get<bool>(SweetSetting.ScreenshotCaptureCursor);
                if (!captureCursor)
                    cursorVisibility.Value = true;
            }

            private void rescaleImage(in Image<Rgba32> image)
            {
                float posX = config.Get<float>(SweetSetting.ScalingPositionX);
                float posY = config.Get<float>(SweetSetting.ScalingPositionY);
                float sizeX = config.Get<float>(SweetSetting.ScalingSizeX);
                float sizeY = config.Get<float>(SweetSetting.ScalingSizeY);

                image.Mutate(m =>
                {
                    Rectangle rect = new Rectangle(Point.Empty, m.GetCurrentSize());

                    // Reduce size by user scale settings...
                    int sx = (rect.Width - (int)(rect.Width * sizeX)) / 2;
                    int sy = (rect.Height - (int)(rect.Height * sizeY)) / 2;
                    rect.Inflate(-sx, -sy);

                    // ...then adjust the region based on their positional offset.
                    rect.X = (int)(rect.X * posX) * 2;
                    rect.Y = (int)(rect.Y * posY) * 2;

                    m.Crop(rect);
                });
            }

            // Loads the image pixels into a texture
            private void uploadScreenshot(in Image<Rgba32> image)
            {
                Texture = renderer.CreateTexture(image.Width, image.Height, true);
                Texture?.SetData(new TextureUpload(image));
            }

            private void saveImage(in Image<Rgba32> image)
            {
                ScreenshotFormat screenshotFormat = config.Get<ScreenshotFormat>(SweetSetting.ScreenshotFormat);

                // Get the file writable stream
                (string filename, Stream stream) = getWritableStream(screenshotFormat);

                // If no filename is available return
                if (filename == null) return;

                using (stream)
                {
                    switch (screenshotFormat)
                    {
                        case ScreenshotFormat.Png:
                            image.SaveAsPngAsync(stream).ConfigureAwait(false);
                            break;

                        case ScreenshotFormat.Jpg:
                            image.SaveAsJpegAsync(stream, new JpegEncoder { Quality = jpeg_quality, })
                                .ConfigureAwait(false);
                            break;

                        default:
                            throw new InvalidOperationException($"Unknown enum member {nameof(ScreenshotFormat)} {screenshotFormat}.");
                    }
                }
            }

            // The reason why we schedule on the update thread is because the mutation is only allowed there
            // thus transforms and all have to run on the update thread, and because we are running inside an async thread
            // (doesnt matter if its async or not, we are not inside the framework threads) we have to schedule the operation
            // to mutate the sprite in the update thread so it works correctly
            private void scheduleOnUpdate(Action action) => host.UpdateThread.Scheduler.Add(action);
            private void scheduleOnUpdate<T>(Action<T> action, T data) => host.UpdateThread.Scheduler.Add(action, data);

            // Saving utils, literally copied over from lazer
            private (string filename, Stream stream) getWritableStream(ScreenshotFormat format)
            {
                lock (filename_reservation_lock)
                {
                    DateTime dt = DateTime.Now;
                    string fileExt = format.ToString().ToLowerInvariant();

                    string withoutIndex = $"livinonsweets_{dt:yyyy-MM-dd_HH-mm-ss}.{fileExt}";
                    if (!storage.Exists(withoutIndex))
                        return (withoutIndex, storage.GetStream(withoutIndex, FileAccess.Write, FileMode.Create));

                    for (ulong i = 1; i < ulong.MaxValue; i++)
                    {
                        string indexedName = $"livinonsweets_{dt:yyyy-MM-dd_HH-mm-ss}-{i}.{fileExt}";
                        if (!storage.Exists(indexedName))
                            return (indexedName, storage.GetStream(indexedName, FileAccess.Write, FileMode.Create));
                    }

                    return (null, null);
                }
            }
        }
    }
}
