using System;
using UnityEngine;
    
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class VirtualCameraWrapper : MonoBehaviour
{
    private VirtualCameraConfig _config;
    private Camera _camera;
    
    public Camera PhysicalCamera => CameraManager.Instance?.GetCamera();

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        _camera.enabled = false;
        _config = VirtualCameraConfig.FromCamera(_camera);
        CameraManager.Instance?.PushVirtualCamera(_config);
    }

    private void Update()
    {
        _config.UpdateFromCamera(_camera);
    }

    private void OnDisable()
    {
        if (_config == null) return;
        CameraManager.Instance?.PopVirtualCamera(_config);
        _config = null;
    }
}