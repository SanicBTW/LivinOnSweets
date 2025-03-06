using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Data.Song;
using LivinOnSweets.API.Stores;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace LivinOnSweets.API.Sprites
{
    // Uses SongMetadata to be able to build up the album cover with all of its elements
    public partial class AlbumSprite : AutoSizeOnceContainer
    {
        public const double TRANSITION_DELAY = 450D;

        public readonly SongMetadata Metadata;

        private DrawableTrack previewTrack;
        private Sprite cover;

        private Sprite disc;
        private bool positioned; // Quick flag to reposition the disc only once
        private float discOutPosition => (disc.DrawWidth / 4) + (disc.DrawWidth / 6.5F);
        public bool DiscSpinning { get; protected set; }

        // I spent 2 hours trying to get the movement working, I'm going fnf way now
        // In a couple of minutes I got the fnf alphabet movement working, fuck you
        public int TargetY = 0;
        public Vector2 StartPosition = Vector2.Zero;
        public Vector2 DistancePerItem = new(-124, 32);

        public AlbumSprite(SongMetadata songMeta) : base(Axes.Both)
        {
            AutoSizeAxes = Axes.Both;

            Metadata = songMeta;
        }

        // we using pixel art texture store to add the textures to a texture atlas
        [BackgroundDependencyLoader]
        private void load(PixelArtTextureStore pixArtStore, ITrackStore trackStore)
        {
            // Should I do it like osu!lazer? it seems to create or add? the track when the beatmap is selected, I should look into it
            previewTrack = new DrawableTrack(trackStore.Get($"Songs/{Metadata.Id}/{Metadata.Preview!.Audio}"));
            previewTrack.Looping = true;

            // Instead of using the metadata, refer to the song user save
            string coverAsset = Metadata.Story!.Locked ? Metadata.Album!.Lock : Metadata.Album!.Cover;
            Texture coverTexture = pixArtStore.Get($"MainMenu/UI/Select/Albums/{coverAsset}");
            Texture discTexture = pixArtStore.Get($"MainMenu/UI/Select/Albums/{Metadata.Album!.Disc}");
            coverTexture.ScaleAdjust = discTexture.ScaleAdjust = 1;

            cover = new Sprite()
            {
                Texture = coverTexture,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft
            };

            disc = new Sprite()
            {
                Texture = discTexture,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };

            AddRange([
                previewTrack,
                disc,
                cover,
            ]);

            Anchor = Origin = Anchor.CentreLeft;
            Margin = new MarginPadding() { Left = 62 };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Set the disc on the future position to make auto sizing do the math for the modified container
            disc.X = discOutPosition;
            DistancePerItem.Y += DrawHeight;
        }

        protected override void UpdateAfterAutoSize()
        {
            base.UpdateAfterAutoSize();

            if (positioned)
                return;

            positioned = true;
            ScheduleAfterChildren(() => HideDisc(true));
        }

        public void ApplyPosition(bool animated = true)
        {
            float xPos = (TargetY * DistancePerItem.X);
            float yPos = (TargetY * DistancePerItem.Y);
            Vector2 pos = new Vector2(xPos, yPos) + StartPosition;
            double delay = animated ? TRANSITION_DELAY : 0D;

            this.MoveTo(pos, delay, Easing.OutQuint);
        }

        public void ShowDisc(bool firstTime = false)
        {
            // If played for the first time delay its run
            if (firstTime)
            {
                Scheduler.AddDelayed(() => ShowDisc(), 1000D);
                return;
            }

            previewTrack.Volume.Value = 0;
            previewTrack.Start();
            this.TransformBindableTo(previewTrack.Volume, Metadata.Preview!.Volume, TRANSITION_DELAY);

            disc.MoveToX(discOutPosition, TRANSITION_DELAY, Easing.OutQuint);
            spin();
        }

        public void HideDisc(bool instant = false)
        {
            double delay = instant ? 0D : TRANSITION_DELAY;
            float hiddenPos = -(disc.DrawWidth / 4); // Hide it behind the album cover

            this.TransformBindableTo(previewTrack.Volume, 0, TRANSITION_DELAY).OnComplete(_ => previewTrack.Stop());

            disc.MoveToX(hiddenPos, delay, Easing.InQuint);
            Scheduler.AddDelayed(() =>
            {
                stopSpin();
            }, delay);
        }

        private void spin(double duration = 3800D)
        {
            if (DiscSpinning)
                return;

            // Might have to take a look into the real duration
            disc.Spin(duration, RotationDirection.Clockwise, disc.Rotation);
            DiscSpinning = true;
        }

        private void stopSpin()
        {
            if (!DiscSpinning)
                return;

            disc.RotateTo(disc.Rotation);
            DiscSpinning = false;
        }
    }
}
