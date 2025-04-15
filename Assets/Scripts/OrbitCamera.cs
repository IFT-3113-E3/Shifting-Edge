using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    public new PixelPerfect.PixelPerfectCamera camera;
    
    public Transform target; // The object the camera orbits around
    
    public float rotationSpeed = 0.5f; // Speed of transition for orbiting
    public float followSmoothness = 5f; // Smoothing for following movement
    public float distance = 5f; // Distance from the target
    public float entranceDuration = 2f; // Duration of the entrance transition

    private Vector3 _currentRotation;
    private Vector3 _targetRotation;
    private Vector3 _targetPosition;
    private Vector3 _velocity = Vector3.zero;
    private Coroutine _currentTransition;
    private bool _isDragging = false;
    private Vector3 _lastMousePosition;
    
    [SerializeField] private List<Transform> additionalTargets = new();
    [SerializeField] private float targetPadding = 2f;

    public void SetMainTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void AddAdditionalTarget(Transform newTarget)
    {
        if (!additionalTargets.Contains(newTarget))
        {
            additionalTargets.Add(newTarget);
        }
    }
    
    public void RemoveAdditionalTarget(Transform targetToRemove)
    {
        if (additionalTargets.Contains(targetToRemove))
        {
            additionalTargets.Remove(targetToRemove);
        }
    }
    
    public void ClearAdditionalTargets()
    {
        additionalTargets.Clear();
    }
    
    private void Start()
    {
        _currentRotation = new Vector3(30, 0, 0); // Default starting angle
        _targetRotation = _currentRotation;
        _targetPosition = target.position;

        // _currentTransition = StartCoroutine(EntranceTransition());
    }

    private void Update()
    {
        HandleOrbitInput();
        UpdateCameraPosition();
    }

    private void HandleOrbitInput()
    {
        // if (Input.GetMouseButtonDown(1))
        // {
        //     _isDragging = true;
        //     _lastMousePosition = Input.mousePosition;
        //     Cursor.lockState = CursorLockMode.Locked;
        //     Cursor.visible = false;
        // }
        // else if (Input.GetMouseButtonUp(1))
        // {
        //     _isDragging = false;
        //     SnapToNearestAngle();
        //     Cursor.lockState = CursorLockMode.None;
        //     Cursor.visible = true;
        // }

        if (_isDragging)
        {
            Vector3 delta = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
            _targetRotation.y += delta.x * rotationSpeed;
            _targetRotation.x -= delta.y * rotationSpeed;
            _targetRotation.x = Mathf.Clamp(_targetRotation.x, 15f, 60f);

            StartTransition();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _targetRotation.y -= 45f;
                StartTransition();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                _targetRotation.y += 45f;
                StartTransition();
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                _targetRotation.x = Mathf.Clamp(_targetRotation.x - 15f, 15f, 60f);
                StartTransition();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                _targetRotation.x = Mathf.Clamp(_targetRotation.x + 15f, 15f, 60f);
                StartTransition();
            }
        }
    }

    private void StartTransition()
    {
        if (_currentTransition != null)
        {
            StopCoroutine(_currentTransition);
        }
        _currentTransition = StartCoroutine(SmoothRotate());
    }

    private float EaseInOutQuad(float x)
    {
        return x < 0.5f ? 2f * x * x : 1f - Mathf.Pow(-2f * x + 2f, 2) / 2f;
    }

    private IEnumerator SmoothRotate()
    {
        float t = 0f;
        Vector3 startRotation = _currentRotation;
        Vector3 startVelocity = _velocity;

        camera.pixelSnapping = false;
        
        while (t < 1f)
        {
            t += Time.deltaTime * rotationSpeed;
            _currentRotation = Vector3.Lerp(startRotation, _targetRotation, EaseInOutQuad(t));
            _velocity = Vector3.Lerp(startVelocity, Vector3.zero, EaseInOutQuad(t));
            UpdateCameraPosition();
            yield return null;
        }

        _currentRotation = _targetRotation;
        _velocity = Vector3.zero;
        UpdateCameraPosition();
        camera.pixelSnapping = true;
    }

    private void UpdateCameraPosition()
    {
        if (!target) return;

        // Combine all targets into a bounds
        var allTargets = new List<Transform>(additionalTargets) { target };
        Bounds bounds = new Bounds(allTargets[0].position, Vector3.zero);

        foreach (var t in allTargets)
        {
            bounds.Encapsulate(t.position + Vector3.one * targetPadding);
            bounds.Encapsulate(t.position - Vector3.one * targetPadding);
        }

        // Center is our camera's target point
        _targetPosition = Vector3.SmoothDamp(_targetPosition, bounds.center, ref _velocity, 1f / followSmoothness);

        // Maintain current rotation logic
        Quaternion rotation = Quaternion.Euler(_currentRotation);
        Vector3 lookOffset = new Vector3(0, 0.5f, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        Vector3 desiredPosition = _targetPosition + offset + lookOffset;

        transform.position = desiredPosition;
        transform.LookAt(_targetPosition + lookOffset);

        // Optional: Clamp camera to ensure main target is within view bounds (2D screen space clamp)
        ClampMainTargetVisibility(bounds);
    }


    private void SnapToNearestAngle()
    {
        _targetRotation.y = Mathf.Round(_targetRotation.y / 45f) * 45f;
        _targetRotation.x = Mathf.Round(_targetRotation.x / 15f) * 15f;
        StartTransition();
    }

    private IEnumerator EntranceTransition()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = _targetPosition + Quaternion.Euler(_currentRotation) * new Vector3(0, 0, -distance);
        float elapsedTime = 0f;

        while (elapsedTime < entranceDuration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, EaseInOutQuad(elapsedTime / entranceDuration));
            transform.LookAt(_targetPosition);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        transform.LookAt(_targetPosition);
    }
    
    private void ClampMainTargetVisibility(Bounds bounds)
    {
        if (!target || camera == null) return;

        float cameraHeight = camera.ppu > 0 ? camera.RefResolutionX / (float)camera.ppu : 10f;
        float cameraWidth = cameraHeight * camera.RefResolutionX / camera.RefResolutionY;

        float requiredWidth = bounds.size.x;
        float requiredHeight = bounds.size.z; // Use Z for top-down depth

        if (requiredWidth > cameraWidth || requiredHeight > cameraHeight)
        {
            // Fallback to centering on main target
            _targetPosition = Vector3.SmoothDamp(_targetPosition, target.position, ref _velocity, 1f / followSmoothness);
        }
    }

}