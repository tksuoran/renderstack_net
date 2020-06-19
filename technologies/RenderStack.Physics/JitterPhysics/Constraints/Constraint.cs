#if USE_JITTER_PHYSICS

namespace RenderStack.Physics
{
    public abstract class Constraint
    {
        internal abstract Jitter.Dynamics.Constraints.Constraint constraint { get; }
    }
}

#endif