using System;
using System.Collections;
using UnityEngine;

namespace Enemy.IceBoss
{
    [RequireComponent(typeof(Animator), typeof(FractureEffect))]
    public class BossAnimator : MonoBehaviour
    {
        private Animator animator;
        private FractureEffect fractureEffect;
        
        private Coroutine currentCoroutine;
        
        private AudioSource _audioSource;

        public AudioClip tpInAudioClip;
        public AudioClip tpOutAudioClip;

        private void Start()
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("[BossAnimator] Animator component not found!");
            }

            fractureEffect = GetComponent<FractureEffect>();
            if (fractureEffect == null)
            {
                Debug.LogError("[BossAnimator] FractureEffect component not found!");
            }
            
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                Debug.LogError("[BossController] AudioSource component not found!");
            }
        }


        public void AssembleAndSpawn(Vector3 targetPos, Quaternion targetRotation,
            Action onComplete = null)
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
            StartCoroutine(AssembleAndSpawnRoutine(targetPos, targetRotation, onComplete));
        }

        private IEnumerator AssembleAndSpawnRoutine(Vector3 targetPos,
            Quaternion targetRotation, Action onComplete = null)
        {
            fractureEffect.SetFragmentsEnabled(true);

            // Set original object invisible
            fractureEffect.SwapVisibility(false);

            yield return null;

            transform.position = targetPos;
            transform.rotation = targetRotation;

            yield return StartCoroutine(fractureEffect.ReassembleSmooth(1f, 0.2f));

            fractureEffect.SwapVisibility(true);
            fractureEffect.SetFragmentsEnabled(false);

            onComplete?.Invoke();
        }

        public void TeleportWithExplosion(Vector3 targetPos, Quaternion targetRotation,
            Action onComplete = null)
        {
            StartCoroutine(TeleportSequence(targetPos, targetRotation, onComplete));
        }

        private IEnumerator TeleportSequence(Vector3 targetPos, Quaternion targetRotation,
            Action onComplete)
        {
            fractureEffect.SetFragmentsEnabled(true);

            // Reset fragments position and rotation
            fractureEffect.SetGravityEnabled(false);
            fractureEffect.ResetFragmentsAtSource();

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            // Set original object invisible
            fractureEffect.SwapVisibility(false);

            // yield return new WaitForFixedUpdate();
            yield return StartCoroutine(
                fractureEffect.BreakAndPauseFragmentsSmooth(explosionForce: 0.05f, slowdownTime: 0.75f));
            
            // Play teleport sound
            if (_audioSource != null && tpOutAudioClip != null)
            {
                _audioSource.PlayOneShot(tpOutAudioClip);
            }
            
            yield return new WaitForSeconds(0.5f);

            fractureEffect.DissolveFragments();

            // yield return StartCoroutine(BreakAndPauseFragmentsSmooth(explosionForce: 0.5f, slowdownTime: 0.75f));

            yield return new WaitUntil(fractureEffect.AreAllFragmentsFinishedDissolving);
            fractureEffect.SetFragmentGroupPositionAndOrientation(
                targetPos, targetRotation);

            yield return null;

            fractureEffect.AppearFragments();

            transform.position = targetPos;
            transform.rotation = targetRotation;

            if (_audioSource != null && tpInAudioClip != null)
            {
                _audioSource.PlayOneShot(tpInAudioClip);
            }
            yield return StartCoroutine(fractureEffect.ReassembleSmooth(0.5f, 0.05f));

            
            yield return new WaitUntil(fractureEffect.AreAllFragmentsFinishedDissolving);

            // Set original object visible
            fractureEffect.SwapVisibility(true);

            fractureEffect.ResetFragmentsAtSource();

            fractureEffect.SetFragmentsEnabled(false);


            onComplete?.Invoke();
        }

        public void EnsureBossIsVisibleAndFragmentsAreDisabled()
        {
            if (fractureEffect != null)
            {
                fractureEffect.SwapVisibility(true);
                fractureEffect.SetFragmentsEnabled(false);
            }
        }
        
        private Vector3 tpAim;
        private Vector3 tpAimDirection;

        private void Update()
        {
            Vector2 screenMouse = Input.mousePosition;

            if (Camera.main)
            {
                screenMouse.x /= Screen.width;
                screenMouse.y /= Screen.height;
                screenMouse.x *= Camera.main.pixelWidth;
                screenMouse.y *= Camera.main.pixelHeight;
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                if (Camera.main)
                {
                    Ray ray = Camera.main.ScreenPointToRay(screenMouse);
                    Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 2f);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        tpAim = hit.point;
                    }
                }
            }

            if (Input.GetKey(KeyCode.T))
            {
                if (Camera.main)
                {
                    Ray ray = Camera.main.ScreenPointToRay(screenMouse);
                    Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 2f);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        tpAimDirection = hit.point;
                    }
                }
            }
            else if (Input.GetKeyUp(KeyCode.T))
            {
                TeleportWithExplosion(tpAim, Quaternion.LookRotation(tpAimDirection - tpAim));
            }
        }
    }
}