using osu.Framework.Graphics;
using osu.Framework.Screens;

namespace LivinOnSweets.API.Screens
{
    // ScreenStack that contains another stack on top, acting like substates?
    public partial class SweetScreenStack : ScreenStack
    {
        // Subscreens system
        private SweetScreenStack subStack;
        public bool IsSubStack { get; protected set; }
        public bool IsSubScreenOpen => !IsSubStack && subStack.CurrentScreen != null;

        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        public SweetScreenStack(bool isSubStack = false) : base(false)
        {
            IsSubStack = isSubStack;
            if (!isSubStack)
                InternalChild = subStack = new SweetScreenStack(true) { Depth = -1, RelativeSizeAxes = Axes.Both };
        }

        public void PushSynchronously(SweetScreen screen)
        {
            LoadComponent(screen);
            Push(screen);
        }

        public void PushSubScreen(SweetSubScreen subScreen)
        {
            // Throw an exception?
            if (IsSubStack || IsSubScreenOpen)
                return;

            subStack.Push(subScreen);
        }

        // If the SubScreen doesn't call exit on themselves (ScreenStack.Exit) you can use this on the parent screen which spawned the subscreen
        public void ExitSubScreen()
        {
            // If its a sub stack do not call sub stack exit since the sub stack is null
            if (IsSubStack || !IsSubScreenOpen)
                return;

            subStack.Exit();
        }
    }
}
