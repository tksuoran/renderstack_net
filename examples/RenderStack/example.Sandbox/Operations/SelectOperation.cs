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

        void IOperation.Execute(Application sandbox)
        {
            SelectionManager selectionManager = BaseServices.Get<SelectionManager>();
            if (selectionManager == null)
            {
                return;
            }

            foreach (var model in selectionManager.Models)
            {
                model.Selected = false;
            }
            selectionManager.Models.Clear();

            foreach (var model in after)
            {
                model.Selected = true;
                selectionManager.Models.Add(model);
            }
        }
        void IOperation.Undo(Application sandbox)
        {
            SelectionManager selectionManager = BaseServices.Get<SelectionManager>();
            if (selectionManager == null)
            {
                return;
            }

            foreach (var model in selectionManager.Models)
            {
                model.Selected = false;
            }
            selectionManager.Models.Clear();

            foreach (var model in before)
            {
                model.Selected = true;
                selectionManager.Models.Add(model);
            }
        }
    }
}