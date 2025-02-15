using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Stores;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osuTK;

namespace LivinOnSweets.API.Sprites
{
    // Represents the menu entries (Play, Option, Story) with the characters in the og game
    // TODO: Rewrite or look for the source of lag, probably its due to the huge size of the containers, I don't really know
    public partial class CharacterParallaxBackground : Container
    {
        private const string character_path = "MainMenu/Characters";
        private const string play_chars_path = $"{character_path}/Play";
        private const string options_chars_path = $"{character_path}/Options";
        private const string story_chars_path = $"{character_path}/Story";

        public readonly string BackgroundImage;
        public readonly MainMenuEntry TargetEntry;

        public bool FinishedTransform => Precision.AlmostEquals(LatestTransformEndTime - Time.Current, 0);

        public double SlideTime = 700D;
        public Easing SlideEase = Easing.InOutCubic;

        private Sprite background;
        private Container<CharacterTracker> characters; // The character container inside the update tree
        private EntryName entry;

        public CharacterParallaxBackground(MainMenuEntry targetEntry)
        {
            TargetEntry = targetEntry;
            BackgroundImage = targetEntry.GetDescription();

            Anchor = Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(MainMenuStore mmStore)
        {
            AddRangeInternal([
                background = new Sprite()
                {
                    Texture = mmStore.Get(BackgroundImage),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                // No width for ya guys, even if it looks weird, its for the greater good of the current layout
                // lmao jk TODO! Fix the characters container not having a width set since it breaks everything else and also breaks sliding animations
                characters = new Container<CharacterTracker>()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Y,
                    Depth = -1,
                },
                entry = new EntryName(TargetEntry, this)
            ]);

            if (TargetEntry == MainMenuEntry.OPTION)
            {
                // the "light" that can be seen on the options slide
                Add(new Sprite()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = mmStore.Get("MainMenu/UI/Options/SlideOverlay.png"),
                    Blending = BlendingParameters.Additive
                });
            }

            if (TargetEntry == MainMenuEntry.STORY)
            {
                // The shadow of the hands of kazusa
                Add(new Sprite()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = mmStore.Get($"{story_chars_path}/KazusaShadow.png"),
                    // Apparently this is the multiply blending, chatgpt gave me the correct arguments except destination
                    // which would be inherit, now it renders correctly
                    Blending = new BlendingParameters()
                    {
                        Source = BlendingType.DstColor,
                        Destination = BlendingType.Inherit,
                        SourceAlpha = BlendingType.One,
                        DestinationAlpha = BlendingType.Zero,
                        RGBEquation = BlendingEquation.Add,
                        AlphaEquation = BlendingEquation.Add,
                    },
                });
            }

            AddBackCharacters();
            AddFrontCharacters();
        }

        /*
         * Play: K, Y (Front) | A, N (Back)
         * Story: N, Y (Front) | A, K (Back)
         * Option: A (Front 1) N (Middle 2) Y (Back 1) K (Back 2)
         */

        protected virtual void AddBackCharacters()
        {
            switch (TargetEntry)
            {
                case MainMenuEntry.PLAY:
                    characters.Add(new CharacterTracker(this,  TargetEntry, Students.AIRI, true));
                    characters.Add(new CharacterTracker(this,  TargetEntry, Students.NATSU, true));
                    break;

                case MainMenuEntry.STORY:
                    characters.Add(new CharacterTracker(this,  TargetEntry, Students.KAZUSA, true));
                    characters.Add(new CharacterTracker(this,  TargetEntry, Students.AIRI, true));
                    break;

                case MainMenuEntry.OPTION:
                    characters.Add(new CharacterTracker(this,  TargetEntry, Students.YOSHIMI, true));
                    characters.Add(new CharacterTracker(this,  TargetEntry, Students.KAZUSA, false));
                    break;
            }
        }

        protected virtual void AddFrontCharacters()
        {
            switch (TargetEntry)
            {
                case MainMenuEntry.PLAY:
                    characters.Add(new CharacterTracker(this,  TargetEntry, Students.KAZUSA, false));
                    characters.Add(new CharacterTracker(this,  TargetEntry, Students.YOSHIMI, false));
                    break;

                case MainMenuEntry.STORY:
                    characters.Add(new CharacterTracker(this,  TargetEntry, Students.NATSU, false));
                    break;

                case MainMenuEntry.OPTION:
                    characters.Add(new CharacterTracker(this,  TargetEntry, Students.NATSU, false));
                    characters.Add(new CharacterTracker(this,  TargetEntry, Students.AIRI, true));
                    break;
            }
        }

        public void SlideIn()
        {
            this.MoveToX(0, SlideTime, SlideEase);
        }

        // slideeee to the left, slideeee to the right, criss cross
        public void SlideOut(bool slideLeft)
        {
            this.MoveToX(background.DrawWidth * (slideLeft ? -1 : 1), SlideTime, SlideEase);
        }

        // This prepares the background position offscreen, mostly used to position the third background when changing selection
        public void SlideOffscreen(bool wasSlideLeft)
        {
            this.MoveToX(wasSlideLeft ? background.DrawWidth : -background.DrawWidth);

            entry.SetShadowsPositions(Position);
            foreach (CharacterTracker charTrack in characters)
            {
                charTrack.MoveTo(Position);
            }
        }

        // One hour later, all i had to track was the background, not the fg sprite...
        private partial class EntryName : Container
        {
            private const string path = "MainMenu/UI/Slides";

            public readonly string Label; // The target entry string we representing
            public string BackgroundTexture => $"{path}/{Label}BG"; // The background texture path of the label
            public string ForegroundTexture => $"{path}/{Label}FG"; // The foreground texture path of the label

