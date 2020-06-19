using System.Collections.Generic;
using RenderStack.Services;

namespace example.Sandbox
{
    public class OperationStack : Service
    {
        public override string Name
        {
            get { return "OperationStack"; }
        }

        Application sandbox;

        private Stack<IOperation> executed = new Stack<IOperation>();
        private Stack<IOperation> undone = new Stack<IOperation>();

        public void Connect(Application sandbox)
        {
            this.sandbox = sandbox;
        }

        protected override void InitializeService()
        {
        }

        public void Do(IOperation operation)
        {
            operation.Execute(sandbox);
            executed.Push(operation);
        }

        public void Undo()
        {
            if(executed.Count == 0)
            {
                return;
            }

            IOperation operation = executed.Pop();
            operation.Undo(sandbox);
            undone.Push(operation);
        }

        public void Redo()
        {
            if(undone.Count == 0)
            {
                return;
            }

            IOperation operation = undone.Pop();
            operation.Execute(sandbox);
            executed.Push(operation);
        }
    }
}