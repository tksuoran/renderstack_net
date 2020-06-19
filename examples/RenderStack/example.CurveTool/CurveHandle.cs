using RenderStack.Graphics;
using RenderStack.Math;

using example.Renderer;

//  TODO:
//      http://www.niksula.hut.fi/~hkankaan/Homepages/bezierfast.html
namespace example.CurveTool
{
    /*  Comment: Experimental  */ 
    public class CurveHandle
    {
        public Model model;

        public Floats Parameters = new Floats(0.0f, 0.0f, 0.0f);

        public Vector3 Position
        {
            get
            {
                //model.Frame.UpdateHierarchical();
                return new Vector3(
                    model.Frame.LocalToWorld.Matrix._03,
                    model.Frame.LocalToWorld.Matrix._13,
                    model.Frame.LocalToWorld.Matrix._23
                );
            }
            set
            {
                //  TODO take into account parent transform!
                //model.Frame.UpdateHierarchical();
                model.Frame.LocalToParent.SetTranslation(value);
                model.Frame.LocalToWorld.SetTranslation(value);
                //model.Frame.UpdateHierarchical();
            }
        }
    }
}
