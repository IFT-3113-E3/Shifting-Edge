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
    
    public SwordSlashAnimator _slashAnimator;
    private Vector3 _previousBladeDirection;
    private Vector3 _motionDirection;
    private float _currentVelocity;

    private float _slashTimer;
    private bool _isSlashing;
    
    public GameObject slashPrefab;

    
    private Vector3 _previousPosition;
    private Quaternion _previousRotation;
    private float _cooldownTimer;
    
    private Coroutine _slashCoroutine;
    
    public Transform _swordTransform;
    
    private Action<SlashInfo> _onSlashTriggered;
    
    void Start()
    {

        
        _previousPosition = _swordTransform.position;
        _previousRotation = _swordTransform.rotation;
    }

    void Update()
    {
        _cooldownTimer -= Time.deltaTime;

        Vector3 currentPosition = _swordTransform.position;
        Quaternion currentRotation = _swordTransform.rotation;

        Vector3 movementDelta = currentPosition - _previousPosition;
        float distanceMoved = movementDelta.magnitude;

        if (distanceMoved > Mathf.Epsilon && _cooldownTimer <= 0f)
        {
            Vector3 bladeDirection = _swordTransform.right;

            float bladeAlignedMovement = Vector3.Dot(movementDelta.normalized, bladeDirection);

            if (Mathf.Abs(bladeAlignedMovement) <= 1f && distanceMoved >= motionThreshold)
            {
                TriggerSlashEffect(_previousPosition, currentPosition, _previousRotation * Vector3.forward, currentRotation * Vector3.forward, bladeAlignedMovement);
                _cooldownTimer = cooldown;
            }
        }

        _previousPosition = currentPosition;
        _previousRotation = currentRotation;
    }

    void TriggerSlashEffect(Vector3 start, Vector3 end, Vector3 startDirection, Vector3 endDirection, float alignment)
    {
        // if (_slashCoroutine != null)
        // {
        //     StopCoroutine(_slashCoroutine);
        // }
        _slashCoroutine = StartCoroutine(SlashAnimSwordDrag(start, end, startDirection, endDirection, alignment));
    }
    
    IEnumerator SlashAnimSwordDrag(Vector3 start, Vector3 end, Vector3 startDirection, Vector3 endDirection, float alignment)
    {
        GameObject slashObject = Instantiate(slashPrefab, transform.position, Quaternion.identity);
        var slashHitbox = slashObject.GetComponent<DamageHitbox>();
        if (slashHitbox == null)
        {
            Debug.LogError("No DamageHitbox found on the slash prefab!");
            yield break;
        }
        slashHitbox.owner = gameObject;
        var slashAnimator = slashObject.GetComponent<SwordSlashAnimator>();
        if (slashAnimator == null)
        {
            Debug.LogError("No SwordSlashAnimator found on the slash prefab!");
            yield break;
        }
        
        slashAnimator.name = "Dynamic Sword Slash Animator";
        slashAnimator.transform.SetParent(null, false);

        slashAnimator.transform.position = transform.position;

        slashAnimator.Configure(slashConfig);
        slashAnimator.SetupSlash(start, end, startDirection, endDirection, alignment < 0f);
        slashAnimator.PlaySlash();

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
        
        for (int i = 0; i < followUpFrames; i++)
        {
            yield return 0;

            
            slashAnimator.transform.position = transform.position;

            Vector3 currentPosition = _swordTransform.position;
            Vector3 currentDirection = _swordTransform.forward;
            Debug.Log($"Distance: {Vector3.Distance(currentPosition, end)}, Direction: {Vector3.Distance(currentDirection, endDirection)}");
            // if currentPositon and currentDirection are too far from the original end configuration, dont update
            if (Vector3.Distance(currentPosition, end) < 0.1f &&
                Vector3.Distance(currentDirection, endDirection) < 0.5f)
            {
                slashAnimator.SetupSlash(start, currentPosition, startDirection, currentDirection, alignment < 0f);
            }

        }
    }
    
    public void SetOnSlashTriggered(Action<SlashInfo> onSlashTriggered)
    {
        _onSlashTriggered = onSlashTriggered;
    }
}
