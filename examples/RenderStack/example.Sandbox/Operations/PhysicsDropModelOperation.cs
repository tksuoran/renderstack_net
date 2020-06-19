namespace example.Sandbox
{
    public partial class Operations
    {
        public override string Name
        {
            get { return "Operations"; }
        }
        public bool PhysicsOk()
        {
            if(
                (Configuration.physics == false) ||
                (selectionManager == null) ||
                (selectionManager.HoverModel == null) ||
                (selectionManager.HoverModel.RigidBody == null) ||
                (selectionManager.HoverModel.Static == true)
            )
            {
                return false;
            }
            return true;
        }
    }
}
