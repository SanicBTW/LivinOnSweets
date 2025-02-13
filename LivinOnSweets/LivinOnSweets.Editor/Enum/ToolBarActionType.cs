// ReSharper disable InconsistentNaming
namespace LivinOnSweets.Editor.Enum;

// Represents the possible types a ToolBarButton action can act like
// Kind of dumb yeah but I want ToolBarButton to be as reusable as possible
internal enum ToolBarActionType
{
    TOGGLEABLE, // Can toggle the action
    STANDARD, // Behaves like a standard butotn
}
