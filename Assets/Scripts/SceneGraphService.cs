using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneGraphService : MonoBehaviour
{
    private class SceneNode
    {
        public string Tag;
        public string SceneName;
        public List<SceneNode> Children = new();
    }

    private readonly Dictionary<string, SceneNode> _sceneRoots = new();
    private readonly Dictionary<string, Scene> _loadedScenes = new();
    
    public void SetRootScene(string tag, string sceneName)
    {
        if (_sceneRoots.ContainsKey(tag))
            throw new InvalidOperationException($"Scene tag '{tag}' already set as root");

        var scene = SceneManager.GetSceneByName(sceneName);

        _loadedScenes[tag] = scene;
        _sceneRoots[tag] = new SceneNode { Tag = tag, SceneName = sceneName };
    }
    
    public static Task LoadSceneAdditiveAsync(string sceneName, bool setActive = false)
    {
        var tcs = new TaskCompletionSource<object>();

        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (op == null)
        {
            tcs.SetException(new InvalidOperationException($"Failed to start loading scene: {sceneName}"));
            return tcs.Task;
        }

        op.completed += _ =>
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid())
            {
                tcs.SetException(new InvalidOperationException($"Scene '{sceneName}' was not loaded correctly."));
                return;
            }

            if (setActive)
            {
                SceneManager.SetActiveScene(scene);
            }

            tcs.SetResult(null);
        };

        return tcs.Task;
    }
    
    public static Task UnloadSceneAsync(string sceneName)
    {
        var tcs = new TaskCompletionSource<object>();

        var op = SceneManager.UnloadSceneAsync(sceneName);
        if (op == null)
        {
            tcs.SetException(new InvalidOperationException($"Failed to start unloading scene: {sceneName}"));
            return tcs.Task;
        }

        op.completed += _ =>
        {
            if (op.isDone)
            {
                tcs.SetResult(null);
            }
            else
            {
                tcs.SetException(new InvalidOperationException($"Scene '{sceneName}' was not unloaded correctly."));
            }
        };

        return tcs.Task;
    }

    public async Task LoadScene(string tag, string sceneName)
    {
        if (_loadedScenes.ContainsKey(tag))
            throw new InvalidOperationException($"Scene tag '{tag}' already loaded");

        await LoadSceneAdditiveAsync(sceneName, setActive: true);

        var scene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(scene);
        _loadedScenes[tag] = scene;
        _sceneRoots[tag] = new SceneNode { Tag = tag, SceneName = sceneName };
    }

    public async Task ReplaceScene(string tag, string sceneName)
    {
        if (_loadedScenes.TryGetValue(tag, out var existing))
        {
            // if there are children, unload them first
            if (_sceneRoots.TryGetValue(tag, out var node))
            {
                foreach (var child in node.Children.ToList())
                {
                    await UnloadScene(child.Tag);
                }
            }
            var unloadOp = SceneManager.UnloadSceneAsync(existing);
            while (unloadOp is { isDone: false }) await Task.Yield();

            _loadedScenes.Remove(tag);
            _sceneRoots.Remove(tag);
        }

        await LoadScene(tag, sceneName);
    }

    public async Task PushChildScene(string parentTag, string childTag, string childSceneName)
    {
        Debug.Log($"Pushing child scene '{childTag}' to parent '{parentTag}'");
        if (!_sceneRoots.TryGetValue(parentTag, out var parent))
            throw new InvalidOperationException($"Parent tag '{parentTag}' not loaded");
        Debug.Log($"Pushing child scene '{childTag}' to parent '{parentTag}'");

        await LoadScene(childTag, childSceneName);
        var child = _sceneRoots[childTag];
        parent.Children.Add(child);
    }

    public async Task PopChildren(string tag)
    {
        if (!_sceneRoots.TryGetValue(tag, out var parent)) return;

        foreach (var child in parent.Children.ToList())
        {
            await UnloadScene(child.Tag);
        }

        parent.Children.Clear();
    }

    public async Task UnloadScene(string tag)
    {
        if (!_loadedScenes.TryGetValue(tag, out var scene)) return;

        // var op = SceneManager.UnloadSceneAsync(scene);
        // while (op is { isDone: false }) await Task.Yield();
        await UnloadSceneAsync(scene.name);

        _loadedScenes.Remove(tag);
        _sceneRoots.Remove(tag);
    }
    
    public bool HasChild(string parentTag, string childTag)
    {
        return _sceneRoots.TryGetValue(parentTag, out var parent) && parent.Children.Any(child => child.Tag == childTag);
    }

    public bool NodeExists(string tag)
    {
        return _sceneRoots.ContainsKey(tag);
    }

    public Scene? GetScene(string tag) => _loadedScenes.TryGetValue(tag, out var scene) ? scene : (Scene?)null;
}
