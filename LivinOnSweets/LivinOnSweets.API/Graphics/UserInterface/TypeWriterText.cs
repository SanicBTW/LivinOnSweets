using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace LivinOnSweets.API.Graphics.UserInterface
{
    // Should make it more modifiable, for now its gonna be basic, until I reach the novel stuff
    public partial class TypeWriterText : CompositeDrawable
    {
        private static FontUsage font => new(family: "GyeonggiTitle", size: 24F);

        private TextFlowContainer text;

        private string endText = "";
        private int currentIndex;

        private double timePerChar = 50;
        private double lastTime;

        private readonly BindableBool isWriting = new();
        public bool IsFinished => !isWriting.Value;

        public TypeWriterText()
        {
            AutoSizeAxes = Axes.Both;
            AutoSizeDuration = 500F;
            AutoSizeEasing = Easing.OutQuint;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = text = new TextFlowContainer(spr => spr.Font = font) { AutoSizeAxes = Axes.Both };
        }

        public void Start(string typeText, double speed = 50)
        {
            endText = typeText;
            currentIndex = 0;

            text.Text = "";
            timePerChar = speed;
            isWriting.Toggle();

            lastTime = Clock.CurrentTime;
        }

        protected override void Update()
        {
            base.Update();

            if (!isWriting.Value || currentIndex >= endText.Length)
                return;

            double now = Clock.CurrentTime;

            while (now - lastTime >= timePerChar && currentIndex < endText.Length)
            {
                text.AddText(endText[currentIndex].ToString());
                currentIndex++;
                lastTime += timePerChar;
            }

            if (currentIndex >= endText.Length)
                isWriting.Toggle();
        }
    }
}
