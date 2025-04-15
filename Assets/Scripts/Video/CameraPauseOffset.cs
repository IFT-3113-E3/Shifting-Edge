using UnityEngine;

public class CameraPauseOffset : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform; // La caméra (pas besoin d'être enfant)
    public Transform target;          // Joueur ou cible

    [Header("Settings")]
    public float pauseDistance = 6f;
    public float hoverAmount = 0.5f;
    public float moveSpeed = 3f;

    private Vector3 originalPosition; // Stocke la position monde (pas locale)
    private Vector3 targetOffset;
    private bool isPaused = false;

    void Start()
    {
        if (cameraTransform != null)
            originalPosition = cameraTransform.position; // Monde, pas localPosition
    }

    void LateUpdate()
    {
        if (cameraTransform == null || target == null) return;

        if (isPaused)
        {
            // Direction monde (pas besoin de conversion parente)
            Vector3 desiredOffset = -cameraTransform.forward * pauseDistance + targetOffset;
            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                originalPosition + desiredOffset,
                Time.unscaledDeltaTime * moveSpeed
            );
        }
        else
        {
            // Retour à la position originale (monde)
            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                originalPosition,
                Time.unscaledDeltaTime * moveSpeed
            );
        }
    }

    public void EnterPause()
    {
        isPaused = true;
        targetOffset = Vector3.up * 1f + (-cameraTransform.forward * 4f);
    }

    public void ExitPause()
    {
        isPaused = false;
        targetOffset = Vector3.zero;
    }

    public void Hover(string dir)
    {
        if (!isPaused || cameraTransform == null) return;

        Vector3 worldOffset = Vector3.zero;
        switch (dir)
        {
            case "left":  worldOffset = -cameraTransform.right; break;
            case "right": worldOffset = cameraTransform.right; break;
            case "up":    worldOffset = cameraTransform.up;    break;
            case "down":  worldOffset = -cameraTransform.up;   break;
        }

        targetOffset = worldOffset * hoverAmount + Vector3.up * 0.5f;
    }
}