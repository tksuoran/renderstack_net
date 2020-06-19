using OpenTK.Graphics.OpenGL;

namespace RenderStack.Graphics
{
    /// \brief Attribute for OpenGL program
    /// 
    /// \note Mostly stable.
    public class ProgramAttribute
    {
        public string           Name  { get; private set; }
        public int              Slot  { get; private set; }
        public int              Count { get; private set; }
        public ActiveAttribType Type  { get; private set; }

        public ProgramAttribute(string name, int slot, int count, ActiveAttribType type)
        {
            Name  = name;
            Slot  = slot;
            Count = count;
            Type  = type;
        }
    }
}
