using UnityEngine;
using UnityEditor;
using System.IO;

public class RootMotionRemover : EditorWindow
{
    private AnimationClip sourceClip;
    private string savePath = "Assets/NoRootMotion.anim";

    [MenuItem("Tools/Remove Root Motion From Clip")]
    public static void ShowWindow()
    {
        GetWindow<RootMotionRemover>("Remove Root Motion");
    }

    void OnGUI()
    {
        GUILayout.Label("Remove Root Motion From Animation", EditorStyles.boldLabel);
        sourceClip = (AnimationClip)EditorGUILayout.ObjectField("Source Clip", sourceClip, typeof(AnimationClip), false);
        savePath = EditorGUILayout.TextField("Save Path", savePath);

        if (GUILayout.Button("Remove Root Motion and Save"))
        {
            if (sourceClip == null)
            {
                Debug.LogWarning("Please assign a source animation clip.");
                return;
            }

            RemoveRootMotion(sourceClip, savePath);
        }
    }

    private void RemoveRootMotion(AnimationClip clip, string path)
    {
        AnimationClip newClip = new AnimationClip();
        EditorUtility.CopySerialized(clip, newClip);

        var bindings = AnimationUtility.GetCurveBindings(newClip);
        foreach (var binding in bindings)
        {
            if (IsRootMotionBinding(binding))
            {
                newClip.SetCurve(binding.path, binding.type, binding.propertyName, null);
            }
        }

        // Save the new clip as an asset
        AssetDatabase.CreateAsset(newClip, path);
        AssetDatabase.SaveAssets();

        Debug.Log("Saved animation without root motion to: " + path);
    }

    private bool IsRootMotionBinding(EditorCurveBinding binding)
    {
        string prop = binding.propertyName.ToLower();
        string path = binding.path.ToLower();

        // Typical root motion path and properties
        bool isRootPath = path == "" || path.Contains("root") || path.Contains("hips");

        bool isRootPos = prop.Contains("motiont") || prop.Contains("roott") || prop.EndsWith(".x") || prop.EndsWith(".y") || prop.EndsWith(".z");
        bool isRootRot = prop.Contains("motionq") || prop.Contains("rootq") || prop.EndsWith(".w");

        return isRootPath && (isRootPos || isRootRot);
    }
}
