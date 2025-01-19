namespace LivinOnSweets.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class EditorImportOrder(int position) : Attribute
    {
        public int ImportPosition = position;
    }
}
