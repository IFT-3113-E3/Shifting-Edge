using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class SectionController : MonoBehaviour
    {
        public WorldSection sectionData;
        private readonly Dictionary<string, bool> _triggerFlags = new();

        public void SetFlag(string key, bool value) => _triggerFlags[key] = value;
        public bool GetFlag(string key) => _triggerFlags.TryGetValue(key, out var val) && val;
    }
}