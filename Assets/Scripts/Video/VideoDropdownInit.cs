using TMPro;
using UnityEngine;



namespace UI
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class VideoInitDropdown : MonoBehaviour
    {
        private void Awake()
        {
            var dropdown = GetComponent<TMP_Dropdown>();
            
            // S'assure que le template est configuré
            if (dropdown.template != null)
            {
                // Désactive les composants problématiques dans le template
                var virtualScreen = dropdown.template.GetComponent<VirtualScreen>();
                if (virtualScreen != null)
                {
                    virtualScreen.enabled = false;
                }

                // Alternative plus radicale si nécessaire :
                // Destroy(virtualScreen);
            }
        }
    }
}
