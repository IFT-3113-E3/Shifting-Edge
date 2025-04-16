
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // private static readonly int Crouch = Animator.StringToHash("crouch");
    private static readonly int Run = Animator.StringToHash("isRunning");
    // private static readonly int Sprint = Animator.StringToHash("isSprinting");
    private static readonly int Air = Animator.StringToHash("isFalling");
    private static readonly int AttackTrigger = Animator.StringToHash("lightAttack");
    private static readonly int BreakAttackTrigger = Animator.StringToHash("breakAttack");
    private static readonly int RollTrigger = Animator.StringToHash("roll");
    
    [SerializeField] private float rollForce = 15f;
    [SerializeField] private float rollCooldownTime = 0.5f; // short delay after 3 rolls
    [SerializeField] private int maxConsecutiveRolls = 3;

    private bool _isRolling = false;
    private float _rollCooldownTimer = 0f;
    private int _rollsRemaining = 3;
    private bool _rollLockedOut = false; // New: lockout flag
    private Vector3 _rollDirection = Vector3.zero;
    private float _rollForceMultiplier = 1f;

    
    private InputSystem_Actions _playerControls;

    private float _jumpElapsedTime = 0;

    // Player states
    // private bool _isJumping = false;
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
    private bool _inputRoll;

    private Animator _animator;
    // private CharacterController _cc;
    private EntityMovementController _mc;
    private Camera _camera;
    private ComboManager _cm;
    private DynamicSwordSlash _slashController;
    
    private Vector3 _externalForce = Vector3.zero;
    private bool _movementLocked;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _attackAction;
    private InputAction _rollAction;
    
    private void OnEnable()
    {
        _playerControls = new InputSystem_Actions();
        
        _moveAction = _playerControls.Player.Move;
        _moveAction.Enable();
        
        _jumpAction = _playerControls.Player.Jump;
        _jumpAction.Enable();
        
        _attackAction = _playerControls.Player.Attack;
        _attackAction.Enable();
        
        _rollAction = _playerControls.Player.Roll;
        _rollAction.Enable();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _jumpAction.Disable();
        _attackAction.Disable();
        _rollAction.Disable();
    }

    void Start()
    {
        _camera = Camera.main;
        // _cc = GetComponent<CharacterController>();
        _mc = GetComponent<EntityMovementController>();
        _animator = GetComponent<Animator>();
        _cm = GetComponent<ComboManager>();
        _slashController = GetComponent<DynamicSwordSlash>();

        if (!_animator)
            Debug.LogWarning("Animator is required on the PlayerController");
        
        if (!_mc)
            Debug.LogWarning("EntityMovementController is required on the PlayerController");

        if (!_cm)
            Debug.LogWarning("ComboManager is required on the PlayerController");

        if (_cm)
        {
            _cm.OnComboStep += PlayAttack;
            _cm.OnComboEnd += OnBreakCombo;
        }

        if (_slashController)
        {
            _slashController.SetOnSlashTriggered(OnSlashInfo);
        }
    }

    public int FPS = 12;
    private float _time;
    
    void OnSlashInfo(SlashInfo slashInfo)
    {
        // draw the debug slash segments
        for (int i = 0; i < slashInfo.segments.Length; i++)
        {
            var segment = slashInfo.segments[i];
            var basePos = transform.position + segment.basePosition;
            var tipPos = transform.position + segment.tipPosition;
            Debug.DrawLine(basePos, tipPos, Color.red, 1f);
        }
        
    }
    
    void PlayAttack(int i)
    {
        // reorient the player to the last input direction before the next attack
        Vector2 input = new Vector2(_inputHorizontal, _inputVertical);
        if (input.magnitude > 0)
        {
            float angle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
        _movementLocked = true;

        _animator.ResetTrigger(BreakAttackTrigger);
        _animator.SetTrigger(AttackTrigger);
    }
    
    void OnBreakCombo(bool cancelled, string debug)
    {
        _movementLocked = false;
        // Debug.LogError($"Combo broken: {debug}, cancelled: {cancelled}");
        _isInCombo = false;
        if (_animator)
        {
            _animator.ResetTrigger(AttackTrigger);
            _animator.SetTrigger(BreakAttackTrigger);
            // _animator.Play("Idle");
        }
    }

    // TEMP: find a better way to do this
    private Vector3 GetCameraRelativeDirection(float inputX, float inputZ)
    {
        Vector3 inputDir = new Vector3(inputX, 0f, inputZ);

        if (inputDir.sqrMagnitude < 0.01f)
            return Vector3.zero;

        Vector3 camForward = _camera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = _camera.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        return (camForward * inputDir.z + camRight * inputDir.x).normalized;
    }

    private void Roll()
    {
        // cancel attacks if rolling
        if (_isInCombo)
        {
            _cm.ResetCombo("Rolled");
        }

        _movementLocked = true;
        _isRolling = true;

        _animator.ResetTrigger(AttackTrigger);
        _animator.ResetTrigger(BreakAttackTrigger);
        _animator.SetTrigger(RollTrigger);

        Vector3 rollDirection = GetCameraRelativeDirection(_inputHorizontal, _inputVertical);
        if (rollDirection.sqrMagnitude < 0.1f)
        {
            rollDirection = transform.forward;
        }
        else
        {
            rollDirection.Normalize();
            _mc.SetRotation(Quaternion.LookRotation(rollDirection));
        }
        _rollDirection = rollDirection;
    }

    public void OnRollAnimationEnd()
    {
        _isRolling = false;
        _movementLocked = false;
        _animator.ResetTrigger(RollTrigger);
    }
    
    public void OnAttackAnimationEnd()
    {
        _movementLocked = false;
        _cm.OnAttackEnd();
    }

    public void OnAttackDash()
    {
        // dash a little bit forward
        float force = new Vector2(_inputHorizontal, _inputVertical).normalized.magnitude * 7f;
        ApplyExternalForce(
            new Vector3(0, -2f, force),
            true, true);
    }
   
    public void ApplyExternalForce(Vector3 force, bool immediate = false, bool local = false)
    {
        // apply force to the player
        if (local)
        {
            force = transform.TransformDirection(force);
        }

        if (immediate)
        {
            _mc.SetVelocity(force);
        }
        else
        {
            _mc.AddVelocity(force);
        }
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
        
        _inputHorizontal = _moveAction.ReadValue<Vector2>().x;
        _inputVertical = _moveAction.ReadValue<Vector2>().y;
        _inputJump = _jumpAction.triggered;
        // _inputSprint = Mathf.Approximately(Input.GetAxis("Fire3"), 1f);
        // _inputCrouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton1);
        _inputAttack = _attackAction.triggered;
        _inputRoll = _rollAction.triggered;

        // if ( _inputCrouch )
        //     _isCrouching = !_isCrouching;

        if ( _mc.IsGrounded && _animator && !_isRolling)
        {
            // _animator.SetBool(Crouch, _isCrouching);
            
            float minimumSpeed = 0.9f;
            _animator.SetBool(Run, _mc.Velocity.magnitude > minimumSpeed );

            // _isSprinting = _cc.velocity.magnitude > minimumSpeed && _inputSprint;
            // _animator.SetBool(Sprint, _isSprinting );
        }

        // prevent air from spamming from anystate
        if (_animator && !_isRolling)
        {
            _animator.SetBool(Air, !_mc.IsGrounded );
        }
        
        if (_inputAttack && _mc.IsGrounded && !_isRolling)
        {
            _cm.TryAttack();
            _isInCombo = true;
        }

        if (!_mc.IsGrounded && _isInCombo)
        {
            _cm.ResetCombo("Player is not grounded");
        }
        
        if (!_cm.IsAttacking && _isInCombo && (_inputHorizontal != 0 || _inputVertical != 0))
        {
            _cm.ResetCombo("Player is moving while attacking");
        }
        
        MovementUpdate();
        
        // Update cooldown
        if (!_isRolling)
        {
            _rollCooldownTimer += Time.deltaTime;
            if (_rollCooldownTimer >= rollCooldownTime)
            {
                _rollsRemaining = maxConsecutiveRolls;
                _rollLockedOut = false;
                _rollCooldownTimer = 0f;
                _rollForceMultiplier = 1f;
            }
        }

        if (_inputRoll && _mc.IsGrounded && !_isRolling)
        {
            if (_rollsRemaining > 0)
            {
                Roll();
                _rollsRemaining--;
                if (_rollsRemaining == 0)
                {
                    _rollLockedOut = true;
                    _rollCooldownTimer = 0f;
                }
                _rollCooldownTimer = 0f;
            }
            else if (_rollLockedOut)
            {
                // impair the rolling movement if the player is locked out
                _rollForceMultiplier = 0.25f;
                _rollCooldownTimer = 0f;
                Roll();
            }
        }


    }


    private void MovementUpdate()
    {
        if (_isRolling)
        {
            _mc.CancelVelocity();
            ApplyExternalForce(_rollDirection * (rollForce * _rollForceMultiplier), true);
        }
        var inputHorizontal = _movementLocked ? 0 : _inputHorizontal;
        var inputVertical = _movementLocked ? 0 : _inputVertical;
        var inputJump = !_movementLocked && _inputJump;
        var inputs = new PlayerCharacterInputs
        {
            CameraRotation = _camera.transform.rotation,
            JumpDown = inputJump,
            MoveAxisForward = inputVertical,
            MoveAxisRight = inputHorizontal,
        };
        _mc.SetInputs(ref inputs);
    }

    private void OnDestroy()
    {
        if (_cm)
        {
            _cm.OnComboStep -= PlayAttack;
            _cm.OnComboEnd -= OnBreakCombo;
        }
    }
}
