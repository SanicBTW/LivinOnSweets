#if DEBUG
using LivinOnSweets.Editor.Containers;
#else
using System;
using System.IO;
using System.Reflection;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using LivinOnSweets.API.Overlays;
#endif
using LivinOnSweets.API.Input;
using LivinOnSweets.Game.StartScreens;
using osu.Framework.Allocation;
using osu.Framework.Graphics;

using osu.Framework.Screens;

namespace LivinOnSweets.Game
{
    public partial class LivinOnSweetsGame : LivinOnSweetsGameBase
    {
        // We are 100% sure that the Parent (LivinOnSweetsGameBase) last child is gonna be the input container
        protected ManiaActionContainer ActionContainer => (ManiaActionContainer)Content[^1];

        private ScreenStack screenStack;

        [BackgroundDependencyLoader]
        private void load()
        {
            screenStack = new ScreenStack()
            {
                RelativeSizeAxes = Axes.Both
            };

            injectEditor();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            screenStack.Push(new PreloaderScreen());
        }

        // New feature! It will look for the Editor DLL to load it up
        // and hopefully get some nice features on Release builds
        // while also saving some precious DLL size lmao
        private void injectEditor()
        {
            // The debug build has access to the package, thus providing the types, making it easier i believe
#if DEBUG
            LoadComponentAsync(new DebugContainer(screenStack), ActionContainer.Add);
#else
            void defaultAdd()
            {
                ActionContainer.Add(screenStack);
                ActionContainer.Add(new ScreenshotOverlay());
            }

            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LivinOnSweets.Editor.dll");
            if (File.Exists(dllPath))
            {
                try
                {
                    Assembly editorAssembly = Assembly.LoadFrom(dllPath);
                    Type editorContainer = editorAssembly.GetType("LivinOnSweets.Editor.Containers.DebugContainer");
                    if (editorContainer == null)
                        throw new TypeLoadException($"Failed to get \"LivinOnSweets.Editor.Containers.DebugContainer\" type.");

                    object editorInstance = Activator.CreateInstance(editorContainer, args: [screenStack]);
                    if (editorInstance == null)
                        throw new InvalidOperationException("Failed to create an instance of the DebugContainer.");

                    // Cast the DebugEditor into a Container, since thats what it inherits...
                    LoadComponentAsync((Container)editorInstance, ActionContainer.Add);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to inject the editor: {ex.Message}");
                    defaultAdd();
                }
            }
            else
                defaultAdd();
#endif
        }
    }
}
