using System.Collections.Generic;
using UnityEngine;

namespace Projectiles
{
    [CreateAssetMenu(fileName = "ProjectileDatabase", menuName = "Gameplay/Projectile Database", order = 2)]
    public class ProjectileDatabase : ScriptableObject
    {
        [SerializeField]
        private List<ProjectileData> projectiles;

        private Dictionary<string, ProjectileData> _lookup;

        private void OnEnable()
        {
            _lookup = new Dictionary<string, ProjectileData>();
            foreach (var projectile in projectiles)
            {
                if (projectile != null && !_lookup.ContainsKey(projectile.projectileName))
                {
                    _lookup[projectile.projectileName] = projectile;
                }
            }
        }

        public bool TryGet(string name, out ProjectileData data)
        {
            if (_lookup == null) OnEnable();
            return _lookup.TryGetValue(name, out data);
        }

        public ProjectileData Get(string name)
        {
            if (_lookup == null) OnEnable();
            if (_lookup.TryGetValue(name, out var data))
                return data;

            Debug.LogError($"Projectile '{name}' not found in the database.");
            return null;
        }

        public IEnumerable<string> GetAllNames()
        {
            if (_lookup == null) OnEnable();
            return _lookup.Keys;
        }
    }
}