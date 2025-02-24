using LivinOnSweets.API.Enums;
using LivinOnSweets.API.Stores;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osuTK.Input;

namespace LivinOnSweets.API.Sprites
{
    // Same philosophy as PixelButton
    public partial class DifficultySelector : Sprite
    {
        private const string texture_path = "MainMenu/UI/Select/Difficulties.png";

        public readonly SongDifficulty Difficulty;

        // Hold the active / inactive textures to switch
        private Texture activeTexture;
        private Texture inactiveTexture;

        // This button acts like a toggle
        private bool toggled;
        public bool State
        {
            get => toggled;
            set
            {
                toggled = value;
                Texture = value ? activeTexture : inactiveTexture;
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
            Texture = activeTexture = frames[startingFrame].Content;
            inactiveTexture = frames[nextFrame].Content;
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
        protected override void OnMouseUp(MouseUpEvent e)
        {
            Texture = toggled ? activeTexture : inactiveTexture;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            // should add some response since onclick fires after lifting the mouse
            // so the texture change is pretty much useless?
            Texture = toggled ? inactiveTexture : activeTexture;
            return true;
        }

        protected override bool OnClick(ClickEvent e)
        {
            Toggle();
            Action?.Invoke(this);
            return true;
        }
    }
}
