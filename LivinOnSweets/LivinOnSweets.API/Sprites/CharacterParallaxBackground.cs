using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Stores;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osuTK;

namespace LivinOnSweets.API.Sprites
{
    // Represents the menu entries (Play, Option, Story) with the characters in the og game
    public partial class CharacterParallaxBackground : SlideContainer
    {
        private const string background_path = "MainMenu/Backgrounds";
        private const string character_path = "MainMenu/Characters";
        private const string play_chars_path = $"{character_path}/Play";
        private const string options_chars_path = $"{character_path}/Options";
        private const string story_chars_path = $"{character_path}/Story";
        private const string label_path = "MainMenu/UI/Slides";

        // TODO: Move this bs to some file, I'm not too proud of it lol
        private static Dictionary<MainMenuEntry, Dictionary<Students, float>> positions = new()
        {
            [MainMenuEntry.STORY] = new Dictionary<Students, float>()
            {
                [Students.KAZUSA] = 20,
                [Students.AIRI] = 545,
                [Students.NATSU] = -330,
            },
            [MainMenuEntry.OPTION] = new Dictionary<Students, float>()
            {
                [Students.YOSHIMI] = 0,
                [Students.KAZUSA] = 155,
                [Students.NATSU] = -260,
                [Students.AIRI] = 20
            }
        };

        private static Dictionary<MainMenuEntry, Dictionary<Students, int>> charDepths = new()
        {
            [MainMenuEntry.PLAY] = new Dictionary<Students, int>()
            {
                [Students.NATSU] = 3,
                [Students.AIRI] = 6,
                [Students.YOSHIMI] = 7,
                [Students.KAZUSA] = 8,
            },
            [MainMenuEntry.OPTION] = new Dictionary<Students, int>()
            {
                [Students.YOSHIMI] = 9,
                [Students.KAZUSA] = 10,
                [Students.NATSU] = 11,
                [Students.AIRI] = 12
            },
            [MainMenuEntry.STORY] = new Dictionary<Students, int>()
            {
                [Students.KAZUSA] = 2,
                [Students.AIRI] = 4,
                [Students.NATSU] = 5,
            },
        };

        private static Dictionary<MainMenuEntry, int[]> labelDepths = new()
        {
            [MainMenuEntry.PLAY] = [18, 17, 16, 15],
            [MainMenuEntry.OPTION] = [22, 21, 20, 19],
            [MainMenuEntry.STORY] = [24, 23, 22, 21]
        };

        public readonly MainMenuEntry TargetEntry;

        public bool FinishedTransform => Precision.AlmostEquals(LatestTransformEndTime - Time.Current, 0);

        public CharacterParallaxBackground(MainMenuEntry targetEntry)
        {
            TargetEntry = targetEntry;

            // devious work actually, quick hack to hide off slide(screen) transitions, i dont know if this could kill performance but it cant be that bad right
            Masking = true;
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(MainMenuStore mmStore, TextureStore textureStore)
        {
            AddInternal(new Sprite()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Texture = mmStore.Get($"{background_path}/{TargetEntry.GetDescription()}Slide.png"),
                Depth = 99,
            });

            AddBackCharacters(mmStore);
            AddFrontCharacters(mmStore);

            // the sun shine lmao, why tf is it called slide overlay on the resources
            if (TargetEntry == MainMenuEntry.OPTION)
                AddElement(mmStore.Get("MainMenu/UI/Options/SlideOverlay.png"), -140, 1, 13, blending: BlendingParameters.Additive);

            if (TargetEntry == MainMenuEntry.STORY)
            {
                // Apparently this is the multiply blending, chatgpt gave me the correct arguments except destination
                // which would be inherit, now it renders correctly
                BlendingParameters multiplyBlending = new BlendingParameters()
                {
                    Source = BlendingType.DstColor,
                    Destination = BlendingType.Inherit,
                    SourceAlpha = BlendingType.One,
                    DestinationAlpha = BlendingType.Zero,
                    RGBEquation = BlendingEquation.Add,
                    AlphaEquation = BlendingEquation.Add,
                };

                // The shadow of the hands of kazusa
                AddElement(mmStore.Get($"{story_chars_path}/KazusaShadow.png"), 20, -1, 1, blending: multiplyBlending);
            }

            // Use the default texture store to be able to use texture atlases, since the textures wee using aint that big
            AddEntryLabel(textureStore);
        }

        protected virtual void AddBackCharacters(MainMenuStore mmStore)
        {
            (Students[], bool[], int) slideStudents = getEntryStudents(false);
            for (int i = 0; i < slideStudents.Item3; i++)
            {
                Students student = slideStudents.Item1[i];
                bool slideLeft = slideStudents.Item2[i];

                AddElement(mmStore.Get(getCharacterTexture(student)), getCharXPosition(student), slideLeft ? -1 : 1, getCharDepth(student));
            }
        }

