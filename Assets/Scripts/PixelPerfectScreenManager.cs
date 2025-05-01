using System;
using UI;
using UnityEngine;

public class PixelPerfectScreenManager : MonoBehaviour
{
    // public static PixelPerfectScreenManager Instance { get; private set; }
    
    private GameCanvasScaler _canvasScaler;
    
    public GameCanvasScaler CanvasScaler => _canvasScaler;
    
    // private void Awake()
    // {
    //     if (Instance != null)
    //     {
    //         Debug.LogWarning("Multiple PixelPerfectScreenManager in scene!");
    //         Destroy(gameObject);
    //         return;
    //     }
    //     Instance = this;
    //     DontDestroyOnLoad(gameObject);
    //     
    //     Debug.Log("Pixel Perfect Screen Manager has been instantiated.");
    // }
    
    private void Start()
    {
        if (_canvasScaler == null)
        {
            _canvasScaler = GetComponentInChildren<GameCanvasScaler>();
        }
    }

}