using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LivinOnSweets.API.Data.Dialog
{
    // Represents the characters dialog positioning, adapted from the JSON files
    public struct DialogCharacterPosition
    {
        [JsonProperty("position")]
        public int Position;

        [JsonProperty("texture")]
        public string Texture;

        [DefaultValue(false)]
        [JsonProperty("highlight", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Include)]
        public bool Highlight;

        [DefaultValue(false)]
        [JsonProperty("fadeIn", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Include)]
        public bool FadeIn;

        [DefaultValue(false)]
        [JsonProperty("surprise", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Include)]
        public bool Surprise;

        [JsonProperty("transition", NullValueHandling = NullValueHandling.Include)]
        public DialogCharacterTransition? Transition;
    }
}
