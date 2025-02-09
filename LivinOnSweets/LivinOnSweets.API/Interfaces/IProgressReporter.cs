namespace LivinOnSweets.API.Interfaces
{
    // Basic interface that can be plugged with a Screen or anything that accepts this interface
    // e.g: Load screens
    public interface IProgressReporter
    {
        public float GetLoadProgress();
    }
}
