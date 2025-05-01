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

        public void LoadSection(WorldSection section, Action<SectionLoadResult> onLoaded)
        {
            _ = LoadSectionInternalAsync(section, onLoaded);
        }
        
        public void UnloadAll()
        {
            _ = UnloadAllInternalAsync();
        }

        private async Task LoadSectionInternalAsync(WorldSection section, Action<SectionLoadResult> onLoaded)
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
                return;
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
                return;
            }

            onLoaded?.Invoke(new SectionLoadResult { SceneCoordinator = coordinator });
        }
        
        private async Task UnloadAllInternalAsync()
        {
            await _sceneGraph.UnloadScene(SectionTag);
        }
    }
}