using UnityEngine;

public class Collectible : MonoBehaviour
{
    public static int totalCollected = 0;

    private bool isCollected = false;
    [SerializeField] private GameObject cristal;
    private Material cristalMaterial;
    private Light pointLight;

    void Start()
    {
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
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            isCollected = true;
            totalCollected++;
            EnableEmission();
            EnablePointLight();
            Destroy(gameObject);
        }
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
