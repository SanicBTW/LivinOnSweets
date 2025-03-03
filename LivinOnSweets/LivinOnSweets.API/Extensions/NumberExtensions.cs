namespace LivinOnSweets.API.Extensions
{
    public static class NumberExtensions
    {
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
