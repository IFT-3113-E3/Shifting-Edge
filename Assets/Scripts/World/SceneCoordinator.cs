using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace World
{
    public class SceneCoordinator : MonoBehaviour
    {
        public List<Transform> spawnPoints = new();
        private readonly Dictionary<string, Transform> _spawnPointMap = new();
        public SectionController SectionController { get; private set; }
        
        private OrbitCamera _orbitCamera;

        private void Awake()
        {
            SectionController = FindFirstObjectByType<SectionController>();
            _orbitCamera = FindFirstObjectByType<OrbitCamera>();
            foreach (var spawnPoint in spawnPoints.Where(spawnPoint => spawnPoint)
                         .Where(spawnPoint => !_spawnPointMap.TryAdd(spawnPoint.name, spawnPoint)))
            {
                Debug.LogError($"Duplicate spawn point found: {spawnPoint.name}");
            }
        }

        private void Start()
        {

        }
        
        public void AssignCameraTarget(Transform target)
        {
            if (_orbitCamera == null) return;
            _orbitCamera.SetMainTarget(target);
        }

        public Transform GetSpawnPoint(string id)
        {
            return _spawnPointMap.GetValueOrDefault(id);
        }
    }
}