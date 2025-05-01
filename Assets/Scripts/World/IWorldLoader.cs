using System;

namespace World
{
    public struct SectionLoadResult 
    {
        public SceneCoordinator SceneCoordinator;
    }
    
    public interface IWorldLoader
    {
        void LoadSection(WorldSection section, Action<SectionLoadResult> onLoaded);
    }
}