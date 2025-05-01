using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VirtualCameraConfig
{
    public Transform transform;

    public float fieldOfView = 60f;
    public float nearClipPlane = 0.1f;
    public float farClipPlane = 1000f;

    public bool orthographic = false;
    public float orthographicSize = 5f;

    public CameraClearFlags clearFlags = CameraClearFlags.Skybox;
    public Color backgroundColor = Color.black;

    public LayerMask cullingMask = ~0;

    public static VirtualCameraConfig FromCamera(Camera cam)
    {
        return new VirtualCameraConfig
        {
            transform = cam.transform,
            fieldOfView = cam.fieldOfView,
            nearClipPlane = cam.nearClipPlane,
            farClipPlane = cam.farClipPlane,
            orthographic = cam.orthographic,
            orthographicSize = cam.orthographicSize,
            clearFlags = cam.clearFlags,
            backgroundColor = cam.backgroundColor,
            cullingMask = cam.cullingMask
        };
    }
    
    public void UpdateFromCamera(Camera cam)
    {
        transform = cam.transform;
        fieldOfView = cam.fieldOfView;
        nearClipPlane = cam.nearClipPlane;
        farClipPlane = cam.farClipPlane;
        orthographic = cam.orthographic;
        orthographicSize = cam.orthographicSize;
        clearFlags = cam.clearFlags;
        backgroundColor = cam.backgroundColor;
        cullingMask = cam.cullingMask;
    }
}

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [SerializeField] private Camera physicalCamera;
    [SerializeField] private RenderTexture renderTarget;

    private readonly Stack<VirtualCameraConfig> _virtualCameraStack = new();

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
        DontDestroyOnLoad(gameObject);
        physicalCamera.targetTexture = renderTarget;
    }

    public void PushVirtualCamera(VirtualCameraConfig config)
    {
        if (config == null || config.transform == null) return;
        if (_virtualCameraStack.Contains(config)) return;

        _virtualCameraStack.Push(config);
        UpdateCamera();
    }

    public void PopVirtualCamera(VirtualCameraConfig config)
    {
        if (!_virtualCameraStack.Contains(config)) return;

        var list = new List<VirtualCameraConfig>(_virtualCameraStack);
        list.Remove(config);
        _virtualCameraStack.Clear();
        for (int i = list.Count - 1; i >= 0; i--) _virtualCameraStack.Push(list[i]);

        UpdateCamera();
    }


    private void Update()
    {
        UpdateCamera();
    }

    private void UpdateCamera()
    {
        if (_virtualCameraStack.Count == 0) return;

        var cam = _virtualCameraStack.Peek();
        if (cam == null || !cam.transform) return;

        // Transform
        physicalCamera.transform.SetPositionAndRotation(cam.transform.position, cam.transform.rotation);

        // Core settings
        physicalCamera.fieldOfView = cam.fieldOfView;
        physicalCamera.nearClipPlane = cam.nearClipPlane;
        physicalCamera.farClipPlane = cam.farClipPlane;
        physicalCamera.orthographic = cam.orthographic;
        physicalCamera.orthographicSize = cam.orthographicSize;

        // Rendering
        physicalCamera.clearFlags = cam.clearFlags;
        physicalCamera.backgroundColor = cam.backgroundColor;
        physicalCamera.cullingMask = cam.cullingMask;
    }


    public Camera GetCamera() => physicalCamera;
}