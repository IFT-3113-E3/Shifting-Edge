using System;
using System.Collections;
using MeshVFX;
using UnityEngine;
using UnityEngine.VFX;

namespace Enemy.IceBoss
{
    [RequireComponent(typeof(Animator), typeof(FractureEffect))]
    public class BossAnimator : MonoBehaviour
    {
        private Animator _animator;
        private FractureEffect _fractureEffect;
        private MeshTrailEffect _meshTrailEffect;
        private MeshFlashEffect _meshFlashEffect;

        private Coroutine _currentCoroutine;
        private Coroutine _animCoroutine;

        private AudioSource _audioSource;

        public AudioClip tpInAudioClip;
        public AudioClip tpOutAudioClip;
        public AudioClip[] throwAudioClip;
        public AudioClip[] throwPrepareAudioClip;
        public AudioClip[] moveAudioClips;

        private Vector3 _targetPos;
        private Quaternion _targetRotation;
        
        public GameObject fakeSpikePrefab;
        private FakeSpikeController _fakeSpikeController;
        
        private VisualEffect[] _visualEffects;

        public Transform handTransform;

        private EntityMovementController _mc;

        public event Action<Transform> OnThrowEvent;
        
        private void Start()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                Debug.LogError("[BossAnimator] Animator component not found!");
            }

            _fractureEffect = GetComponent<FractureEffect>();
            if (_fractureEffect == null)
            {
                Debug.LogError("[BossAnimator] FractureEffect component not found!");
            }
            
            _meshTrailEffect = GetComponent<MeshTrailEffect>();
            if (_meshTrailEffect == null)
            {
                Debug.LogError("[BossAnimator] MeshTrailEffect component not found!");
            }
            
