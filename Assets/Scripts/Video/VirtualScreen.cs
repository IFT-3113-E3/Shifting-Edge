using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class VirtualScreen : GraphicRaycaster
    {
        [SerializeField] private Camera gameCamera; // Reference to the camera rendering to the RenderTexture
        [SerializeField] private GameCanvasScaler canvasScaler; // The scaler of the UI projecting the RenderTexture
        private Canvas _canvas;

        protected override void Awake()
        {
            if (name.Contains("Template") || transform.parent?.name.Contains("Dropdown") == true) 
                return;
                
            base.Awake();
            _canvas = GetComponent<Canvas>();

            // if (gameCamera == null)
            // {
            //     Debug.LogWarning("RenderTextureGraphicRaycaster: No game camera assigned.");
            // }

            if (canvasScaler == null)
            {
                // Debug.LogWarning("RenderTextureGraphicRaycaster: No render texture assigned.");
                canvasScaler = FindFirstObjectByType<GameCanvasScaler>();
            }
        }

        protected override void Start()
        {
            Debug.Log($"VirtualScreen: {name} started.");
            // if (gameCamera == null)
            // {
            //     gameCamera = Camera.main;
            // }
            if (canvasScaler == null || canvasScaler.isActiveAndEnabled == false ||
                canvasScaler.gameObject.activeInHierarchy == false)
            {
                // Debug.LogWarning("RenderTextureGraphicRaycaster: No render texture assigned.");
                canvasScaler = FindFirstObjectByType<GameCanvasScaler>();
            }
            
            // set the canvas to camera space
            if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                _canvas.renderMode = RenderMode.ScreenSpaceCamera;
                _canvas.worldCamera = CameraManager.Instance.GetCamera();
            }
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (!canvasScaler)
                return;

            canvasScaler.CalculateRaycastPosition(eventData.position, out var adjustedPosition);

            Vector2 originalPosition = eventData.position;

            eventData.position = adjustedPosition;
            base.Raycast(eventData, resultAppendList);

            eventData.position = originalPosition;
        }

    }
}