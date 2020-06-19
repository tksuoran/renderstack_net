//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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

        public OperationStack() : base()
        {
        }

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