using System.Collections.Generic;
using example.Renderer;

namespace example.Sandbox
{
    public class Delete : IOperation
    {
        private class Entry
        {
            public Model    model;
            public bool     selected;

            public Entry(Model model)
            {
                this.model = model;
                selected = model.Selected;
            }
        }
        private Entry[] entries;

        public Delete(ICollection<Model> models)
        {
            entries = new Entry[models.Count];
            int i = 0;
            foreach(var model in models)
            {
                entries[i] = new Entry(model);
                ++i;
            }
        }

        void IOperation.Execute(Application sandbox)
        {
            SceneManager sceneManager = RenderStack.Services.BaseServices.Get<SceneManager>();
            SelectionManager selectionManager = RenderStack.Services.BaseServices.Get<SelectionManager>();
            if (sceneManager == null)
            {
                return;
            }
            foreach (var entry in entries)
            {
                if (entry.model == sceneManager.FloorModel)
                {
                    continue;
                }
                entry.model.Selected = false;
                sceneManager.RemoveModel(entry.model);
                if (selectionManager == null)
                {
                    continue;
                }
                if (entry.selected)
                {
                    selectionManager.Remove(entry.model);
                }
            }
        }
        void IOperation.Undo(Application sandbox)
        {
            SceneManager sceneManager = RenderStack.Services.BaseServices.Get<SceneManager>();
            SelectionManager selectionManager = RenderStack.Services.BaseServices.Get<SelectionManager>();
            if (sceneManager == null)
            {
                return;
            }
            foreach (var entry in entries)
            {
                if (entry.model == sceneManager.FloorModel)
                {
                    continue;
                }
                entry.model.Selected = entry.selected;
                sceneManager.AddModel(entry.model);
                if (selectionManager == null)
                {
                    continue;
                }
                if (entry.selected)
                {
                    selectionManager.Add(entry.model);
                }
            }
        }
    }

    public partial class Operations
    {
        public void Delete()
        {
            SelectionManager selectionManager = RenderStack.Services.BaseServices.Get<SelectionManager>();
            if(selectionManager == null)
            {
                return;
            }

            if(selectionManager.Models.Count == 0)
            {
                Delete(selectionManager.HoverModel);
            }
            else
            {
                Delete(selectionManager.Models);
                /*var deleteList = new List<Model>();
                foreach(var model in selectionManager.Models)
                {
                    deleteList.Add(model);
                }
                foreach(var model in deleteList)
                {
                    Delete(model);
                }*/
            }
        }

        public void Delete(Model model)
        {
            if(model == null)
            {
                return;
            }

            List<Model> models = new List<Model>();
            models.Add(model);

            Delete delete = new Delete(models);
            operationStack.Do(delete);
            //model.Selected = false;
            //sceneManager.RemoveModel(model);
            //selectionManager.Models.Remove(model);
        }
        public void Delete(ICollection<Model> models)
        {
            Delete delete = new Delete(models);
            operationStack.Do(delete);
        }
    }
}
