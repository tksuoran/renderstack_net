using System.Collections.Generic;

namespace RenderStack.Graphics
{
    public interface IUniformBlock
    {
        List<Uniform>   Uniforms        { get; }
        string          Name            { get; }
        int             Size            { get; }
        Callback        ChangeDelegate  { get; set; }
        IUniformBuffer  UniformBuffer   { get; set; }
        int             BindingPointGL  { get; }
        string          SourceGL        { get; }
    }
}
