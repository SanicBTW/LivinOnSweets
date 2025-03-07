using LivinOnSweets.API.Stores;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Textures;

namespace LivinOnSweets.API.Sprites.Game
{
    public partial class CountdownAnimation : TextureAnimation
    {
        private double frameDuration = 320D;

        private bool fired;
        public event Action Finished;

        [BackgroundDependencyLoader]
        private void load(RhythmGameStore rhythmStore)
        {
            Anchor = Origin = Anchor.Centre;

            List<FrameData<Texture>> frames = rhythmStore.GetFrames("RhythmGame/UI/Mania/Countdown.png", frameDuration, 4, 2);
            // we not using the frame duration itself
            // after literal half hour on trying to get a delay working all i had to do was ts (this) shi
            int i = 0;
            foreach (FrameData<Texture> frame in frames)
            {
                int mult = i % 2 == 0 ? 2 : 1;
                AddFrame(frame.Content, frameDuration * mult);
                i++;
            }
            Loop = false;
        }

        protected override void Update()
        {
            base.Update();

            if (!fired && PlaybackPosition >= Duration)
            {
                Finished?.Invoke();
                fired = true;

                Hide(); // Does it make an instant hide?
                // Should be removed from the update tree once is completed, do this on playstate instead
            }
        }
    }
}
