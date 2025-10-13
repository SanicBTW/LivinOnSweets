namespace LivinOnSweets.API.Extensions
{
    // you know i had to do it...
    // https://github.com/SanicBTW/LivinOnSweets/blob/master/LivinOnSweets/LivinOnSweets.API/Extensions/NumberExtensions.cs

    public static class NumberExtensions
    {
        /// <summary>
        /// Wraps the value around the max back to <c>0</c> and the other way around.
        /// </summary>
        /// <param name="value">The value to wrap</param>
        /// <param name="max">The max value until it wraps to <c>0</c></param>
        /// <returns>Wrapped value</returns>
        public static int Wrap(this int value, int max)
        {
            if (max <= 0)
                value = 0;

            if (value < 0)
                value = max - 1;

            if (value >= max)
                value = 0;

            return value;
        }
    }
}
