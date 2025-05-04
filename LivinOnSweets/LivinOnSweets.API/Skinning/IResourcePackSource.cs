namespace LivinOnSweets.API.Skinning
{
    // https://github.com/ppy/osu/blob/master/osu.Game/Skinning/ISkinSource.cs
    /// <summary>
    /// An abstract resource pack implementation, whose primary purpose is to properly handle the propagation of the resource pack changes.
    /// </summary>
    public interface IResourcePackSource : IResourcePack
    {
        /// <summary>
        /// Fired whenever a source change occurs, signalling that consumers should re-query as required.
        /// </summary>
        event Action SourceChanged;
    }
}
