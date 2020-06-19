namespace RenderStack.Graphics.AssetMonitor
{
    /*  Comment: Hightly experimental */
    public interface IMonitored
    {
        void OnChanged();
        void OnFileChanged(string fullpath);
    }
}
