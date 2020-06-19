namespace example.Renderer
{
    public abstract class RenderState
    {
        public abstract void Reset();
        public abstract void Execute();

        public virtual bool Expand()
        {
            return false;
        }
    }
}