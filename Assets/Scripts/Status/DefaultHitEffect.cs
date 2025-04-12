using System.Collections;
using System.Linq;
using UnityEngine;

namespace Status
{
    public class DefaultHitEffect : MonoBehaviour, IHitReaction
    {
        public Color flashColor = Color.red;
        public float flashDuration = 0.1f;

        private Coroutine _flashRoutine;
        
        private Renderer[] _renderers;
        // private Color[] _originalColors;
        
        public ParticleSystem hitParticles;
        
        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>().Except(GetComponentsInChildren<ParticleSystemRenderer>()).ToArray();
            // _originalColors = new Color[_renderers.Length];
            // for (int i = 0; i < _renderers.Length; i++)
            // {
            //     _originalColors[i] = _renderers[i].material.color;
            // }
            hitParticles = GetComponentInChildren<ParticleSystem>();
            if (hitParticles != null)
            {
                hitParticles.Stop();
            }
        }

        public void ReactToHit(DamageRequest damageRequest)
        {
            var hitPoint = damageRequest.hitPoint;
            
            // if (_flashRoutine != null)
            //     StopCoroutine(_flashRoutine);
            // _flashRoutine = StartCoroutine(FlashRoutine());
            if (hitParticles != null)
            {
                hitParticles.transform.position = hitPoint;
                hitParticles.Clear();
                hitParticles.Simulate(hitParticles.main.duration);
                hitParticles.Play();
            }
        }

        private IEnumerator FlashRoutine()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                var material = _renderers[i].material;
                if (!material) continue;
                material.color = flashColor;
            }
            yield return new WaitForSeconds(flashDuration);
            for (int i = 0; i < _renderers.Length; i++)
            {
                var material = _renderers[i].material;
                if (!material) continue;
                // material.color = _originalColors[i];
            }
        }
    }
}