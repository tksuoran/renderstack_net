#if USE_BEPU_PHYSICS

namespace RenderStack.Physics
{
    public abstract class Constraint
    {
        internal abstract BEPUphysics.UpdateableSystems.Updateable updateable { get; }
    }
}

#endif