        protected virtual void AddFrontCharacters(MainMenuStore mmStore)
        {
            (Students[], bool[], int) slideStudents = getEntryStudents(true);
            for (int i = 0; i < slideStudents.Item3; i++)
            {
                Students student = slideStudents.Item1[i];
                bool slideLeft = slideStudents.Item2[i];

                AddElement(mmStore.Get(getCharacterTexture(student)), getCharXPosition(student), slideLeft ? -1 : 1, getCharDepth(student));
            }
        }

        protected virtual void AddEntryLabel(TextureStore textureStore)
        {
            // Essentials
            string label = TargetEntry switch
            {
                MainMenuEntry.PLAY => "Play",
                MainMenuEntry.OPTION => "Option",
                MainMenuEntry.STORY => "Story",
                _ => ""
            };

            Anchor targetAnchor = TargetEntry switch
            {
                MainMenuEntry.PLAY => Anchor.BottomCentre,
                MainMenuEntry.OPTION => Anchor.BottomRight,
                MainMenuEntry.STORY => Anchor.BottomLeft,
                _ => Anchor.TopLeft
            };

            // Textures
            string foregroundTexture = $"{label_path}/{label}FG";
            string backgroundTexture = $"{label_path}/{label}BG";
            Texture fgTex = textureStore.Get(foregroundTexture);
            Texture bgTex = textureStore.Get(backgroundTexture);

            // Since we are gathering from the global texture store which has a scale adjust of 2,
            // we need to set it to 1 to each of the sprites added to this container
            fgTex.ScaleAdjust = bgTex.ScaleAdjust = 1;

            // Foreground
            SlideElement fgEl = AddElement(fgTex, -8, 0, getLabelDepth(0));
            fgEl.Y = -8;
            fgEl.Anchor = fgEl.Origin = targetAnchor;

            // 3 Shadows
            for (int i = 0; i < 3; i++)
            {
                int depthIndex = i + 1;
                double baseDelay = 50D;
                double endDelay = baseDelay + baseDelay * i;

                SlideElement bgEl = AddElement(bgTex, 0, 0, getLabelDepth(depthIndex), endDelay);
                bgEl.Anchor = bgEl.Origin = targetAnchor;
            }
        }

        protected override void UpdateAfterAutoSize()
        {
            base.UpdateAfterAutoSize();

            // This is a hack I learnt while doing the editor, check AutoSizeOnceContainer
            if (AutoSizeAxes.HasFlagFast(Axes.Both))
            {
                Vector2 prevSize = DrawSize;
                AutoSizeAxes = Axes.None;
                Size = prevSize;
            }
        }

        /*
         * Play: K, Y (Front) | A, N (Back)
         * Story: N, Y (Front) | A, K (Back)
         * Option: A (Front 1) N (Middle 2) Y (Back 1) K (Back 2)
         */
        private (Students[], bool[], int) getEntryStudents(bool front)
        {
            Students[] students = new Students[2];
            bool[] slidesLeft = new bool[2];

            switch (TargetEntry)
            {
                case MainMenuEntry.PLAY:
                    students[0] = front ? Students.KAZUSA : Students.AIRI;
                    students[1] = front ? Students.YOSHIMI : Students.NATSU;
                    slidesLeft[0] = slidesLeft[1] = !front; // If front, slide the opposite direction
                    break;

                case MainMenuEntry.STORY:
                    if (front)
                    {
                        students[0] = Students.NATSU;
                        slidesLeft[0] = false;
                    }
                    else
                    {
                        students[0] = Students.KAZUSA;
                        students[1] = Students.AIRI;
                        slidesLeft[0] = slidesLeft[1] = true;
                    }
                    break;

                case MainMenuEntry.OPTION:
                    students[0] = front ? Students.NATSU : Students.YOSHIMI;
                    students[1] = front ? Students.AIRI : Students.KAZUSA;

                    slidesLeft[0] = !front;
                    slidesLeft[1] = front;
                    break;
            }

            // students length will always match the slides left length
            return (students, slidesLeft, students.Length);
        }

        private string getCharacterTexture(Students targetStudent)
        {
            if (targetStudent == Students.RANDOM)
                throw new InvalidOperationException();

            string studentImage = targetStudent.GetDescription();

            string imagePath = TargetEntry switch
            {
                MainMenuEntry.PLAY => $"{play_chars_path}/{studentImage}",
                MainMenuEntry.OPTION => $"{options_chars_path}/{studentImage}",
                _ => null
            };

            if (imagePath == null)
            {
                switch (TargetEntry)
                {
                    case MainMenuEntry.STORY:
                        if (targetStudent == Students.AIRI || targetStudent == Students.KAZUSA)
                            imagePath = $"{story_chars_path}/{studentImage}";
                        else
                            imagePath = $"{story_chars_path}/NatsuYoshimi.png";
                        break;
                }
            }

            return imagePath;
        }

        private float getCharXPosition(Students student)
        {
            float posX = 0;
            if (positions.TryGetValue(TargetEntry, out Dictionary<Students, float> posDict))
                posX = posDict[student];

            return posX;
        }

        private int getCharDepth(Students student) => charDepths[TargetEntry][student];

        private int getLabelDepth(int index) => labelDepths[TargetEntry][index];
    }
}
