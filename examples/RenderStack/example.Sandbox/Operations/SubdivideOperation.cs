namespace example.Sandbox
{
    public partial class Operations
    {
        public void Subdivide()
        {
            if(selectionManager == null)
            {
                return;
            }

            if(selectionManager.Models.Count == 0)
            {
                Subdivide(selectionManager.HoverModel);
            }
            else
            {
                foreach(var model in selectionManager.Models)
                {
                    Subdivide(model);
                }
            }
        }
    }
}