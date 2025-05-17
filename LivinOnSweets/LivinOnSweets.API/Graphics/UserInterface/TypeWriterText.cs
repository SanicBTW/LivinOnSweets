using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace LivinOnSweets.API.Graphics.UserInterface
{
    // Should make it more modifiable, for now its gonna be basic, until I reach the novel stuff
    public partial class TypeWriterText : CompositeDrawable
    {
        private static LocalisableString emptyStr => new("");
        private static FontUsage font => new(family: "GyeonggiTitle", size: 24F);

        [Resolved]
        protected LocalisationManager Localisation { get; private set; }

        private TextFlowContainer text;

        private string endText = "";
        private int currentIndex;

        private double lastTime;

        // now its able to change the speed of the text, havent checked it too deeply but it should work properly
        public BindableDouble Speed = new(50D);

        private readonly BindableBool isWriting = new();
        public bool IsFinished => !isWriting.Value;

        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        public new Axes AutoSizeAxes
        {
            get => base.AutoSizeAxes;
            init => base.AutoSizeAxes = value;
        }

        public TypeWriterText()
        {
            AutoSizeAxes = Axes.Both;
            AutoSizeDuration = 500F;
            AutoSizeEasing = Easing.OutQuint;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = text = new TextFlowContainer(spr => spr.Font = font) { AutoSizeAxes = AutoSizeAxes, RelativeSizeAxes = RelativeSizeAxes };
        }

        public void Start(string typeText, double speed = 50D)
        {
            endText = typeText;
            setup(speed);
        }

        public void Start(LocalisableString localisedText, double speed = 50D)
        {
            endText = Localisation.GetLocalisedString(localisedText);
            setup(speed);
        }

        protected override void Update()
        {
            base.Update();

            if (!isWriting.Value || currentIndex >= endText.Length)
                return;

            double now = Clock.CurrentTime;

            while (now - lastTime >= Speed.Value && currentIndex < endText.Length)
            {
                text.AddText(endText[currentIndex].ToString());
                currentIndex++;
                lastTime += Speed.Value;
            }

            if (currentIndex >= endText.Length)
                isWriting.Toggle();
        }

        private void setup(double speed = 50D)
        {
            currentIndex = 0;

            text.Text = emptyStr;
            Speed.Default = speed; // In case of switching speeds in the middle of writing, easy come back
            Speed.Value = speed;
            isWriting.Toggle();

            lastTime = Clock.CurrentTime;
        }
    }
}
