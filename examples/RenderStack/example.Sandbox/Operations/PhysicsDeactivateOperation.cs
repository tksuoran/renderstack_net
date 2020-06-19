namespace example.Sandbox
{
    public partial class Operations
    {
        public void PhysicsDeactivate()
        {
            if(selectionManager == null)
            {
                return;
            }

            if(
                (selectionManager.HoverModel != null) && 
                (selectionManager.HoverModel.Static == false)
            )
            {
                if(selectionManager.HoverModel != null)
                {
                    sceneManager.DeactivateModel(selectionManager.HoverModel);
                }
            }
        }
    }
}
