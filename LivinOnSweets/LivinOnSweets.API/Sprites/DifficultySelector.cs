using LivinOnSweets.API.Enums;
using LivinOnSweets.API.Stores;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Textures;

namespace LivinOnSweets.API.Sprites
{
    // Reduced mouse events code, I should properly rewrite this but uhhhhhhhhhhhhhhh
    public partial class DifficultySelector : SpritesheetButton
    {
        private const string texture_path = "MainMenu/UI/Select/Difficulties.png";

        public readonly SongDifficulty Difficulty;

        // This button acts like a toggle
        private bool toggled;
        public bool State
        {
            get => toggled;
            set
            {
                toggled = value;
                Texture = value ? ActiveTexture : IdleTexture;
            }
        }
        public Action<DifficultySelector> Action;

        public DifficultySelector(SongDifficulty difficulty, bool startToggled = true)
        {
            toggled = startToggled;
            Difficulty = difficulty;
        }

        [BackgroundDependencyLoader]
        private void load(AnimatedPixelArtTextureStore animPixStore)
        {
            Texture spriteSheet = animPixStore.Get(texture_path);
            spriteSheet.ScaleAdjust = 1;

            List<FrameData<Texture>> frames = animPixStore.GetFrames(texture_path, 0, 1, 6);

            // next = starting + 1
            // 0 active / 1 inactive
            // 2 active / 3 inactive
            // 4 active / 5 inactive
            // 0 * 2 = 0
            // 1 * 2 = 2
            // 2 * 2 = 4
            int startingFrame = (int)Difficulty * 2;
            int nextFrame = startingFrame + 1;
            SetFrames([frames[nextFrame].Content, frames[startingFrame].Content]);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Invoke the action if the selector was toggled on creation
            if (toggled)
                Action?.Invoke(this);
        }

        public void Toggle() => State = !State;

        // if the mouse is lifted and wasn't toggled (moved away) reset the texture
        protected override Texture MouseUpTexture() => toggled ? ActiveTexture : IdleTexture;

        // should add some response since onclick fires after lifting the mouse
        // so the texture change is pretty much useless?
        protected override Texture MouseDownTexture() => toggled ? IdleTexture : ActiveTexture;

        protected override void FireEvent()
        {
            Toggle();
            Action?.Invoke(this);
        }
    }
}
