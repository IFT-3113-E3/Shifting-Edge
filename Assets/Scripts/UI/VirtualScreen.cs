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
            base.Awake();
            _canvas = GetComponent<Canvas>();

            if (gameCamera == null)
            {
                Debug.LogWarning("RenderTextureGraphicRaycaster: No game camera assigned.");
            }

            if (canvasScaler == null)
            {
                Debug.LogWarning("RenderTextureGraphicRaycaster: No render texture assigned.");
            }
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (gameCamera == null || canvasScaler == null || _canvas == null)
                return;

            canvasScaler.CalculateRaycastPosition(eventData.position, out Vector2 adjustedPosition);

            eventData.position = adjustedPosition;

            base.Raycast(eventData, resultAppendList);
        }
    }
}
