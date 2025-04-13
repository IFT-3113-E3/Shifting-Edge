using UnityEngine;
using System.Collections;
using MeshVFX;
using UnityEngine.VFX;

namespace Projectiles
{

    public class GroundSpikeProjectile : MonoBehaviour
    {
        public float startHeight = -2f;
        public float riseHeight = 2f;
        public float riseTime = 0.1f;
        public float holdTime = 1f;
        public float retractTime = 0.3f;
        
        public VisualEffect eruptionVFX;
        private VisualEffect _vfx;
        
        private MeshFlashEffect _meshFlashEffect;

        private Vector3 _startPos;
        private Vector3 _endPos;
        private bool _isDone = false;

        private void Start()
        {
            _startPos = transform.position + transform.forward * startHeight;
            _endPos = _startPos + transform.forward * riseHeight;
            
            _meshFlashEffect = GetComponent<MeshFlashEffect>();
            if (_meshFlashEffect)
            {
                _meshFlashEffect.SetOptions(new FlashOptions
                {
                    UseFade = true,
                    BaseAlpha = 1f,
                });
            }
            
            // Start the behavior sequence
            StartCoroutine(SpikeRoutine());
        }

        private IEnumerator SpikeRoutine()
        {
            if (_meshFlashEffect)
            {
                _meshFlashEffect.FlashOnce(1f);
            }
            Vector3 groundPos = FindGroundPosition(_endPos);
            // Spawn the eruption VFX at the ground position
            SpawnEruptionVFX(groundPos);
            
            yield return MoveSpike(_startPos, _endPos, riseTime);

            yield return new WaitForSeconds(holdTime);

            yield return MoveSpike(_endPos, _startPos, retractTime);

            Destroy(gameObject);
        }

        private IEnumerator MoveSpike(Vector3 from, Vector3 to, float duration)
        {
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                float progress = Mathf.SmoothStep(0, 1, t / duration);
                transform.position = Vector3.Lerp(from, to, progress);
                yield return null;
            }
        }

        private void SpawnEruptionVFX(Vector3 position)
        {
            if (eruptionVFX != null)
            {
                _vfx = Instantiate(eruptionVFX, position, Quaternion.identity, null);
                _vfx.transform.up = transform.forward;
                _vfx.Play();
                Destroy(_vfx.gameObject, 2f);
            }
            else
            {
                Debug.LogError("Eruption VFX prefab not assigned.");
            }
        }
        
        private Vector3 FindGroundPosition(Vector3 position)
        {
            RaycastHit hit;
            if (Physics.Raycast(position, -transform.forward, out hit, 20f))
            {
                return hit.point;
            }
            return position;
        }
        
    }

}