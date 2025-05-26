namespace LivinOnSweets.API.Enums
{
    /// <summary>
    /// Enum representing the available students.
    /// </summary>
    public enum Students
    {
        // Sugar rush students ranging from 0 to 3

        Natsu,

        Kazusa,

        Airi,

        Yoshimi,

        // Antique seraphim students ranging from 4 to 6

        Sakurako,

        Mari,

        Mine,

        // Extras for more personalization

        /// <summary>
        /// Value only used in <c>Sugar Rush</c> and <c>Livin on Sweets</c> updates.
        /// </summary>
        Random,

        /// <summary>
        /// Value representing its using a custom student which is not in the game files.
        /// </summary>
        Custom,
    }
}
