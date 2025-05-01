using UnityEngine;

/**
 * This class is used to mark objects that should only exist in the editor.
 * It does not have any functionality and is used for organizational purposes.
 * They will be deleted when the game is built, and when the game is running in the editor.
 */
public class EditorOnlyObject : MonoBehaviour
{
    private void Awake()
    {
        Destroy(gameObject);
    }
}