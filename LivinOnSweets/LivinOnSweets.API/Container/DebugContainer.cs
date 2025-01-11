using System.Reflection;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.Overlays;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Screens;
using OContainer = osu.Framework.Graphics.Containers.Container;

namespace LivinOnSweets.API.Container
{
    // Container that handles debug actions like refreshing or opening the layout editor, which in fact, its inside of this one
    // TODO: Make a refresh target menu
    public partial class DebugContainer : OContainer, IKeyBindingHandler<ManiaAction>
    {
        [Resolved]
        private GameStateManager stateManager { get; set; }

        private static Type screenStackType => typeof(ScreenStack);

        private ScreenStack screenStackRef;
        private Stack<IScreen> screens;

        // sanco here, 3:34am, just read that this attribute propagates the field
        // to its children, so by doing this im propagating the editor container
        // thru the dependency container of screen stack and to other screens, really good
        [Cached]
        private EditorContainer editor;

        public DebugContainer(ScreenStack screenStack)
        {
            Depth = -99;
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                screenStackRef = screenStack,
                editor = new EditorContainer()
            };

            saveStackReference();
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            switch (e.Action)
            {
                // Maybe its much worse than just handling the refresh on the startup screen which would result on
                // a single screen to exit and no screen stack swapping, but this is global
                // meaning that i can refresh anywhere now
                case ManiaAction.REFRESH:
                    IScreen firstScreen = screens.ToArray()[^1];
                    if (firstScreen == null)
                    {
                        Logger.Log("Couldn't refresh the game, is there any screen on the stack?");
                        return true;
                    }

                    // Because doing a for loop for the amount of screens in the stack
                    // can be troublesome we reset the screen stack
                    RemoveInternal(screenStackRef, true);
                    screenStackRef = new ScreenStack() { RelativeSizeAxes = Axes.Both };
                    AddInternal(screenStackRef);
                    ChangeInternalChildDepth(screenStackRef, 1);

                    stateManager.Reset();

                    Type screenType = firstScreen.GetType();
                    screenStackRef.Push((IScreen)Activator.CreateInstance(screenType));
                    saveStackReference();
                    return true;

                case ManiaAction.EDITOR:
                    editor.ToggleVisibility();
                    return true;

                default:
                    return false;
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }

        private void saveStackReference()
        {
            FieldInfo member = screenStackType.GetField("stack", BindingFlags.NonPublic | BindingFlags.Instance);
            if (member == null)
                return;

            screens = (Stack<IScreen>)member.GetValueDirect(__makeref(screenStackRef));
        }
    }
}
