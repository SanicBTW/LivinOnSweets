using System.Reflection;
using LivinOnSweets.API.Attributes;
using LivinOnSweets.API.Sprites.Editor;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osuTK;

namespace LivinOnSweets.API.Containers.Editor
{
    // The sidebar of the editor, contains everything needed for the editor itself, selecting container, buttons etc
    // the reason this isnt inside editor container its because it would be really long and bad lookin lol

    // There should be a way to register the categories inside the dependency container,
    // just like in lazer ruleset store loading the rulesets dynamically yknow
    public partial class EditorSideBar : FillFlowContainer<EditorEntry>
    {
        private const string namespace_target_folder = "EditorComponents";

        [Cached]
        private SlideContainer slideContainer;

        public BindableColour4 PrimaryColor = new();
        public BindableColour4 SecondaryColor = new();

        public EditorSideBar(SlideContainer slideContainer)
        {
            this.slideContainer = slideContainer;

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
            Type entryType = typeof(EditorEntry);
            Type attrType = typeof(EditorImportOrder);

            Assembly curAssembly = Assembly.GetExecutingAssembly();
            TypeInfo[] types = curAssembly.DefinedTypes
                .Where(ti => ti.BaseType == entryType &&
                                ti.Namespace!.Contains(namespace_target_folder)).ToArray();

            EditorEntry[] orderedImports = new EditorEntry[types.Length];
            foreach (TypeInfo type in types)
            {
                EditorEntry instance = (EditorEntry)Activator.CreateInstance(type, this);
                EditorImportOrder importIndex = (EditorImportOrder)type.GetCustomAttributes(attrType).FirstOrDefault();
                if (importIndex is null)
                    throw new ArgumentNullException($"{instance!.GetType()} is missing the attribute {attrType}");

                orderedImports[importIndex.ImportPosition] = instance;
            }

            AddRangeInternal(orderedImports);
        }

        internal void PropagateScreenCtxChange(ScreenStack newCtx)
        {
            foreach (Drawable drawable in InternalChildren)
            {
                EditorEntry entry = (EditorEntry)drawable;
                entry.ChangeScreenCtx(newCtx);
            }
        }
    }
}
