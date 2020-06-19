using System;

namespace RenderStack.Graphics
{
    /// Interface for uniform values that can be fed to OpenGL uniforms
    /// 
    /// \note Somewhat experimental.
    public interface IUniformValue : IComparable
    {
        //! Creates a clone of this IUniformValue, returning the same type but does not copy values
        IUniformValue GetUninitialized();

        //! Copies values from another IUniformValue. It should have the same type
        bool    IsCompatibleWith(object o);
        void    CopyFrom(IUniformValue cache);

        bool    Dirty   { get; set; }
        int     Index   { get; set; }
    }
}
