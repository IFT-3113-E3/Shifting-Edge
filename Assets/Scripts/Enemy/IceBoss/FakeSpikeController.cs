using MeshVFX;
using UnityEngine;

namespace Enemy.IceBoss
{
    public class FakeSpikeController
    {
        private readonly GameObject _fakeSpikeInstance;
        private readonly DissolveEffectController _dissolveEffectController;
        private readonly Transform _handTransform;
        
        bool _isFormed = false;

        public FakeSpikeController(GameObject fakeSpikePrefab, Transform handTransform)
        {
            if (fakeSpikePrefab == null)
            {
                Debug.LogError("[FakeSpikeController] Fake spike prefab is not assigned.");
                return;
            }

            _handTransform = handTransform;
            _fakeSpikeInstance = Object.Instantiate(fakeSpikePrefab, handTransform.position,
                handTransform.rotation);
            _fakeSpikeInstance.transform.SetParent(handTransform);
            
            _dissolveEffectController = _fakeSpikeInstance.GetComponent<DissolveEffectController>();
            
            _fakeSpikeInstance.SetActive(false);
        }

        public void Form()
        {
            _fakeSpikeInstance.SetActive(true);
            _dissolveEffectController.PlayEffect(DissolveEffectController.EffectMode.Materialize);
        }
        
        public void Hide()
        {
            _fakeSpikeInstance.SetActive(false);
        }
        
        public bool IsFormed()
        {
            return _fakeSpikeInstance.activeSelf && _dissolveEffectController.IsVisible;
        }
    }
}