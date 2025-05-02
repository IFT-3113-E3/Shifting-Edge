using UnityEngine;

public class Collectible : MonoBehaviour
{
    public CollectibleData collectibleData;

    private bool isCollected = false;
    [SerializeField] private GameObject cristal;
    private Material cristalMaterial;
    private Light pointLight;

    private void Start()
    {
        if (collectibleData == null)
        {
            Debug.LogError("CollectibleData is not assigned in the inspector.");
        }
        if (cristal != null)
        {
            if (cristal.TryGetComponent<Renderer>(out var renderer))
            {
                cristalMaterial = renderer.material;
                DisableEmission();
            }

            var pointLightTransform = cristal.transform.Find("Point Light");
            if (pointLightTransform != null)
            {
                pointLight = pointLightTransform.GetComponent<Light>();
                if (pointLight != null)
                {
                    pointLight.enabled = false;
                }
            }
        }
        CheckCollectedState();
    }

    private void CheckCollectedState()
    {
        if (collectibleData == null)
        {
            return;
        }
        if (GameManager.Instance.GameSession.GameProgression.HasCollected(collectibleData.id))
        {
            isCollected = true;
            EnableEmission();
            EnablePointLight();
            gameObject.SetActive(false);
        }
        else
        {
            isCollected = false;
            DisableEmission();
            if (pointLight != null)
            {
                pointLight.enabled = false;
            }
            gameObject.SetActive(true);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Collect();
    }
    
    private void Collect()
    {
        if (collectibleData == null)
        {
            return;
        }
        if (isCollected) return;

        isCollected = true;
        GameManager.Instance.GameSession.GameProgression.MarkCollectibleCollected(
            collectibleData.id);
        EnableEmission();
        EnablePointLight();
        // Set the collectible to inactive after a delay
        gameObject.SetActive(false);
    }

    private void DisableEmission()
    {
        if (cristalMaterial != null)
        {
            cristalMaterial.DisableKeyword("_EMISSION");
        }
    }

    private void EnableEmission()
    {
        if (cristalMaterial != null)
        {
            cristalMaterial.EnableKeyword("_EMISSION");
        }
    }

    private void EnablePointLight()
    {
        if (pointLight != null)
        {
            pointLight.enabled = true;
        }
    }
}
