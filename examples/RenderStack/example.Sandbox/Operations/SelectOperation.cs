//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System.Collections.Generic;

using RenderStack.Services;
using example.Renderer;

namespace example.Sandbox
{
    public class SelectOperation : IOperation
    {
        private List<Model> before;
        private List<Model> after;

        public SelectOperation(
            List<Model> before,
            List<Model> after
        )
        {
            this.before = before;
            this.after = after;
        }

        public void Execute(Application sandbox)
        {
            var selectionManager = Services.Get<SelectionManager>();
            if(selectionManager == null)
            {
                return;
            }

            foreach(var model in selectionManager.Models)
            {
                model.Selected = false;
            }
            selectionManager.Models.Clear();

            foreach(var model in after)
            {
                model.Selected = true;
                selectionManager.Models.Add(model);
            }
        }
        public void Undo(Application sandbox)
        {
            var selectionManager = Services.Get<SelectionManager>();
            if(selectionManager == null)
            {
                return;
            }

            foreach(var model in selectionManager.Models)
            {
                model.Selected = false;
            }
            selectionManager.Models.Clear();

            foreach(var model in before)
            {
                model.Selected = true;
                selectionManager.Models.Add(model);
            }
        }
    }
}