using System;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.Stores;
using LivinOnSweets.Resources;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osuTK;

namespace LivinOnSweets.Game
{
    public partial class LivinOnSweetsGameBase : osu.Framework.Game
    {
        protected override Container<Drawable> Content { get; }
        private DependencyContainer gameDependencies;
        private ResizeHandler resizeHandler = new ResizeHandler();

        protected LivinOnSweetsGameBase()
        {
            base.Content.Add(Content = new DrawSizePreservingFillContainer
            {
                TargetDrawSize = new Vector2(1280, 720)
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Resources.AddStore(new DllResourceStore(LivinOnSweetsResources.ResourceAssembly));
            SetupDependencies(gameDependencies);

            resizeHandler.UpdateSize(Window.ClientSize);
            Window.Resized += () => resizeHandler.UpdateSize(Window.ClientSize);
        }

        protected virtual void SetupDependencies(DependencyContainer container)
        {
            // Cache the storage variable from the host since it will be used inside the configuration managers
            // And make it accessible across the tree
            container.CacheAs(Host.Storage);

            // Used to handle resizes on some parts of the game, passed down through a container for better usability
            container.CacheAs(resizeHandler);

            // Used to save states and react to them on some parts of the game
            container.CacheAs(new GameStateManager());

            // Used to pass an accent store through the dp container
            container.CacheAs(new AccentStore(Resources));

            IResourceStore<TextureUpload> texUpload = Host.CreateTextureLoaderStore(new NamespacedResourceStore<byte[]>(Resources, "Textures"));

            LargeTextureStore largeTS = new(Host.Renderer, texUpload);
            container.CacheAs(largeTS);

            // since the game is pixel art (most of the times except the story mode sprites) we make a pixel art store to set the filter mode to nearest
            PixelArtTextureStore pixArtTS = new(Host.Renderer, texUpload);
            container.CacheAs(pixArtTS);

            Action<IResourceStore<TextureUpload>>[] texLookups = [largeTS.AddTextureSource, pixArtTS.AddTextureSource, Textures.AddTextureSource];

            // Add the resource stores to the texture lookups
            container.CacheAs(AddToTextureLookup(new StoryModeStore(Resources), texLookups));
            container.CacheAs(AddToTextureLookup(new StartupStore(Resources), texLookups));

            ManiaActionContainer actionContainer = [];
            container.CacheAs(actionContainer);
            Content.Add(actionContainer);
        }

        // this might be insecure af but we balling with it anyways trust
        protected virtual T AddToTextureLookup<T>(T store, Action<IResourceStore<TextureUpload>>[] addFuncs)
            where T : IResourceStore<byte[]>
        {
            IResourceStore<TextureUpload> upload = Host.CreateTextureLoaderStore(store);

            foreach (Action<IResourceStore<TextureUpload>> addFunc in addFuncs)
                addFunc(upload);

            return store;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            gameDependencies = new(base.CreateChildDependencies(parent));
    }
}
