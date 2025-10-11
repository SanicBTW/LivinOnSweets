using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using OAnchor = osu.Framework.Graphics.Anchor; // bruh
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global

namespace LivinOnSweets.API.Data
{
    // im kinda new to this attribute, gotta check it more in depth
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class MenuEntryInfo
    {
        // i wanna say THIS is my first time making such a complex and almost full toml structure
        // which adds the ability to modify something in game and im surprised it works
        public InheritanceInfo Inheritance { get; set; } = new();
        public List<EntryInfo> Entries { get; set; } = [];

        public class InheritanceInfo
        {
            public string Id { get; set; } = "";
            public bool Enabled { get; set; } = false;
        }

        public class EntryInfo
        {
            // this should be used in order to recognize the action to fire inside main menu screen, not the best way honestly
            public string Id { get; set; } = "";
            public string Background { get; set; } = "";
            public LabelInfo Label { get; set; } = new();
            public List<CharacterInfo> Characters { get; set; } = [];
            public List<SlideSprite> Extras { get; set; } = [];

            public void Populate(EntryInfo them)
            {
                if (string.IsNullOrEmpty(Background))
                    Background = them.Background;

                Label.Populate(them.Label);

                foreach (CharacterInfo parentCharacter in them.Characters)
                {
                    // search for the existing character with the same student name
                    CharacterInfo existing = Characters.Find(c =>
                        string.Equals(c.Student, parentCharacter.Student, StringComparison.OrdinalIgnoreCase));

                    // if it exists we do the inheritance if not we add it
                    if (existing != null)
                        existing.Populate(parentCharacter);
                    else
                        Characters.Add(parentCharacter);
                }

                foreach (SlideSprite parentEffect in them.Extras)
                {
                    SlideSprite existing = Extras.Find(e =>
                        string.Equals(e.Id, parentEffect.Id, StringComparison.OrdinalIgnoreCase));

                    if (existing != null)
                        existing.Populate(parentEffect);
                    else
                        Extras.Add(parentEffect);
                }
            }
        }

        public class LabelInfo
        {
            public string ForegroundTexture { get; set; } = "";
            public string ShadowTexture { get; set; } = "";
            public int Shadows { get; set; } = 3;
            public float ShadowOffset { get; set; } = 8;
            public string Anchor { get; set; } = "BottomCentre";
            public List<int> Depths { get; set; } = [];

            public void Populate(LabelInfo them)
            {
                if (string.IsNullOrEmpty(ForegroundTexture))
                    ForegroundTexture = them.ForegroundTexture;

                if (string.IsNullOrEmpty(ShadowTexture))
                    ShadowTexture = them.ShadowTexture;

                if (Shadows == 3)
                    Shadows = them.Shadows;

                if (string.IsNullOrEmpty(Anchor) || Anchor == "BottomCentre")
                    Anchor = them.Anchor;

                if (Depths.Count == 0 && them.Depths.Count > 0)
                    Depths = them.Depths;
            }
        }

        public class TomlSprite<T> where T : TomlSprite<T>
        {
            // for reference lookup in population
            public string Id { get; set; } = "";
            public string Texture { get; set; } = "";
            public float X { get; set; }
            public float Y { get; set; }
            public string Anchor { get; set; } = "TopLeft";
            public int Depth { get; set; }
            public CustomBlendingInfo Blending { get; set; } = new();
            public TextureUploadInfo TextureUpload { get; set; } = new();

            public virtual void Populate(T them)
            {
                if (string.IsNullOrEmpty(Id))
                    Id = them.Id;

                if (string.IsNullOrEmpty(Texture))
                    Texture = them.Texture;

                if (X == 0)
                    X = them.X;

                if (Y == 0)
                    Y = them.Y;

                if (string.IsNullOrEmpty(Anchor) || Anchor == "TopLeft")
                    Anchor = them.Anchor;

                if (Depth == 0)
                    Depth = them.Depth;

                Blending.Populate(them.Blending);
                TextureUpload.Populate(them.TextureUpload);
            }
        }

        // default generic implementation
        public class TomlSprite : TomlSprite<TomlSprite>
        {
            // i should make err an interface which implements this lol
            public static OAnchor ParseAnchor(string value) => value.ToLowerInvariant() switch
            {
                "bottomcentre" or "bottom_center" => OAnchor.BottomCentre,
                "bottomleft" or "bottom_left" => OAnchor.BottomLeft,
                "bottomright" or "bottom_right" => OAnchor.BottomRight,

                "topcentre" or "top_center" => OAnchor.TopCentre,
                "topleft" or "top_left" => OAnchor.TopLeft,
                "topright" or "top_right" => OAnchor.TopRight,

                "centre" or "center" => OAnchor.Centre,
                "centreleft" or "center_left" => OAnchor.CentreLeft,
                "centreright" or "center_right" => OAnchor.CentreRight,

                _ => OAnchor.BottomCentre // defaults to bottom center
            };
        }

        public class SlideSprite<T> : TomlSprite<T> where T : SlideSprite<T>
        {
            public int SlideMult { get; set; }

            public override void Populate(T them)
            {
                base.Populate(them);

                if (SlideMult == 0)
                    SlideMult = them.SlideMult;
            }
        }

        public class SlideSprite : SlideSprite<SlideSprite> { }

        public class CharacterInfo : SlideSprite<CharacterInfo>
        {
            // honestly its not used anywhere but im keeping it in order to know which character im adding
            // lowkey i could use comments but no one really reads em atp
            public string Student { get; set; } = "KAZUSA";

            // THIS is used instead of SlideMult just so you know
            public bool SlideLeft { get; set; }

            public override void Populate(CharacterInfo them)
            {
                base.Populate(them);

                // i need to check this
                if (!SlideLeft && them.SlideLeft)
                    SlideLeft = true;

                if (string.IsNullOrEmpty(Student) || Student == "KAZUSA")
                    Student = them.Student;
            }
        }

        public class CustomBlendingInfo
        {
            public string Type { get; set; } = "inherit";
            public string Source { get; set; } = "";
            public string Destination { get; set; } = "";
            public string SourceAlpha { get; set; } = "";
            public string DestinationAlpha { get; set; } = "";
            public string RgbEquation { get; set; } = "";
            public string AlphaEquation { get; set; } = "";

            public void Populate(CustomBlendingInfo them)
            {
                if (string.Equals(Type, "inherit", StringComparison.OrdinalIgnoreCase))
                    Type = them.Type;

                if (string.IsNullOrEmpty(Source))
                    Source = them.Source;

                if (string.IsNullOrEmpty(Destination))
                    Destination = them.Destination;

                if (string.IsNullOrEmpty(SourceAlpha))
                    SourceAlpha = them.SourceAlpha;

                if (string.IsNullOrEmpty(DestinationAlpha))
                    DestinationAlpha = them.DestinationAlpha;

                if (string.IsNullOrEmpty(RgbEquation))
                    RgbEquation = them.RgbEquation;

                if (string.IsNullOrEmpty(AlphaEquation))
                    AlphaEquation = them.AlphaEquation;
            }

            public static BlendingParameters ParseBlending(CustomBlendingInfo b)
            {
                if (string.Equals(b.Type, "additive", StringComparison.OrdinalIgnoreCase))
                    return BlendingParameters.Additive;

                // this is lowkey real UNSAFE since im not catching any crash execption for a miss type so uhh yeah
                if (string.Equals(b.Type, "custom", StringComparison.OrdinalIgnoreCase))
                {
                    return new BlendingParameters
                    {
                        Source = Enum.Parse<BlendingType>(b.Source),
                        Destination = Enum.Parse<BlendingType>(b.Destination),
                        SourceAlpha = Enum.Parse<BlendingType>(b.SourceAlpha),
                        DestinationAlpha = Enum.Parse<BlendingType>(b.DestinationAlpha),
                        RGBEquation = Enum.Parse<BlendingEquation>(b.RgbEquation),
                        AlphaEquation = Enum.Parse<BlendingEquation>(b.AlphaEquation)
                    };
                }

                return BlendingParameters.Inherit;
            }
        }

        public class TextureUploadInfo
        {
            public WrapModeInfo WrapMode { get; set; } = new();
            public bool UseAtlas { get; set; } = true;
            public bool ManualMipmaps { get; set; }
            public string FilteringMode { get; set; } = "nearest";
            public float ScaleAdjust { get; set; } = 1;

            public void Populate(TextureUploadInfo them)
            {
                WrapMode.Populate(them.WrapMode);
                UseAtlas = them.UseAtlas;
                ManualMipmaps = them.ManualMipmaps;
                FilteringMode = them.FilteringMode;
                ScaleAdjust = them.ScaleAdjust;
            }

            public static TextureFilteringMode ParseFilteringMode(string filteringMode) =>
                filteringMode.ToLowerInvariant() switch
                {
                    "nearest" => TextureFilteringMode.Nearest,
                    _ => TextureFilteringMode.Linear
                };
        }

        public class WrapModeInfo
        {
            public string WrapHorizontal { get; set; } = "none";
            public string WrapVertical { get; set; } = "none";

            public void Populate(WrapModeInfo them)
            {
                // we aint really checking
                WrapHorizontal = them.WrapHorizontal;
                WrapVertical = them.WrapVertical;
            }

            public static WrapMode Parse(string value) => value.ToLowerInvariant() switch
            {
                "clamptoedge" or "clamp_to_edge" => WrapMode.ClampToEdge,
                "clamptoborder" or "clamp_to_border" => WrapMode.ClampToBorder,
                "repeat" => WrapMode.Repeat,
                _ => WrapMode.None
            };
        }
    }
}
