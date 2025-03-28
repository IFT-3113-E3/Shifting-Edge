
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // private static readonly int Crouch = Animator.StringToHash("crouch");
    private static readonly int Run = Animator.StringToHash("isRunning");
    // private static readonly int Sprint = Animator.StringToHash("isSprinting");
    private static readonly int Air = Animator.StringToHash("isFalling");
    private static readonly int Attack = Animator.StringToHash("lightAttack");
    private static readonly int BreakAttack = Animator.StringToHash("breakAttack");

    [Tooltip("Speed at which the character moves. It is not affected by gravity or jumping.")]
    public float velocity = 5f;
    [Tooltip("This value is added to the speed value while the character is sprinting.")]
    public float sprintAdittion = 3.5f;
    [Tooltip("The higher the value, the higher the character will jump.")]
    public float jumpForce = 18f;
    [Tooltip("Stay in the air. The higher the value, the longer the character floats before falling.")]
    public float jumpTime = 0.85f;
    [Space]
    [Tooltip("Force that pulls the player down. Changing this value causes all movement, jumping and falling to be changed as well.")]
    public float gravity = 9.8f;

    private float _jumpElapsedTime = 0;

    // Player states
    private bool _isJumping = false;
    private bool _isSprinting = false;
    private bool _isCrouching = false;
    private bool _isInCombo = false;

    // Inputs
    private float _inputHorizontal;
    private float _inputVertical;
    private bool _inputJump;
    private bool _inputAttack;
    private bool _inputCrouch;
    private bool _inputSprint;

    private Animator _animator;
    private CharacterController _cc;
    private Camera _camera;
    private ComboManager _cm;
    
    private Vector2 attackVelocity = Vector2.zero;
    
    void Start()
    {
        _camera = Camera.main;
        _cc = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _cm = GetComponent<ComboManager>();

        if (!_animator)
            Debug.LogWarning("Animator is required on the PlayerController");
        
        if (!_cc)
            Debug.LogWarning("CharacterController is required on the PlayerController");
        
        if (!_cm)
            Debug.LogWarning("ComboManager is required on the PlayerController");

        if (_cm)
        {
            _cm.OnComboStep += PlayAttack;
            _cm.OnComboEnd += BreakCombo;
        }
    }

    public int FPS = 12;
    private float _time;
    
    void PlayAttack(int i)
    {
        // reorient the player to the last input direction before the next attack
        Vector2 input = new Vector2(_inputHorizontal, _inputVertical);
        if (input.magnitude > 0)
        {
            float angle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
        _animator.ResetTrigger(BreakAttack);
        _animator.SetTrigger(Attack);
    }
    
    void BreakCombo(bool cancelled, string debug)
    {
        Debug.LogError($"Combo broken: {debug}, cancelled: {cancelled}");
        _isInCombo = false;
        if (_animator)
        {
            _animator.ResetTrigger(Attack);
            _animator.SetTrigger(BreakAttack);  
        }
    }
    
    public void OnAttackAnimationEnd()
    {
        _cm.OnAttackEnd();
    }

    public void OnAttackDash()
    {
        // dash a little bit forward
        
        attackVelocity = new Vector2(_inputHorizontal, _inputVertical) * 2.5f;
    }
    
    void Update()
    {
        _time += Time.deltaTime;
        var updateTime = 1f / FPS;
        _animator.speed = 0;

        if (_time > updateTime)
        {
            _time -= updateTime;
            _animator.speed = updateTime / Time.deltaTime;
        }
        
        _inputHorizontal = Input.GetAxis("Horizontal");
        _inputVertical = Input.GetAxis("Vertical");
        _inputJump = Mathf.Approximately(Input.GetAxis("Jump"), 1f);
        _inputSprint = Mathf.Approximately(Input.GetAxis("Fire3"), 1f);
        _inputCrouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton1);
        _inputAttack = Input.GetKeyDown(KeyCode.Mouse0);

        // if ( _inputCrouch )
        //     _isCrouching = !_isCrouching;

        if ( _cc.isGrounded && _animator )
        {
            // _animator.SetBool(Crouch, _isCrouching);
            
            float minimumSpeed = 0.9f;
            _animator.SetBool(Run, _cc.velocity.magnitude > minimumSpeed );

            // _isSprinting = _cc.velocity.magnitude > minimumSpeed && _inputSprint;
            // _animator.SetBool(Sprint, _isSprinting );

        }

        // prevent air from spamming from anystate
        if (_animator)
        {
            _animator.SetBool(Air, _cc.isGrounded == false );
        }

        if ( _inputJump && _cc.isGrounded )
        {
            _isJumping = true;
        }
        
        if (_inputAttack && _cc.isGrounded)
        {
            _cm.TryAttack();
            _isInCombo = true;
        }

        if (!_cc.isGrounded)
        {
            _cm.ResetCombo("Player is not grounded");
        }
        HeadHittingDetect();

    }


    private void FixedUpdate()
    {
        
        float velocityAddition = 0;
        if ( _isSprinting )
            velocityAddition = sprintAdittion;
        if (_isCrouching)
            velocityAddition =  - (velocity * 0.50f);

        float horiz, vert;
        if (_cm.IsAttacking)
        {
            horiz = 0;
            vert = 0;
            if (attackVelocity.magnitude > 0)
            {
                _cc.Move(new Vector3(attackVelocity.x, 0, attackVelocity.y) * Time.deltaTime);
            }
        }
        else
        {
            if (_isInCombo && (_inputHorizontal != 0 || _inputVertical != 0))
            {
                _cm.ResetCombo("Player is moving while attacking");
            }
            horiz = _inputHorizontal;
            vert = _inputVertical;
        }
        
        if (attackVelocity.magnitude > 0)
        {
            attackVelocity = Vector2.Lerp(attackVelocity, Vector2.zero, Time.deltaTime * 5f);
        }

        
        float directionX = (horiz * (velocity + velocityAddition)) * Time.deltaTime;
        float directionZ = (vert * (velocity + velocityAddition)) * Time.deltaTime;
        float directionY = 0;

        if ( _isJumping )
        {

            // Apply inertia and smoothness when climbing the jump
            // It is not necessary when descending, as gravity itself will gradually pulls
            directionY = Mathf.SmoothStep(jumpForce, jumpForce * 0.30f, _jumpElapsedTime / jumpTime) * Time.deltaTime;

            // Jump timer
            _jumpElapsedTime += Time.deltaTime;
            if (_jumpElapsedTime >= jumpTime)
            {
                _isJumping = false;
                _jumpElapsedTime = 0;
            }
        }

        // Add gravity to Y axis
        directionY -= gravity * Time.deltaTime;

        
        if (!_camera) return;
        Vector3 forward = _camera.transform.forward;
        Vector3 right = _camera.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        // Relate the front with the Z direction (depth) and right with X (lateral movement)
        forward *= directionZ;
        right *= directionX;

        if (directionX != 0 || directionZ != 0)
        {
            float angle = Mathf.Atan2(forward.x + right.x, forward.z + right.z) * Mathf.Rad2Deg;

            var rotation =
                // Snap rotation to 8 directions
                Quaternion.Euler(0, Mathf.Round(angle / 45) * 45, 0);
            
            transform.rotation = rotation;
        }
        
        Vector3 verticalDirection = Vector3.up * directionY;
        Vector3 horizontalDirection = forward + right;

        Vector3 moviment = verticalDirection + horizontalDirection;
        _cc.Move( moviment );
    }

    void HeadHittingDetect()
    {
        float headHitDistance = 1.1f;
        Vector3 ccCenter = transform.TransformPoint(_cc.center);
        float hitCalc = _cc.height / 2f * headHitDistance;

        if (Physics.Raycast(ccCenter, Vector3.up, hitCalc))
        {
            _jumpElapsedTime = 0;
            _isJumping = false;
        }
    }

    private void OnDestroy()
    {
        if (_cm)
        {
            _cm.OnComboStep -= PlayAttack;
        }
    }
}
