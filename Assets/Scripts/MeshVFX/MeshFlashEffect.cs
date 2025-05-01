using System.Collections.Generic;
using UnityEngine;

namespace MeshVFX
{
    public enum FlashMode { None, OneShot, Looping }

    public struct FlashOptions
    {
        public float BaseAlpha;
        public float Interval;
        public float FadeTime;
        public bool UseFade;
        public Color? ColorOverride;

        public static FlashOptions Default => new()
        {
            BaseAlpha = 1f,
            Interval = 0.5f,
            FadeTime = 0.25f,
            UseFade = true,
            ColorOverride = null,
        };
    }

    public class MeshFlashEffect : MonoBehaviour
    {
        public Material overrideMaterial;
        public string baseColorPropertyName = "_Color";

        private FlashMode _mode = FlashMode.None;
        private FlashOptions _currentOptions;
        private float _timer;
        private float _oneShotDuration;
        private bool _hasPassedStartEvent = false;
        private bool _hasFlashedBeforeStart = false;

        private readonly List<Renderer> _flashRenderers = new();
        private MaterialPropertyBlock _mpb;

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
                    var clone = Instantiate(rend, rend.transform);
                    clone.transform.localPosition = Vector3.zero;
                    clone.transform.localRotation = Quaternion.identity;
                    clone.transform.localScale = Vector3.one;
                    clone.name = rend.name + "_FlashClone";
                    clone.tag = "MeshVFX";
                    clone.gameObject.SetActive(true);
                    clone.material = new Material(overrideMaterial);
                    _flashRenderers.Add(clone);
                }
            }

            // Small trick to not re-disable the flash if a script enables it before the start event
            if (!_hasFlashedBeforeStart)
            {
                DisableFlash();
            }
            else
            {
                EnableFlash();
            }
            _hasPassedStartEvent = true;
        }

        private void Update()
        {
            if (_mode == FlashMode.None)
                return;

            _timer += Time.deltaTime;
            float alpha = 0f;

            switch (_mode)
            {
                case FlashMode.OneShot:
                    alpha = _currentOptions.UseFade
                        ? Mathf.Clamp01(1f - (_timer / _oneShotDuration)) * _currentOptions.BaseAlpha
                        : _currentOptions.BaseAlpha;

                    if (_timer >= _oneShotDuration)
                        StopFlashing();
                    break;

                case FlashMode.Looping:
                    float cycle = _timer % _currentOptions.Interval;
                    if (_currentOptions.UseFade && _currentOptions.FadeTime > 0)
                    {
                        if (cycle < _currentOptions.FadeTime)
                        {
                            alpha = Mathf.Lerp(_currentOptions.BaseAlpha, 0f, cycle / _currentOptions.FadeTime);
                        }
                        else
                        {
                            alpha = 0f;
                        }
                    }
                    else
                    {
                        alpha = (cycle < (_currentOptions.Interval / 2f)) ? _currentOptions.BaseAlpha : 0f;
                    }
                    break;
            }

            ApplyAlpha(alpha);
        }

        private void ApplyAlpha(float alpha)
        {
            foreach (var clone in _flashRenderers)
            {
                if (!clone || !clone.gameObject.activeSelf)
                    continue;
                clone.GetPropertyBlock(_mpb);
                var color = _currentOptions.ColorOverride ?? overrideMaterial.GetColor(baseColorPropertyName);
                color.a = alpha;
                _mpb.SetColor(baseColorPropertyName, color);
                clone.SetPropertyBlock(_mpb);
            }
        }

        private void EnableFlash()
        {
            if (!_hasPassedStartEvent)
            {
                _hasFlashedBeforeStart = true;
                return;
            }

            foreach (var clone in _flashRenderers)
            {
                if (!clone || !clone.gameObject.activeSelf)
                    continue;
                clone.enabled = true;
            }
        }

        private void DisableFlash()
        {
            foreach (var clone in _flashRenderers)
            {
                if (!clone || !clone.gameObject.activeSelf)
                    continue;
                clone.enabled = false;
            }
        }

        // === PUBLIC API ===

        public void SetOptions(FlashOptions options)
        {
            _currentOptions = options;
        }
        
        public void FlashOnce(float duration)
        {
            _oneShotDuration = duration;
            _mode = FlashMode.OneShot;
            _timer = 0f;
            EnableFlash();
        }

        public void StartFlashing()
        {
            // _currentOptions = options;
            _mode = FlashMode.Looping;
            _timer = 0f;
            EnableFlash();
        }

        public void StopFlashing()
        {
            _mode = FlashMode.None;
            _timer = 0f;
            DisableFlash();
        }

        public bool IsFlashing => _mode != FlashMode.None;
    }
}
