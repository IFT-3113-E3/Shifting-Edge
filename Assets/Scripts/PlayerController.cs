using UnityEngine;
using UnityEngine.InputSystem;

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

    private InputSystem_Actions _playerControls;

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

    private void Awake()
    {
        _playerControls = new InputSystem_Actions();

        _moveAction = _playerControls.Player.Move;
        _moveAction.Enable();

        _jumpAction = _playerControls.Player.Jump;
        _jumpAction.Enable();

        _attackAction = _playerControls.Player.Attack;
        _attackAction.Enable();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _jumpAction.Disable();
        _attackAction.Disable();
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

        // if (!_cc)
        //     Debug.LogWarning("CharacterController is required on the PlayerController");

        if (!_cm)
            Debug.LogWarning("ComboManager is required on the PlayerController");

        if (_cm)
        {
            _cm.OnComboStep += PlayAttack;
            _cm.OnComboEnd += BreakCombo;
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
        Vector2 input = new(_inputHorizontal, _inputVertical);
        if (input.magnitude > 0)
        {
            float angle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
        _movementLocked = true;

        _animator.ResetTrigger(BreakAttack);
        _animator.SetTrigger(Attack);
    }

    void BreakCombo(bool cancelled, string debug)
    {
        _movementLocked = false;
        // Debug.LogError($"Combo broken: {debug}, cancelled: {cancelled}");
        _isInCombo = false;
        if (_animator)
        {
            _animator.ResetTrigger(Attack);
            _animator.SetTrigger(BreakAttack);
        }
    }

    public void OnAttackAnimationEnd()
    {
        _movementLocked = false;
        _cm.OnAttackEnd();
    }

    public void OnAttackDash()
    {
        // dash a little bit forward
        float force = new Vector2(_inputHorizontal, _inputVertical).normalized.magnitude * 4.5f;
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
            _externalForce += force;
        }
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        // create a knockback force going up in the air in the direction of the hit
        var height = force + gravity;
        var knockback = new Vector3(direction.x, height, direction.y) * force;
        ApplyExternalForce(knockback, true);
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

        // if ( _inputCrouch )
        //     _isCrouching = !_isCrouching;

        if ( _mc.IsGrounded && _animator )
        {
            // _animator.SetBool(Crouch, _isCrouching);

            float minimumSpeed = 0.9f;
            _animator.SetBool(Run, _mc.Velocity.magnitude > minimumSpeed );

            // _isSprinting = _mc.velocity.magnitude > minimumSpeed && _inputSprint;
            // _animator.SetBool(Sprint, _isSprinting );

        }

        // prevent air from spamming from anystate
        if (_animator)
        {
            _animator.SetBool(Air, !_mc.IsGrounded);
        }

        if ( _inputJump && _mc.IsGrounded )
        {
            _isJumping = true;
        }

        if (_inputAttack && _mc.IsGrounded)
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

        // HeadHittingDetect();

        MovementUpdate();
    }


    private void MovementUpdate()
    {
        Vector3 inputVector = new(_inputHorizontal, 0, _inputVertical);
        inputVector = Vector3.ClampMagnitude(inputVector, 1f);

        if (_movementLocked)
        {
            inputVector = Vector3.zero;
        }

        Vector3 moveDirection = Vector3.zero;
        if (inputVector.magnitude > 0.1)
        {
            Vector3 camForward  = _camera.transform.forward;
            camForward.y = 0;
            camForward.Normalize();
            Vector3 camRight = _camera.transform.right;
            camRight.y = 0;
            camRight.Normalize();
            moveDirection = (camForward * inputVector.z + camRight * inputVector.x).normalized;

            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, Mathf.Round(targetAngle / 45f) * 45f, 0);
        }

        // Apply the movement to the character controller
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

        float velocityAddition = velocity;
        if ( _isSprinting )
            velocityAddition += sprintAdittion;
        if (_isCrouching)
            velocityAddition -= velocity * 0.50f;

        Vector3 horizontalVelocity = moveDirection * velocityAddition;
        if ( _isJumping )
        {

            // Apply inertia and smoothness when climbing the jump
            // It is not necessary when descending, as gravity itself will gradually pulls
            float verticalVelocity = Mathf.SmoothStep(jumpForce, jumpForce * 0.30f, _jumpElapsedTime / jumpTime);

            // Jump timer
            _jumpElapsedTime += Time.deltaTime;
            if (_jumpElapsedTime >= jumpTime)
            {
                _isJumping = false;
                _jumpElapsedTime = 0;
            }

            // Add gravity to Y axis
            if (!_mc.IsGrounded)
            {
                verticalVelocity -= gravity;
            }
            else
            {
                verticalVelocity -= 1f; // to keep the character grounded
            }

            // Apply external force
            if (_externalForce.magnitude > 0)
            {
                _externalForce = Vector3.Lerp(_externalForce, Vector3.zero, Time.deltaTime * 5f);
            }

            Vector3 finalVelocity = horizontalVelocity + Vector3.up * verticalVelocity + _externalForce;

            // Apply the final velocity to the character controller
            _mc.MoveTo(finalVelocity * Time.deltaTime);
        }
    }

    // void HeadHittingDetect()
    // {
    //     float headHitDistance = 1.1f;
    //     Vector3 ccCenter = transform.TransformPoint(_cc.center);
    //     float hitCalc = _cc.height / 2f * headHitDistance;

    //     if (Physics.Raycast(ccCenter, Vector3.up, hitCalc))
    //     {
    //         _jumpElapsedTime = 0;
    //         _isJumping = false;
    //     }
    // }

    private void OnDestroy()
    {
        if (_cm)
        {
            _cm.OnComboStep -= PlayAttack;
            _cm.OnComboEnd -= BreakCombo;
        }
    }
}
