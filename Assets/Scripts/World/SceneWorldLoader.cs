using System;
using System.Threading.Tasks;
using UnityEngine;

namespace World
{
    public class SceneWorldLoader : IWorldLoader
    {
        private readonly SceneGraphService _sceneGraph;

        private const string SectionTag = "Section";

        public SceneWorldLoader(SceneGraphService sceneGraph)
        {
            _sceneGraph = sceneGraph;
        }

        public async Task<SectionLoadResult> LoadSection(WorldSection section)
        {
            return await LoadSectionInternalAsync(section);
        }
        
        public async Task UnloadAll()
        {
            await UnloadAllInternalAsync();
        }

        private async Task<SectionLoadResult> LoadSectionInternalAsync(WorldSection section)
        {
            if (!_sceneGraph.NodeExists(SectionTag))
            {
                await _sceneGraph.PushChildScene("Main", SectionTag, section.sceneName);
            }
            else
            {
                await _sceneGraph.ReplaceScene(SectionTag, section.sceneName);
            }

            var scene = _sceneGraph.GetScene(SectionTag);
            if (scene == null)
            {
                Debug.LogError($"Scene '{section.sceneName}' could not be retrieved by tag '{SectionTag}' after load.");
                throw new InvalidOperationException($"Scene '{section.sceneName}' could not be retrieved by tag '{SectionTag}' after load.");
            }

            SceneCoordinator coordinator = null;
            foreach (var obj in scene.Value.GetRootGameObjects())
            {
                coordinator = obj.GetComponentInChildren<SceneCoordinator>();
                if (coordinator) break;
            }

            if (!coordinator)
            {
                Debug.LogError($"SceneCoordinator not found in scene '{section.sceneName}'");
                throw new InvalidOperationException($"SceneCoordinator not found in scene '{section.sceneName}'");
            }
            
            return new SectionLoadResult { SceneCoordinator = coordinator };
        }
        
        private async Task UnloadAllInternalAsync()
        {
            await _sceneGraph.UnloadScene(SectionTag);
        }
    }
}