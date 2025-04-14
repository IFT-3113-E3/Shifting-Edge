using System;
using System.Collections;
using Status;
using UnityEngine;

// Info on a triggered slash geometry
// used to generate attack hitboxes
public struct SlashInfo
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    public Vector3 startDirection;
    public Vector3 endDirection;
    public float alignment;
    public SlashSegment[] segments;
}

public class DynamicSwordSlash : MonoBehaviour
{
    [Header("Slash Detection Settings")]
    public float motionThreshold = 1.0f;
    public float cooldown = 0.2f;
    public int followUpFrames = 1;
    public SlashAnimationConfig slashConfig;
    public Transform _swordTransform;
    public GameObject slashPrefab;

    private Vector3 _previousBladeDirection;
    private Vector3 _motionDirection;
    private float _currentVelocity;

    private float _slashTimer;
    private bool _isSlashing;
    

    
    private float _cooldownTimer;
    
    
    private Action<SlashInfo> _onSlashTriggered;
    
    void Start()
    {
        if (_swordTransform == null)
        {
            Debug.LogError("Sword Transform is not assigned!");
            return;
        }

        if (slashPrefab == null)
        {
            Debug.LogError("Slash Prefab is not assigned!");
            return;
        }

        _lastBladePosition = _swordTransform.position;
        _lastLocalBladePosition = transform.InverseTransformPoint(_swordTransform.position);
        _lastBladeDirection = _swordTransform.forward;
    }

    private Vector3 _lastBladePosition;
    private Vector3 _lastLocalBladePosition;
    private Vector3 _lastBladeDirection;

    void Update()
    {
        _cooldownTimer -= Time.deltaTime;

        Vector3 localBladePosition = transform.InverseTransformPoint(_swordTransform.position);

        Vector3 movementDelta = localBladePosition - _lastLocalBladePosition;
        float distanceMoved = movementDelta.magnitude;

        if (distanceMoved > Mathf.Epsilon && _cooldownTimer <= 0f)
        {
            Vector3 bladeDirection = transform.InverseTransformDirection(_swordTransform.right);
            float bladeAlignedMovement = Vector3.Dot(movementDelta.normalized, bladeDirection);

            if (Mathf.Abs(bladeAlignedMovement) <= 1f && distanceMoved >= motionThreshold)
            {
                Vector3 prevPos = _lastBladePosition;
                Vector3 currPos = _swordTransform.position;
                Vector3 prevDir = _lastBladeDirection;
                Vector3 currDir = _swordTransform.forward;
                
                if (prevPos == currPos && prevDir == currDir)
                {
                    return;
                }
                
                TriggerSlashEffect(prevPos, currPos, prevDir, currDir, bladeAlignedMovement);
                _cooldownTimer = cooldown;
            }
        }

        _lastBladePosition = _swordTransform.position;
        _lastBladeDirection = _swordTransform.forward;
        _lastLocalBladePosition = localBladePosition;
    }


    void TriggerSlashEffect(Vector3 start, Vector3 end, Vector3 startDirection, Vector3 endDirection, float alignment)
    {
        StartCoroutine(SlashAnimSwordDrag(start, end, startDirection, endDirection, alignment));
    }
    
    IEnumerator SlashAnimSwordDrag(Vector3 start, Vector3 end, Vector3 startDirection, Vector3 endDirection, float alignment)
    {
        GameObject slashObject = Instantiate(slashPrefab, transform.position, Quaternion.identity);
        var slashHitbox = slashObject.GetComponent<DamageHitbox>();
        if (!slashHitbox)
        {
            Debug.LogError("No DamageHitbox found on the slash prefab!");
            yield break;
        }
        slashHitbox.owner = gameObject;
        var slashAnimator = slashObject.GetComponent<SwordSlashAnimator>();
        if (!slashAnimator)
        {
            Debug.LogError("No SwordSlashAnimator found on the slash prefab!");
            yield break;
        }
        
        slashAnimator.name = "Dynamic Sword Slash Animator";
        slashAnimator.transform.SetParent(null, false);

        slashAnimator.transform.position = transform.position;

        bool isCompleted = false;
        
        slashAnimator.Configure(slashConfig);
        slashAnimator.SetupSlash(start, end, startDirection, endDirection, alignment < 0f);
        slashAnimator.PlaySlash(() => isCompleted = true);

        if (_onSlashTriggered != null)
        {
            SlashInfo slashInfo = new SlashInfo
            {
                startPosition = start,
                endPosition = end,
                startDirection = startDirection,
                endDirection = endDirection,
                alignment = alignment,
                segments = slashAnimator.GenerateGeometry()
            };
            _onSlashTriggered.Invoke(slashInfo);            
        }

        Vector3 lastLocalEnd = transform.InverseTransformPoint(end);
        Vector3 lastEndDir = endDirection;
        
        for (int i = 0; i < followUpFrames; i++)
        {
            yield return 0;
            
            if (isCompleted)
            {
                slashAnimator.gameObject.SetActive(false);
                Destroy(slashAnimator.gameObject);
                yield break;
            }
            
            slashAnimator.transform.position = transform.position;

            Vector3 currentPosition = _swordTransform.position;
            Vector3 currentDirection = _swordTransform.forward;
            
            Vector3 localCurrentPosition = transform.InverseTransformPoint(currentPosition);
            // if currentPositon and currentDirection are too far from the original end configuration, dont update
            if (Vector3.Distance(localCurrentPosition, lastLocalEnd) < 0.1f &&
                Vector3.Distance(currentDirection, lastEndDir) < 0.5f)
            {
                slashAnimator.SetupSlash(start, currentPosition, startDirection, currentDirection, alignment < 0f);
                lastLocalEnd = localCurrentPosition;
                lastEndDir = currentDirection;
            }

        }
        
        yield return new WaitUntil(() => isCompleted);
        slashAnimator.gameObject.SetActive(false);
        Destroy(slashAnimator.gameObject);
    }
    
    public void SetOnSlashTriggered(Action<SlashInfo> onSlashTriggered)
    {
        _onSlashTriggered = onSlashTriggered;
    }
}
