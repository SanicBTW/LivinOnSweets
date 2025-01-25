using JetBrains.Annotations;
using LivinOnSweets.API.Containers;

namespace LivinOnSweets.API.Data
{
    // Basic readonly class that is used to pass over a type reference to a SweetScreen alongside
    // some arguments for the constructor to load it inside a GameContainer
    public class GameScreenData(Type nextScreen, [CanBeNull] object[] args = null, [CanBeNull] Action onLoad = null, [CanBeNull] Action onError = null)
    {
        private static Type screenTargetType => typeof(SweetScreen);

        public readonly Type NextScreen = nextScreen;
        [CanBeNull] [ItemCanBeNull] private readonly object[] ctorArgs = args;
        [CanBeNull] public readonly Action OnLoad = onLoad;
        [CanBeNull] public readonly Action OnError = onError;

        // Ensures if the given type is a SweetScreen, to avoid getting an exception of "Unable to cast object"
        // I should make it so it allows you to pass an IScreen or Screen but uhh yeah I'm only gonna use SweetScreen
        public bool EnsureScreen() => NextScreen == null ? false : NextScreen.BaseType == screenTargetType;

        // Wraps the unsafe create with a quick check of the given type before calling activator, if it doesnt match, return a null value, most likely to be handled by the context
        [CanBeNull] public SweetScreen CreateScreen() => EnsureScreen() ? CreateScreenUnsafe() : null;

        // Method which calls Activator and casts the given object to a SweetScreen, called unsafe because theres no failsafe
        [CanBeNull]
        public SweetScreen CreateScreenUnsafe() => (SweetScreen)Activator.CreateInstance(NextScreen, args: ctorArgs);
    }
}
