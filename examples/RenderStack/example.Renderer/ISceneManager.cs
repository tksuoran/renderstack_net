using RenderStack.Services;

using System.Collections.Generic;

namespace example.Renderer
{
    public interface ISceneManager : IService
    {
        Group       RenderGroup         { get; }
        List<Group> RenderGroups        { get; }
        List<Group> IdGroups            { get; }
        List<Group> ShadowCasterGroups  { get; }
    }
}