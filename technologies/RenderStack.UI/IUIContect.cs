using RenderStack.Math;

namespace RenderStack.UI
{
    /*  Comment: Experimental  */ 
    public interface IUIContext
    {
        bool[]  MouseButtons    { get; }
        Vector2 Mouse           { get; }

        //Material Material(string name);
    }
}