            _meshFlashEffect = GetComponent<MeshFlashEffect>();
            if (_meshFlashEffect == null)
            {
                Debug.LogError("[BossAnimator] MeshFlashEffect component not found!");
            }

            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                Debug.LogError("[BossController] AudioSource component not found!");
            }
            
            if (fakeSpikePrefab == null)
            {
                Debug.LogError("[BossAnimator] Fake spike prefab is not assigned.");
            }
            else
            {
                _fakeSpikeController = new FakeSpikeController(fakeSpikePrefab, handTransform);
            }
            
            _mc = GetComponent<EntityMovementController>();
            if (_mc == null)
            {
                Debug.LogError("[BossAnimator] EntityMovementController component not found!");
            }
            
            _visualEffects = GetComponentsInChildren<VisualEffect>();
            
        }

        public void UpdateTarget(Vector3 targetPos, Quaternion targetRotation)
        {
            _targetPos = targetPos;
            _targetRotation = targetRotation;
        }
        
        public void SetVisible(bool isVisible)
        {
            if (_fractureEffect != null)
            {
                _fractureEffect.SwapVisibility(isVisible);
            }
            foreach (var vfx in _visualEffects)
            {
                if (vfx != null)
                {
                    if (isVisible)
                    {
                        vfx.Play();
                    }
                    else
                    {
                        vfx.Stop();
                    }
                }
            }
        }
        
        public void SetFlashEnabled(bool enabled, float flashTime = 0.5f)
        {
            if (enabled)
            {
                _meshFlashEffect.SetFlashing(true);
            }
            else
            {
                _meshFlashEffect.SetFlashing(false);
            }
        }

        public void AssembleAndSpawn(Vector3 targetPos, Quaternion targetRotation,
            Action onComplete = null)
        {
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
            }

            StartCoroutine(AssembleAndSpawnRoutine(targetPos, targetRotation, onComplete));
        }

        private IEnumerator AssembleAndSpawnRoutine(Vector3 targetPos,
            Quaternion targetRotation, Action onComplete = null)
        {
            _fractureEffect.SetFragmentsEnabled(true);

            // Set original object invisible
            SetVisible(false);

            yield return null;

            _mc.SetPosition(targetPos);
            _mc.SetRotation(targetRotation);

            yield return StartCoroutine(_fractureEffect.ReassembleSmooth(1f, 0.2f));

            SetVisible(true);
            _fractureEffect.SetFragmentsEnabled(false);

            onComplete?.Invoke();
        }

        public void TeleportWithExplosion(Vector3 targetPos, Quaternion targetRotation,
            Action onComplete = null)
        {
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
            }

            _targetPos = targetPos;
            _targetRotation = targetRotation;
            StartCoroutine(TeleportSequence(onComplete));
        }

        private IEnumerator TeleportSequence(Action onComplete)
        {
            _fractureEffect.SetFragmentsEnabled(true);

            // Reset fragments position and rotation
            _fractureEffect.SetGravityEnabled(false);
            _fractureEffect.ResetFragmentsAtSource();

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            // Set original object invisible
            SetVisible(false);

            // yield return new WaitForFixedUpdate();
            yield return StartCoroutine(
                _fractureEffect.BreakAndPauseFragmentsSmooth(explosionForce: 0.05f,
                    slowdownTime: 0.75f));

            // Play teleport sound
            if (_audioSource != null && tpOutAudioClip != null)
            {
                _audioSource.PlayOneShot(tpOutAudioClip);
            }

            yield return new WaitForSeconds(0.1f);

            _fractureEffect.DissolveFragments();

            // yield return StartCoroutine(BreakAndPauseFragmentsSmooth(explosionForce: 0.5f, slowdownTime: 0.75f));
            yield return new WaitUntil(_fractureEffect.AreAllFragmentsFinishedDissolving);
            var pos = _targetPos;
            var rot = _targetRotation;
            _fractureEffect.SetFragmentGroupPositionAndOrientation(
                pos, rot);

            yield return null;

            _fractureEffect.AppearFragments();

            _mc.SetPosition(pos);
            _mc.SetRotation(rot);

            if (_audioSource != null && tpInAudioClip != null)
            {
                _audioSource.PlayOneShot(tpInAudioClip);
            }

            yield return StartCoroutine(_fractureEffect.ReassembleSmooth(0.5f, 0.05f));


            yield return new WaitUntil(_fractureEffect.AreAllFragmentsFinishedDissolving);

            // Set original object visible
            SetVisible(true);

            _fractureEffect.ResetFragmentsAtSource();

            _fractureEffect.SetFragmentsEnabled(false);


            onComplete?.Invoke();
        }

        public void EnsureBossIsVisibleAndFragmentsAreDisabled()
        {
            if (_fractureEffect != null)
            {
                SetVisible(true);
                _fractureEffect.SetFragmentsEnabled(false);
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

            if (_meshTrailEffect)
            {
                _meshTrailEffect.UpdateVelocity(_mc.Motor.Velocity);
            }
        }

        public void ThrowSpike(Action onComplete = null)
        {
            if (_animCoroutine != null)
            {
                StopCoroutine(_animCoroutine);
            }
            _fakeSpikeController.Form();
            PlayThrowPrepareAudio();
            _animCoroutine = StartCoroutine(PlayThrowAnimAndCallback("Throw", onComplete));
        }
        
        private IEnumerator PlayThrowAnimAndCallback(string stateName, Action onComplete)
        {
            _animator.Play(stateName);
            
            SetFlashEnabled(true, 0.1f);
            yield return new WaitUntil(() => _fakeSpikeController.IsFormed());
            _animator.speed = 1;
            SetFlashEnabled(false);
            yield return null;
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(stateInfo.normalizedTime * stateInfo.length);

            // Invoke the callback
            onComplete?.Invoke();
        }
        
        
        private IEnumerator PlayAnimAndCallback(string stateName, Action onComplete)
        {
            _animator.Play(stateName);
            
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(stateInfo.length);

            // Invoke the callback
            onComplete?.Invoke();
        }
        
        
        public void Punch(Action onComplete = null)
        {
            if (_animCoroutine != null)
            {
                StopCoroutine(_animCoroutine);
            }
            _animCoroutine = StartCoroutine(PlayAnimAndCallback("Punch", onComplete));
        }


        private void OnGolemThrowEvent()
        {
            _fakeSpikeController.Hide();
            PlayThrowAudio();

            if (handTransform != null)
            {
                OnThrowEvent?.Invoke(handTransform);
            }
            else
            {
                Debug.LogError("[BossAnimator] Hand transform is not assigned.");
            }
        }
        
        private void OnGolemPrepareThrowEvent()
        {
            if (_animator != null)
            {
                if (!_fakeSpikeController.IsFormed())
                {
                    _animator.speed = 0;
                }
            }
            else
            {
                Debug.LogError("[BossAnimator] Animator component not found!");
            }
        }
        
        private void PlayThrowAudio()
        {
            if (_audioSource != null && throwAudioClip.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, throwAudioClip.Length);
                _audioSource.PlayOneShot(throwAudioClip[randomIndex]);
            }
            else
            {
                Debug.LogWarning("[BossAnimator] AudioSource component not found or no audio clips assigned!");
            }
        }
        
        private void PlayThrowPrepareAudio()
        {
            if (_audioSource != null && throwPrepareAudioClip.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, throwPrepareAudioClip.Length);
                _audioSource.PlayOneShot(throwPrepareAudioClip[randomIndex]);
            }
            else
            {
                Debug.LogWarning("[BossAnimator] AudioSource component not found or no audio clips assigned!");
            }
        }
        
        private void PlayMoveAudio()
        {
            if (_audioSource != null && moveAudioClips.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, moveAudioClips.Length);
                _audioSource.PlayOneShot(moveAudioClips[randomIndex]);
            }
            else
            {
                Debug.LogWarning("[BossAnimator] AudioSource component not found or no audio clips assigned!");
            }
        }
    }
}