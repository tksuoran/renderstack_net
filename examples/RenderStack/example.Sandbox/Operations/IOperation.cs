namespace example.Sandbox
{
    public interface IOperation
    {
        void Execute(Application sandbox);
        void Undo(Application sandbox);
    }
}