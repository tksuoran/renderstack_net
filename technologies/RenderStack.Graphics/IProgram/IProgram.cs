using System.Diagnostics;

namespace RenderStack.Graphics
{
    public interface IProgram : System.IDisposable
    {
        string              Name                { get; set; }
        bool                Valid               { get; }
        AttributeMappings   AttributeMappings   { get; set; }

        void                Use         (int baseInstance);
        ProgramAttribute    Attribute   (string name);
    }
    public class ProgramFactory
    {
        public static IProgram Load(string name)
        {
            if(Configuration.useGl1)
            {
                return (IProgram)ProgramGL1.Load(name);
            }
            return (IProgram)ProgramGL3.Load(name);
        }
    }
}
