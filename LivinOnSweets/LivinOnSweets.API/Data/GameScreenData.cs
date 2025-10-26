using JetBrains.Annotations;
using LivinOnSweets.API.Screens;
// ReSharper disable MemberCanBePrivate.Global

namespace LivinOnSweets.API.Data
{
    // https://github.com/SanicBTW/LivinOnSweets/blob/master/LivinOnSweets/LivinOnSweets.API/Data/GameScreenData.cs
    // Basic readonly class that is used to pass over a type reference to a SweetScreen alongside
    // some arguments for the constructor to load it inside a GameContainer
    public class GameScreenData(Type nextScreen, [CanBeNull] object[] args = null, [CanBeNull] Action onLoad = null, [CanBeNull] Action onError = null)
    {
        private static Type screenTargetType => typeof(SweetScreen);
        private static Type screenTargetSubType => typeof(SweetSubScreen);

        public readonly Type NextScreen = nextScreen;
        [CanBeNull] [ItemCanBeNull] private readonly object[] ctorArgs = args;
        [CanBeNull] public readonly Action OnLoad = onLoad;
        [CanBeNull] public readonly Action OnError = onError;

        // Ensures if the given type is a SweetScreen, to avoid getting an exception of "Unable to cast object"
        // I should make it so it allows you to pass an IScreen or Screen but uhh yeah I'm only gonna use SweetScreen
        // not anymore brochacho, embrace generics!!!
        public bool EnsureScreen() => NextScreen != null && (NextScreen.BaseType == screenTargetType || NextScreen.BaseType == screenTargetSubType);

        public bool IsSubScreen() => NextScreen != null && NextScreen.BaseType == screenTargetSubType;

        // Wraps the unsafe create with a quick check of the given type before calling activator, if it doesnt match, return a null value, most likely to be handled by the context
        [CanBeNull] public T CreateScreen<T>() where T : SweetScreen  => EnsureScreen() ? CreateScreenUnsafe<T>() : null;

        // Method which calls Activator and casts the given object to a SweetScreen, called unsafe because theres no failsafe
        [CanBeNull] public T CreateScreenUnsafe<T>() where T : SweetScreen => (T)Activator.CreateInstance(NextScreen, args: ctorArgs);
    }
}
