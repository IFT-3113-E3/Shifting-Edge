using UnityEngine;

public class AssignRenderTexture : MonoBehaviour
{
    public Camera minimapCamera;
    public RenderTexture minimapTexture;

    void Start()
    {
        if (minimapCamera != null && minimapTexture != null)
        {
            minimapCamera.targetTexture = minimapTexture;
        }
    }
}
