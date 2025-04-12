using UnityEngine;
using System.Collections;

namespace Projectiles
{

    public class GroundSpikeProjectile : MonoBehaviour
    {
        public float startHeight = -2f;
        public float riseHeight = 2f;
        public float riseTime = 0.1f;
        public float holdTime = 1f;
        public float retractTime = 0.3f;

        private Vector3 _startPos;
        private Vector3 _endPos;
        private bool _isDone = false;

        private void Start()
        {
            // total length in z for the spike to correctly hideitself before rising
            
            _startPos = transform.position + transform.forward * startHeight;
            _endPos = _startPos + transform.forward * riseHeight;

            // Start the behavior sequence
            StartCoroutine(SpikeRoutine());
        }

        private IEnumerator SpikeRoutine()
        {
            // BURST UP
            yield return MoveSpike(_startPos, _endPos, riseTime);

            // HOLD
            yield return new WaitForSeconds(holdTime);

            // RETRACT DOWN
            yield return MoveSpike(_endPos, _startPos, retractTime);

            // DESTROY
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
    }

}