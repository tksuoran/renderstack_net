using RenderStack.Scene;
using RenderStack.Physics;

namespace example.Renderer
{
    // \brief Interface to allow Models to act as physics objects
    // \note This will probably be removed from example.Renderer namespace eventually
    public interface IPhysicsObject
    {
        Frame       Frame           { get; }
        string      Name            { get; }
        Shape       PhysicsShape    { get; set; } 
        RigidBody   RigidBody       { get; set; }
        bool        Static          { get; set; }
        bool        ShadowCaster    { get; } // \todo this is a bit hacky?
        bool        UsePosition     { get; }
        bool        UseRotation     { get; }
    }
}

