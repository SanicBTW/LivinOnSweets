using Newtonsoft.Json;

namespace LivinOnSweets.API.Data.Dialog
{
    // Represents the JSON file structure, used for deserialization
    // This is the ENTRY point of the JSON
    public struct PositionsJSON
    {
        [JsonProperty("positions")]
        public CharactersJSON[] Positions;
    }

    // Represents the JSON object inside the positions array
    // Kind of lame structure if you ask me but the nesting is needed
    // To continue alongside the dialogue without any tricks or sum
    // So it would be like dialogPos[curDialog]
    public struct CharactersJSON
    {
        [JsonProperty("characters")]
        public DialogCharacterPosition[] Characters;
    }
}
