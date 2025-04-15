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
        
        public GameObject coreObject;
        private IVisibilityTransitionEffect coreDissolver;

        private Coroutine _currentCoroutine;
        private Coroutine _animCoroutine;

        public AudioSource sfxAudioSource;
        public AudioSource secondarySfxAudioSource;
        public AudioSource ambientAudioSource;

        public AudioClip tpInAudioClip;
        public AudioClip tpOutAudioClip;
        public AudioClip[] throwAudioClip;
        public AudioClip[] throwPrepareAudioClip;
        public AudioClip[] moveAudioClips;

        public AudioClip[] voiceAudioClips;
        public AudioClip[] chargeAudioClips;
        public AudioClip[] dashAudioClips;
        public AudioClip[] smashAudioClips;
        public AudioClip[] punchAudioClips;
        
        public AudioClip deathAudioClip;
        public AudioClip[] hitAudioClips;
        
        public AudioClip ambientLoopAudioClip;
        

        private Vector3 _targetPos;
        private Quaternion _targetRotation;
        
        public GameObject fakeSpikePrefab;
        private FakeSpikeController _fakeSpikeController;
        
        private VisualEffect[] _visualEffects;

        public Transform handTransform;

        private EntityMovementController _mc;
        
        public VisualEffect damageVFX;

        private FlashOptions _chargingFlashOptions = new FlashOptions
        {
            UseFade = false,
            BaseAlpha = 0.5f,
            Interval = 0.1f,
        };
        
        private FlashOptions _hurtFlashOptions = new FlashOptions
        {
            UseFade = true,
            FadeTime = 0.25f,
            BaseAlpha = 0.5f,
            Interval = 0.1f,
            ColorOverride = Color.red,
        };
        
        public event Action<Transform> OnThrowEvent;
        public event Action OnGroundAttackEvent;
        public event Action<Transform> OnPunchEvent;
        
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

            _meshFlashEffect.SetOptions(_chargingFlashOptions);

            if (sfxAudioSource == null || ambientAudioSource == null)
            {
                Debug.LogError("[BossAnimator] AudioSource components not found!");
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

            if (coreObject)
            {
                coreDissolver = coreObject.GetComponent<IVisibilityTransitionEffect>();
            }
            
        }

        public void UpdateTarget(Vector3 targetPos, Quaternion targetRotation)
        {
            _targetPos = targetPos;
            _targetRotation = targetRotation;
        }
        
        public void SetVisible(bool isVisible)
        {
            if (_fractureEffect)
            {
                _fractureEffect.SwapVisibility(isVisible);
            }
            foreach (var vfx in _visualEffects)
            {
                if (!vfx) continue;
                // if (!vfx.enabled) continue;
                if (isVisible)
                {
                    // vfx.Play();
                    vfx.enabled = true;
                }
                else
                {
                    vfx.enabled = false;
                    // vfx.Stop();
                }
            }
        }

        public void ResetAnimator()
        {
            _animator.Play("Dormant");
            _animator.speed = 1;
            SetChargingFlashEnabled(false);
            _fakeSpikeController.Hide();
            _fractureEffect.SetFragmentsEnabled(true);
            _fractureEffect.ResetFragmentsAtSource();
            _fractureEffect.SetGravityEnabled(true);

            SetVisible(false);
        }
        
        public void AbsorbCore(Transform target, Action onComplete = null)
        {
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
            }

            _currentCoroutine = StartCoroutine(AbsorbCoreRoutine(target, onComplete));
        }
        
        private IEnumerator AbsorbCoreRoutine(Transform target, Action onComplete = null)
        {
            SetChargingFlashEnabled(false);

            var core = coreObject;
            // ensure all subobjects are visible and enabled
            var absorbDistance = 3f;
            while (Vector3.Distance(core.transform.position, target.position + Vector3.up*1.5f) > absorbDistance)
            {
                core.transform.position = Vector3.MoveTowards(core.transform.position, target.position + Vector3.up*1.5f,
                    Time.deltaTime * 2f);
                yield return null;
            }
            SetChargingFlashEnabled(false);

            if (coreDissolver != null)
            {
                coreDissolver.Hide();

                while (coreDissolver.IsVisible)
                {
                    core.transform.position = Vector3.MoveTowards(core.transform.position, target.position + Vector3.up*1.5f,
                        Time.deltaTime * 2f);
                    yield return null;
                }
            }
            
            onComplete?.Invoke();
        }
        
        public void PreparePunch()
        {
            _animator.Play("PreparePunch");
            PlayWindupAudio();
        }
        
        public void ReturnToIdle()
        {
            _animator.CrossFade("Idle", 0.1f);
            // ensure fake spike is hidden
            _fakeSpikeController.Hide();
            SetChargingFlashEnabled(false);
        }
        
        public void SetChargingFlashEnabled(bool flashEnabled)
        {
            if (_meshFlashEffect)
            {
                if (flashEnabled)
                {
                    _meshFlashEffect.SetOptions(_chargingFlashOptions);
                    _meshFlashEffect.StartFlashing();
                }
                else
                {
                    _meshFlashEffect.StopFlashing();
                }
            }
        }
        
        public void HurtEffect()
        {
            if (_meshFlashEffect)
            {
                _meshFlashEffect.SetOptions(_hurtFlashOptions);
                _meshFlashEffect.FlashOnce(0.2f);
            }
            if (damageVFX)
            {
                damageVFX.enabled = true;
                damageVFX.Play();
            }
            PlayHitAudio();
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
            
            PlayAmbientLoop();
            
            PlayVoiceAudio();
            yield return PlayAnimAndCallback("Enraged", null);
            
            onComplete?.Invoke();
        }
        
        public void Despawn(Action onComplete = null)
        {
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
            }

            _currentCoroutine = StartCoroutine(DespawnRoutine(onComplete));
        }

        private IEnumerator DespawnRoutine(Action onComplete = null)
        {
            PlayDeathAudio();

            SetChargingFlashEnabled(false);
            
            _fractureEffect.SetFragmentsEnabled(true);
            _fractureEffect.SetDissolveSpeedFactor(1f);
            _fractureEffect.ResetFragmentsAtSource();
            
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            _fractureEffect.SetGravityEnabled(true);
            _fractureEffect.SetCollisionEnabled(true);
            SetVisible(false);

            yield return new WaitForFixedUpdate();

            // Set original object invisible

            yield return new WaitForSeconds(1f);
            
            _fractureEffect.SetDissolveSpeedFactor(3f);
            _fractureEffect.DissolveFragments();

            yield return new WaitUntil(_fractureEffect.AreAllFragmentsFinishedDissolving);

            // Set original object invisible

            _fractureEffect.SetFragmentsEnabled(false);
            
            onComplete?.Invoke();
        }

        public void TeleportWithExplosion(Vector3 targetPos, Quaternion targetRotation,
            float speedFactor = 1f, Action onComplete = null)
        {
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
            }

            _targetPos = targetPos;
            _targetRotation = targetRotation;
            StartCoroutine(TeleportSequence(speedFactor, onComplete));
        }

        private IEnumerator TeleportSequence(float speedFactor, Action onComplete)
        {
            _fractureEffect.SetFragmentsEnabled(true);
            _fractureEffect.SetDissolveSpeedFactor(speedFactor);

            // Reset fragments position and rotation
            _fractureEffect.SetGravityEnabled(false);
            _fractureEffect.ResetFragmentsAtSource();

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            // Set original object invisible
            SetVisible(false);

            // yield return new WaitForFixedUpdate();
            yield return StartCoroutine(
                _fractureEffect.BreakAndPauseFragmentsSmooth(explosionForce: 0.05f / speedFactor,
                    slowdownTime: 0.75f * speedFactor));

            // Play teleport sound
            PlayTeleportOutAudio();

            yield return new WaitForSeconds(0.1f * speedFactor);

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
            
            PlayTeleportInAudio();

            yield return StartCoroutine(_fractureEffect.ReassembleSmooth(0.5f * speedFactor, 0.05f * speedFactor));


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
            
            SetChargingFlashEnabled(true);
            yield return new WaitUntil(() => _fakeSpikeController.IsFormed());
            _animator.speed = 1;
            SetChargingFlashEnabled(false);
            yield return null;
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(stateInfo.normalizedTime * stateInfo.length);

            // Invoke the callback
            onComplete?.Invoke();
        }
        
        public void GroundAttack(Action onComplete = null)
        {
            if (_animCoroutine != null)
            {
                StopCoroutine(_animCoroutine);
            }
            _animCoroutine = StartCoroutine(PlayAnimAndCallback("GroundAttack", onComplete));
        }
        
        public void PlayEnragedAnimAndCallback(Action onComplete = null)
        {
            if (_animCoroutine != null)
            {
                StopCoroutine(_animCoroutine);
            }
            _animCoroutine = StartCoroutine(PlayAnimAndCallback("Enraged", onComplete));
            PlayVoiceAudio();
        }
        
        private IEnumerator PlayAnimAndCallback(string stateName, Action onComplete)
        {
            _animator.Play(stateName);
            
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(stateInfo.length);

            onComplete?.Invoke();
        }
        
        
        public void Punch(Action onComplete = null)
        {
            if (_animCoroutine != null)
            {
                StopCoroutine(_animCoroutine);
            }
            _animCoroutine = StartCoroutine(PlayAnimAndCallback("Punch", onComplete));
            PlayPunchAudio();
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
            if (!_fakeSpikeController.IsFormed())
            {
                _animator.speed = 0;
            }
        }
        
        private void OnGolemGroundAttackEvent()
        {
            OnGroundAttackEvent?.Invoke();
            PlaySmashAudio();
        }
        
        private void OnGolemPunchEvent()
        {
            if (handTransform != null)
            {
                OnPunchEvent?.Invoke(handTransform);
            }
            else
            {
                Debug.LogError("[BossAnimator] Hand transform is not assigned.");
            }
        }

        private void PlayRandomSFX(AudioSource audioSource, AudioClip[] clips)
        {
            if (clips.Length <= 0) return;
            var randomIndex = UnityEngine.Random.Range(0, clips.Length);
            audioSource.PlayOneShot(clips[randomIndex]);
        }
        
        private void PlayPrimarySFX(AudioClip[] clips)
        {
            if (sfxAudioSource == null) return;
            PlayRandomSFX(sfxAudioSource, clips);
        }
        
        private void PlayPrimarySFX(AudioClip clip)
        {
            if (sfxAudioSource == null) return;
            if (clip == null) return;
            sfxAudioSource.PlayOneShot(clip);
        }
        
        private void PlaySecondarySFX(AudioClip[] clips)
        {
            if (secondarySfxAudioSource == null) return;
            PlayRandomSFX(secondarySfxAudioSource, clips);
        }
        
        private void PlaySecondarySFX(AudioClip clip)
        {
            if (secondarySfxAudioSource == null) return;
            if (clip == null) return;
            secondarySfxAudioSource.PlayOneShot(clip);
        }

        public void PlayAmbientLoop()
        {
            if (ambientAudioSource == null) return;
            if (ambientLoopAudioClip == null) return;
            ambientAudioSource.clip = ambientLoopAudioClip;
            ambientAudioSource.loop = true;
            ambientAudioSource.Play();
        }
        
        public void StopAmbientLoop()
        {
            if (ambientAudioSource == null) return;
            ambientAudioSource.Stop();
        }
        
        public void PlayThrowAudio()
        {
            PlayPrimarySFX(throwAudioClip);
        }
        
        public void PlayThrowPrepareAudio()
        {
            PlayPrimarySFX(throwPrepareAudioClip);
        }
        
        public void PlayMoveAudio()
        {
            PlayPrimarySFX(moveAudioClips);
        }
        
        public void PlayVoiceAudio()
        {
            PlayPrimarySFX(voiceAudioClips);
        }
        
        public void PlayChargeAudio()
        {
            PlayPrimarySFX(chargeAudioClips);
        }
        
        public void PlayDashAudio()
        {
            PlayPrimarySFX(dashAudioClips);
        }
        
        public void PlaySmashAudio()
        {
            PlayPrimarySFX(smashAudioClips);
        }
        
        public void PlayPunchAudio()
        {
            PlayPrimarySFX(punchAudioClips);
        }
        
        public void PlayDeathAudio()
        {
            PlayPrimarySFX(deathAudioClip);
        }
        
        public void PlayHitAudio()
        {
            PlaySecondarySFX(hitAudioClips);
        }
        
        private void PlayTeleportInAudio()
        {
            PlaySecondarySFX(tpInAudioClip);
        }
        
        private void PlayTeleportOutAudio()
        {
            PlaySecondarySFX(tpOutAudioClip);
        }
        
        public void PlayWindupAudio()
        {
            PlaySecondarySFX(chargeAudioClips);
        }
    }
}