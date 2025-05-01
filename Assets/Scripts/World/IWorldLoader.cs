using System;
using System.Threading.Tasks;

namespace World
{
    public struct SectionLoadResult 
    {
        public SceneCoordinator SceneCoordinator;
    }
    
    public interface IWorldLoader
    {
        Task<SectionLoadResult> LoadSection(WorldSection section);
    }
}