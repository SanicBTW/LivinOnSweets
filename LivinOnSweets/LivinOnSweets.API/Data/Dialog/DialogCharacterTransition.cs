using Newtonsoft.Json;

namespace LivinOnSweets.API.Data.Dialog
{
    // TODO!
    public struct DialogCharacterTransition
    {
        [JsonProperty("texture")]
        public string Texture;

        [JsonProperty("delay")]
        public float Delay;
    }
}
