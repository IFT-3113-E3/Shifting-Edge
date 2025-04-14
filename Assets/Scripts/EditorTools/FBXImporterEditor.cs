using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace EditorTools
{
    public class FBXImporterEditor : EditorWindow
    {
        private GameObject fbxAsset;

        [MenuItem("Tools/FBX Cleanup Tool")]
        public static void ShowWindow()
        {
            GetWindow<FBXImporterEditor>("FBX Cleanup");
        }

        private void OnGUI()
        {
            fbxAsset = (GameObject)EditorGUILayout.ObjectField("FBX Model", fbxAsset, typeof(GameObject), false);

            if (fbxAsset != null && GUILayout.Button("Clean and Instantiate"))
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset);
                instance.name = fbxAsset.name + "_Cleaned";

                CleanHierarchy(instance.transform);

                Selection.activeGameObject = instance;
            }
        }

        private void CleanHierarchy(Transform root)
        {
            // Recursively clean all names and structure
            foreach (Transform child in root)
            {
                CleanHierarchy(child);

                // Strip namespace prefix
                if (child.name.Contains(":"))
                {
                    string newName = child.name.Substring(child.name.IndexOf(":") + 1);
                    child.name = newName;
                }

                // Flatten "Group" if needed
                if (child.name.ToLower() == "group" && child.childCount > 0)
                {
                    Transform parent = child.parent;

                    // Move all children up
                    for (int i = child.childCount - 1; i >= 0; i--)
                    {
                        Transform grandChild = child.GetChild(i);
                        grandChild.SetParent(parent);
                    }

                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }
    }
}
#endif