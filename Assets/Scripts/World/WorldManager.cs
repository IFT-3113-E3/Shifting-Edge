using System;
using UnityEngine;

namespace World
{
    public class WorldManager : MonoBehaviour
    {
        
        private SceneWorldLoader _loader;
        private WorldSection _currentSection;
        private SceneCoordinator _currentSceneCoordinator;

        private GameContext _gameContext;
        private GameSession _session;
        
        public event Action<SectionLoadResult> OnLoaded;


        public void Initialize(GameContext gameContext, GameSession session)
        {
            _gameContext = gameContext;
            _session = session;

            if (_gameContext == null)
            {
                Debug.LogError("GameContext is null.");
                return;
            }

            if (_session == null)
            {
                Debug.LogError("GameSession is null.");
                return;
            }

            _loader = new SceneWorldLoader(_gameContext.SceneService);
        }
        
        public void Uninitialize()
        {
            if (_loader != null)
            {
                _loader.UnloadAll();
                _loader = null;
            }

            _currentSection = null;
            _currentSceneCoordinator = null;
        }

        public void StartSessionAtSection(WorldSection section, string spawnPointId)
        {
            if (!section)
            {
                Debug.LogError("Section is null, cannot start session.");
                return;
            }

            _currentSection = section;
            _session.worldSectionId = section.sectionId;
            _session.spawnPointId = spawnPointId;

            _loader.LoadSection(section, OnSectionLoaded);
        }

        private void OnSectionLoaded(SectionLoadResult result)
        {
            _currentSceneCoordinator = result.SceneCoordinator;

            if (!_currentSceneCoordinator)
            {
                Debug.LogError("SceneCoordinator is null after load.");
                return;
            }
            
            OnLoaded?.Invoke(result);
        }

        public void TransitionTo(string exitId)
        {
            if (_currentSection == null)
            {
                Debug.LogError("Cannot transition: current section is null.");
                return;
            }

            var exit = _currentSection.exits.Find(e => e.exitId == exitId);
            if (exit == null || exit.targetSection == null)
            {
                Debug.LogError($"Exit '{exitId}' not found or has no target.");
                return;
            }

            Debug.Log($"Transitioning to section '{exit.targetSection.sectionId}' via exit '{exitId}'.");

            StartSessionAtSection(exit.targetSection, exit.targetSpawnPointId);
        }

        public SceneCoordinator GetCurrentSceneCoordinator() => _currentSceneCoordinator;

        public SectionController GetCurrentSectionController() => _currentSceneCoordinator?.SectionController;

        public WorldSection GetCurrentSection() => _currentSection;
    }
}