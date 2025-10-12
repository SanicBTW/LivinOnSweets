using System.Reflection;

namespace LivinOnSweets.API
{
    // Just like LivinOnSweetsResources, exposes a type to access the assembly easier
    // Its mostly used under GameBase to register Overridable Classes
    public static class LivinOnSweetsApi
    {
        public static Assembly ApiAssembly => typeof(LivinOnSweetsApi).Assembly;
    }
}