            private CharacterParallaxBackground parallaxBackground;
            private Sprite fgSpr;
            private Container<PositionTrackerSprite> shadows;

            public EntryName(MainMenuEntry entry, CharacterParallaxBackground background)
            {
                // Kind of accurate position relative to the OG version :grin:
                Margin = new MarginPadding() { Bottom = -36, Left = 16 };
                Depth = -2;

                Label = entry switch
                {
                    MainMenuEntry.PLAY => "Play",
                    MainMenuEntry.OPTION => "Option",
                    MainMenuEntry.STORY => "Story",
                    _ => ""
                };

                Anchor = Origin = Anchor.BottomCentre;
                parallaxBackground = background;

                // Create the sprite and set the origin & anchor here
                fgSpr = new Sprite();
                fgSpr.Anchor = fgSpr.Origin = entry switch
                {
                    MainMenuEntry.PLAY => Anchor.BottomCentre,
                    MainMenuEntry.OPTION => Anchor.BottomRight,
                    MainMenuEntry.STORY => Anchor.BottomLeft,
                    _ => Anchor.TopLeft
                };
                fgSpr.MoveToOffset(new Vector2(-8));

                shadows = new Container<PositionTrackerSprite>()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                };

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textureStore)
            {
                Texture bgTex = textureStore.Get(BackgroundTexture);
                Texture fgTex = textureStore.Get(ForegroundTexture);

                // Since we are gathering from the global texture store which has a scale adjust of 2,
                // we need to set it to 1 to each of the sprites added to this container
                bgTex.ScaleAdjust = fgTex.ScaleAdjust = 1;

                // Set the sprite texture here
                fgSpr.Texture = fgTex;

                GenerateShadows(bgTex);

                AddRange([
                    shadows,
                    fgSpr
                ]);
            }

            protected virtual void GenerateShadows(Texture bgTex)
            {
                shadows.AddRange([
                    new PositionTrackerSprite(parallaxBackground, 50)
                    {
                        Texture = bgTex,
                        Anchor = fgSpr.Anchor,
                        Origin = fgSpr.Origin,
                    },
                    new PositionTrackerSprite(parallaxBackground, 100)
                    {
                        Texture = bgTex,
                        Anchor = fgSpr.Anchor,
                        Origin = fgSpr.Origin,
                    },
                    new PositionTrackerSprite(parallaxBackground, 150)
                    {
                        Texture = bgTex,
                        Anchor = fgSpr.Anchor,
                        Origin = fgSpr.Origin,
                    }
                ]);
            }

            public void SetShadowsPositions(Vector2 newPos)
            {
                foreach (PositionTrackerSprite tracker in shadows)
                {
                    tracker.MoveTo(newPos);
                }
            }
        }

        private partial class CharacterTracker : PositionTrackerSprite
        {
            private MainMenuEntry entry;
            private Students student;
            private string imagePath;

            public CharacterTracker(CharacterParallaxBackground track, MainMenuEntry targetEntry, Students targetStudent, bool goesLeft)
                : base(track, 0D, Easing.InOutCubic, v => goesLeft ? -v : v)
            {
                if (targetStudent == Students.RANDOM)
                    throw new InvalidOperationException();

                entry = targetEntry;
                student = targetStudent;

                string studentImage = targetStudent.GetDescription();

                imagePath = targetEntry switch
                {
                    MainMenuEntry.PLAY => $"{play_chars_path}/{studentImage}",
                    MainMenuEntry.OPTION => $"{options_chars_path}/{studentImage}",
                    _ => null
                };

                if (imagePath == null)
                {
                    switch (targetEntry)
                    {
                        case MainMenuEntry.STORY:
                            if (targetStudent == Students.AIRI || targetStudent == Students.KAZUSA)
                            {
                                imagePath = $"{story_chars_path}/{studentImage}";
                                return;
                            }

                            imagePath = $"{story_chars_path}/NatsuYoshimi.png";
                            break;
                    }
                }
            }

            [BackgroundDependencyLoader]
            private void load(MainMenuStore mmStore)
            {
                Texture = mmStore.Get(imagePath);
            }

            // Manual adjustments my friends
            protected override void LoadComplete()
            {
                base.LoadComplete();

                Anchor = Origin = Anchor.Centre;
                if (imagePath.Contains("NatsuYoshimi"))
                {
                    Origin = Anchor.CentreRight;
                    Anchor = Origin.Opposite();

                    float target = DrawWidth / 4;
                    Margin = new MarginPadding() { Right = -target };
                }

                if (entry == MainMenuEntry.STORY && student == Students.AIRI)
                {
                    Origin = Anchor.CentreLeft;
                    Anchor = Origin.Opposite();

                    float half = DrawWidth / 2;
                    float targetHalf = DrawWidth / 2.5f;
                    float diff = half - targetHalf;
                    Margin = new MarginPadding() { Left = -diff };
                }

                if (entry == MainMenuEntry.OPTION)
                {
                    float half = DrawWidth / 2;

                    switch (student)
                    {
                        case Students.NATSU:
                            Origin = Anchor.CentreRight;
                            Anchor = Origin.Opposite();

                            float quint = DrawWidth / 5f;
                            float nDiff = half - quint;
                            Margin = new MarginPadding() { Right = -nDiff };
                            break;

                        case Students.KAZUSA:
                            Origin = Anchor.CentreLeft;
                            Anchor = Origin.Opposite();

                            // 4.2 is a magic number, i figured it out using Figma and my basic maths :pray:
                            float quartsec = half / 4.2f;
                            float kDiff = half - quartsec;
                            Margin = new MarginPadding() { Left = -kDiff };
                            break;
                    }
                }
            }
        }
    }
}
