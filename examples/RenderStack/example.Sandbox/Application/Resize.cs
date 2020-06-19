using System;

namespace example.Sandbox
{
    public partial class Application
    {
        private void Application_Resize(object sender, EventArgs e)
        {
            RenderStack.Services.BaseServices.Get<HighLevelRenderer>().Resize(Width, this.Height);
        }
    }
}