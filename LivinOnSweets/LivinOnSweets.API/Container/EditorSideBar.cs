using System.Reflection;
using LivinOnSweets.API.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace LivinOnSweets.API.Container
{
    // The sidebar of the editor, contains everything needed for the editor itself, selecting container, buttons etc
    // the reason this isnt inside editor container its because it would be really long and bad lookin lol

    // There should be a way to register the categories inside the dependency container,
    // just like in lazer ruleset store loading the rulesets dynamically yknow
    public partial class EditorSideBar : FillFlowContainer<EditorEntry>
    {
        private const string namespace_target_folder = "EditorComponents";

        public BindableColour4 PrimaryColor = new();
        public BindableColour4 SecondaryColor = new();

        public EditorSideBar()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(6);
            Padding = new MarginPadding(8);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // Regarding the comment above the class definition, this is probably one of the
            // dynamic ways to load up entries without having to break my brain too much
            Assembly curAssembly = Assembly.GetExecutingAssembly();

            IEnumerable<TypeInfo> types = curAssembly.DefinedTypes;
            Type entryType = typeof(EditorEntry);
            foreach (TypeInfo type in types)
            {
                if (type.BaseType != entryType)
                    continue;

                if (!type.Namespace!.Contains(namespace_target_folder))
                    continue;

                AddInternal((EditorEntry)Activator.CreateInstance(type, this));
            }
        }
    }
}
