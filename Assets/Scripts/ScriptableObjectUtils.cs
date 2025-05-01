using UnityEngine;

public static class ScriptableObjectUtils
{
    public static T CreateRuntimeInstance<T>(T source) where T : ScriptableObject
    {
        return Object.Instantiate(source);
    }
}