using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Canvas))]
// [ExecuteInEditMode]
public class PixelUICanvas : MonoBehaviour
{
    [ReadOnly]
    private Canvas canvas;
    
    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvas.pixelPerfect = true;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.sortingLayerName = "UI";
    }

    private void Start()
    {
        canvas.worldCamera = CameraManager.Instance.GetCamera();
    }
}