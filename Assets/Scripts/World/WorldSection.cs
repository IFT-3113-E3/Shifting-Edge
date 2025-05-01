using System;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    [CreateAssetMenu(menuName = "Game/World Section", fileName = "NewWorldSection")]
    public class WorldSection : ScriptableObject
    {
        public string sectionId; // Unique identifier like "forest01" or "castle02"

        [Header("Presentation")]
        public string displayName;

        [Header("Loading")]
        public string sceneName; // Optional if using scene backend
        public Vector3 defaultSpawnPosition; // Used if no spawn point is specified

        [Header("Connections")]
        public List<WorldConnection> exits;
    }

    [Serializable]
    public class WorldConnection
    {
        public string exitId; // e.g., "north_exit"
        public WorldSection targetSection;
        public string targetSpawnPointId;
    }
}