using System.Collections.Generic;
using UnityEngine;

namespace MeshVFX
{
    public class MeshFlashEffect : MonoBehaviour
    {
        public bool useFade = true;
        public float flashInterval = 0.5f;
        public float fadeTime = 0.5f; // ignored if useFade is false

        public float baseAlpha = 1f;
        public Material overrideMaterial;
        public string baseColorPropertyName = "_Color";

        private float _time;
        private bool _isFlashing;
        private float _lastFlashTime;
        private bool _flashState;
        
        private MaterialPropertyBlock _mpb;
        private readonly List<Renderer> _meshRenderers = new();
        private readonly List<Renderer> _copiedRenderers = new();

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
        }

        private void Start()
        {
            foreach (var rend in GetComponentsInChildren<Renderer>())
            {
                if (rend is MeshRenderer or SkinnedMeshRenderer)
                {
                    _meshRenderers.Add(rend);
                    
                    // copy the component in place at teh same hierarchy as the original
                    var clone = Instantiate(rend, rend.transform);
                    clone.gameObject.name = $"{rend.gameObject.name}_FlashClone";
                    clone.gameObject.SetActive(false);
                    clone.material = new Material(overrideMaterial);
                    _copiedRenderers.Add(clone);
                }
            }
        }

        public void SetFlashing(bool flashing)
        {
            _isFlashing = flashing;
            _time = 0f;
            _lastFlashTime = 0f;

            if (!_isFlashing)
            {
                foreach (var clone in _copiedRenderers)
                {
                    clone.enabled = false;
                    clone.gameObject.SetActive(false);
                }
            }
            else
            {
                foreach (var clone in _copiedRenderers)
                {
                    clone.enabled = true;
                    clone.gameObject.SetActive(true);
                }
            }
        }
        
        // public void BakeMesh()
        // {
        //     if (_meshFlashClones == null)
        //     {
        //         _meshFlashClones = GetClones();
        //     }
        //
        //     for (int i = 0; i < _meshFlashClones.Length; i++)
        //     {
        //         var clone = _meshFlashClones[i];
        //         clone.SetTransform(_meshRenderers[i].transform);
        //         clone.SetLayer(gameObject.layer);
        //         clone.UpdateMesh();
        //     }
        // }

        private void Update()
        {
            if (!_isFlashing || !overrideMaterial)
                return;

            _time += Time.deltaTime;

            float cycleTime = _time % flashInterval;
            float halfInterval = flashInterval * 0.5f;

            float alpha;
            if (cycleTime < halfInterval)
            {
                if (useFade && fadeTime > 0f)
                {
                    float fade = Mathf.Clamp01(1f - (cycleTime / fadeTime));
                    alpha = baseAlpha * fade;
                }
                else
                {
                    alpha = baseAlpha;
                }
            }
            else
            {
                alpha = 0f;
            }

            var color = overrideMaterial.GetColor(baseColorPropertyName);
            color.a = alpha;

            foreach (var clone in _copiedRenderers)
            {
                if (!clone.enabled)
                    clone.enabled = true;

                clone.GetPropertyBlock(_mpb);
                _mpb.SetColor(baseColorPropertyName, color);
                clone.SetPropertyBlock(_mpb);
            }
        }
    }
}
