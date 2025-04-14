using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace MeshVFX
{
    public class MeshFragment
    {
        private readonly Transform _transform;
        private readonly Rigidbody _rb;
        private readonly Renderer _renderer;
        private Coroutine _motionCoroutine;
        private MonoBehaviour _runner;

        public GameObject GameObject => _transform.gameObject;
        public Transform Transform => _transform;
        public Rigidbody Rigidbody => _rb;

        public MeshFragment(Transform transform, Rigidbody rb, Renderer renderer)
        {
            rb.interpolation = RigidbodyInterpolation.None;
            _transform = transform;
            _rb = rb;
            _renderer = renderer;
        }

        public void Init(MonoBehaviour runner)
        {
            _runner = runner;
        }

        public void StopMotion()
        {
            if (_motionCoroutine != null)
                _runner.StopCoroutine(_motionCoroutine);
            _motionCoroutine = null;
        }

        public void SmoothStop(float duration)
        {
            StopMotion();
            _motionCoroutine = _runner.StartCoroutine(SmoothStopCoroutine(duration));
        }
    
        public void SetVisible(bool visible)
        {
            _renderer.enabled = visible;
        }
    
    
        public Vector3 GetCenter()
        {
            return _rb.worldCenterOfMass;
        }

        private IEnumerator SmoothStopCoroutine(float duration)
        {
            yield return new WaitForFixedUpdate();
            Vector3 initialVelocity = _rb.linearVelocity;
            Vector3 initialAngular = _rb.angularVelocity;

            float time = 0f;
            while (time < duration)
            {
                time += Time.fixedDeltaTime;
                float t = time / duration;
                float factor = Mathf.SmoothStep(1f, 0f, t);

                _rb.linearVelocity = initialVelocity * factor;
                _rb.angularVelocity = initialAngular * factor;

                yield return new WaitForFixedUpdate();
            }

            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        public void AnimateTo(Vector3 targetPos, Quaternion targetRot, float delay, float duration)
        {
            StopMotion();
            _motionCoroutine =
                _runner.StartCoroutine(MoveToCoroutine(targetPos, targetRot, delay, duration));
        }
    
        public void AnimateToTransform(Transform target, float delay, float duration)
        {
            StopMotion();
            _motionCoroutine =
                _runner.StartCoroutine(MoveToTransformCoroutine(target, delay, duration));
        }

        private IEnumerator MoveToCoroutine(Vector3 endPos, Quaternion endRot, float delay,
            float duration)
        {
            yield return new WaitForSeconds(delay);
        
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;

            Vector3 startPos = _transform.position;
            Quaternion startRot = _transform.rotation;

            float time = 0f;

            while (time < duration)
            {
                time += Time.fixedDeltaTime;
                float t = Mathf.SmoothStep(0, 1, time / duration);

                _rb.MovePosition(Vector3.Lerp(startPos, endPos, t));
                _rb.MoveRotation(Quaternion.Slerp(startRot, endRot, t));

                yield return new WaitForFixedUpdate();
            }

            _rb.MovePosition(endPos);
            _rb.MoveRotation(endRot);
        }
    
        private IEnumerator MoveToTransformCoroutine(Transform transform, float delay,
            float duration)
        {
            yield return new WaitForSeconds(delay);
        
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;

            Vector3 startPos = _transform.position;
            Quaternion startRot = _transform.rotation;

            float time = 0f;

            while (time < duration)
            {
                time += Time.fixedDeltaTime;
                float t = Mathf.SmoothStep(0, 1, time / duration);

                _rb.MovePosition(Vector3.Lerp(startPos, transform.position, t));
                _rb.MoveRotation(Quaternion.Slerp(startRot, transform.rotation, t));

                yield return new WaitForFixedUpdate();
            }

            _rb.MovePosition(transform.position);
            _rb.MoveRotation(transform.rotation);
        }
    }

    public class MeshFractureClone
    {
        private GameObject _fragmentRoot;
        private Transform _sourceTransform;
        private List<MeshFragment> _fragments = new();

        public GameObject Root => _fragmentRoot;
        public Transform Source => _sourceTransform;
    
        private bool _isVisible = true;

        public MeshFractureClone(GameObject fragmentRoot, Transform sourceTransform)
        {
            _fragmentRoot = fragmentRoot;
            _sourceTransform = sourceTransform;

            foreach (Transform child in _fragmentRoot.transform)
            {
                var rb = child.GetComponent<Rigidbody>();
                var renderer = child.GetComponent<Renderer>();
                if (rb != null)
                {
                    _fragments.Add(new MeshFragment(child, rb, renderer));
                }
            }
        }


        private static GameObject CreateCloneObject(string name, Transform target, Mesh mesh,
            Material[] materials, Transform parent, GameObject referenceObject = null)
        {
            var clone = new GameObject(name);
            clone.transform.SetPositionAndRotation(target.position, target.rotation);
            clone.transform.localScale = target.lossyScale;
            clone.transform.SetParent(parent, false);

            var meshFilter = clone.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRenderer = clone.AddComponent<MeshRenderer>();
            meshRenderer.materials = materials;

            var collider = clone.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            collider.convex = true;

            var rigidbody = clone.AddComponent<Rigidbody>();
            var refRb = referenceObject?.GetComponent<Rigidbody>();
            if (refRb != null)
            {
                rigidbody.mass = refRb.mass;
                rigidbody.linearDamping = refRb.linearDamping;
                rigidbody.angularDamping = refRb.angularDamping;
            }
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            rigidbody.detectCollisions = false;

            return clone;
        }

        public static MeshFractureClone FromRenderer(Transform source, Renderer rend,
            Transform parent = null, GameObject fragmentTemplatePrefab = null, GameObject referenceObject = null)
        {
            parent ??= source;

            Mesh mesh = null;
            Material[] materials = rend.sharedMaterials;

            if (rend is SkinnedMeshRenderer skinned)
            {
                mesh = new Mesh();
                skinned.BakeMesh(mesh);
            }
            else if (rend is MeshRenderer meshRenderer)
            {
                var mf = meshRenderer.GetComponent<MeshFilter>();
                if (mf == null) return null;
                mesh = Object.Instantiate(mf.sharedMesh);
            }

            if (mesh == null) return null;

            var cloneGo = CreateCloneObject("FractureClone", rend.transform, mesh, materials, parent, referenceObject);

            var fracture = cloneGo.AddComponent<CustomFracture>();
            fracture.fractureOptions = new FractureOptions
            {
                insideMaterial = materials[0],
                fragmentCount = 25,
            };
            fracture.fragmentTemplatePrefab = fragmentTemplatePrefab;

            GameObject fragmentRoot = fracture.CauseFracture();
            cloneGo.SetActive(false);
            Object.Destroy(cloneGo);

            if (!fragmentRoot) return null;

            return new MeshFractureClone(fragmentRoot, source);
        }

        public IEnumerable<MeshFragment> GetFragments()
        {
            return _fragments;
        }
    
        public void SetEnabled(bool enabled)
        {
            _fragmentRoot.SetActive(enabled);
        }

        public void SetVisible(bool visible)
        {
            if (_isVisible == visible) return;
            _isVisible = visible;
            foreach (var frag in GetFragments())
            {
                frag.SetVisible(visible);
                if (visible)
                {
                    frag.Rigidbody.isKinematic = false;
                    // frag.Rigidbody.detectCollisions = true;
                }
                else
                {
                    frag.Rigidbody.isKinematic = true;
                    // frag.Rigidbody.detectCollisions = false;
                }
            }
        }
    
        public Vector3 GetCenter()
        {
            Vector3 center = Vector3.zero;
            foreach (var frag in GetFragments())
            {
                center += frag.GetCenter();
            }

            return center / _fragments.Count;
        }

        public void SetGroupPosition(Vector3 position)
        {
            var diff = position - _sourceTransform.transform.position;
            foreach (var frag in GetFragments())
            {
                var rb = frag.Rigidbody;
                rb.MovePosition(frag.Rigidbody.position + diff);
            }
        }
    
        public void ResetFragmentsAtPositionAndOrientation(Vector3 position, Quaternion orientation)
        {
            foreach (var frag in GetFragments())
            {
                var rb = frag.Rigidbody;
                rb.MovePosition(position);
                rb.MoveRotation(orientation);
            }
        }
    
        public void ResetFragmentsAtSource()
        {
            foreach (var frag in GetFragments())
            {
                var rb = frag.Rigidbody;

                rb.MovePosition(Source.position);
                rb.MoveRotation(Source.rotation);
            }
        }
    
        public void SetGroupOrientation(Quaternion targetRotation)
        {
            var sourcePos = _sourceTransform.transform.position;
            var sourceRot = _sourceTransform.transform.rotation;

            Quaternion deltaRotation = targetRotation * Quaternion.Inverse(sourceRot);

            foreach (var frag in GetFragments())
            {
                var rb = frag.Rigidbody;
                var fragTransform = frag.Transform;

                Vector3 offset = fragTransform.position - sourcePos;

                Vector3 rotatedOffset = deltaRotation * offset;

                Vector3 newPosition = sourcePos + rotatedOffset;

                Quaternion newRotation = deltaRotation * fragTransform.rotation;

                rb.MovePosition(newPosition);
                rb.MoveRotation(newRotation);
            }
        }


        public void Destroy()
        {
            Object.Destroy(_fragmentRoot);
        }
    }

    public class FractureEffect : MonoBehaviour
    {
        private readonly List<MeshFractureClone> _clones = new();
        private readonly List<Renderer> _renderers = new();

        private Transform _cloneRoot;
    
        public GameObject fractureTemplatePrefab;

        private void Start()
        {
            _cloneRoot = new GameObject("FractureClones").transform;
            foreach (var rend in GetComponentsInChildren<Renderer>())
            {
                if (rend.CompareTag("MeshVFX"))
                {
                    continue;
                }
                MeshFractureClone clone = null;
                if (rend is SkinnedMeshRenderer or MeshRenderer)
                {
                    clone = MeshFractureClone.FromRenderer(rend.transform, rend, _cloneRoot,
                        fractureTemplatePrefab);
                }

                if (clone == null) continue;
                _clones.Add(clone);
                _renderers.Add(rend);
                clone.SetVisible(false);
            }

            foreach (var clone in _clones)
            {
                foreach (var frag in clone.GetFragments())
                {
                    frag.Init(this);
                }
            }
        }
    
        public IEnumerator BreakAndPauseFragmentsSmooth(float explosionForce,
            float slowdownTime = 1.5f)
        {
            foreach (var clone in _clones)
            {
                var center = clone.GetCenter();
                foreach (var frag in clone.GetFragments())
                {
                    var rb = frag.Rigidbody;

                    rb.isKinematic = false;
                    rb.useGravity = false;
                    rb.detectCollisions = true;

                    Vector3 dir = (frag.GetCenter() - center).normalized +
                                  Random.insideUnitSphere * 0.2f;
                    rb.AddForce(dir * explosionForce, ForceMode.Impulse);
                    frag.SmoothStop(slowdownTime);
                }
            }

            yield return null; //WaitForSeconds(slowdownTime);
        }
    
        public void Break(float explosionForce = 1f)
        {
            foreach (var clone in _clones)
            {
                foreach (var frag in clone.GetFragments())
                {
                    var rb = frag.Rigidbody;

                    rb.isKinematic = false;
                    rb.useGravity = true;
                    rb.detectCollisions = true;

                    Vector3 dir = (frag.GetCenter() - clone.GetCenter()).normalized +
                                  Random.insideUnitSphere * 0.2f;
                    rb.AddForce(dir * explosionForce, ForceMode.Impulse);
                }
            }
        }
    
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // draw all fragment center
            foreach (var meshFractureClone in _clones)
            {
                foreach (var meshFragment in meshFractureClone.GetFragments())
                {
                    var color = Color.red;
                    color.a = 0.7f;
                    Gizmos.color = color;
                    Gizmos.DrawSphere(meshFragment.GetCenter(), 0.1f);
                }
            }
        }
#endif

        public void SetDissolveSpeedFactor(float factor)
        {
            foreach (var clone in _clones)
            {
                foreach (var frag in clone.GetFragments())
                {
                    var dissolver = frag.Transform.GetComponent<IVisibilityTransitionEffect>();
                    dissolver?.SetSpeedFactor(factor);
                }
            }
        }
        
        public void DissolveFragments()
        {
            foreach (var clone in _clones)
            {
                foreach (var frag in clone.GetFragments())
                {
                    var dissolver = frag.Transform.GetComponent<IVisibilityTransitionEffect>();
                    dissolver?.Hide();
                }
            }
        }

        public void AppearFragments()
        {
            foreach (var clone in _clones)
            {
                foreach (var frag in clone.GetFragments())
                {
                    var dissolver = frag.Transform.GetComponent<IVisibilityTransitionEffect>();
                    dissolver?.Show();
                }
            }
        }
    
        public void SetGravityEnabled(bool enabled)
        {
            foreach (var clone in _clones)
            {
                foreach (var frag in clone.GetFragments())
                {
                    var rb = frag.Rigidbody;
                    rb.useGravity = enabled;
                }
            }
        }
    
        public bool AreAllFragmentsFinishedDissolving()
        {
            foreach (var clone in _clones)
            {
                foreach (var frag in clone.GetFragments())
                {
                    var dissolver = frag.Transform.GetComponent<IVisibilityTransitionEffect>();
                    if (dissolver is { IsTransitioning: true })
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void SwapVisibility(bool showOriginal)
        {
            if (showOriginal)
            {
                foreach (var clone in _clones)
                {
                    clone.SetVisible(false);
                }
                SetOriginalVisible(true);
            }
            else
            {
                SetOriginalVisible(false);
                foreach (var clone in _clones)
                {
                    clone.SetVisible(true);
                }
            }
        }
    
        public void SetFragmentsEnabled(bool enabled)
        {
            foreach (var clone in _clones)
            {
                clone.SetEnabled(enabled);
            }
        }
    
        public void ResetFragmentsAtPositionAndOrientation(Vector3 position,
            Quaternion orientation)
        {
            foreach (var clone in _clones)
            {
                clone.ResetFragmentsAtPositionAndOrientation(position, orientation);
            }
        }
    
        public void ResetFragmentsAtSource()
        {
            foreach (var clone in _clones)
            {
                clone.ResetFragmentsAtSource();
            }
        }
    
        public void SetFragmentGroupPositionAndOrientation(Vector3 position,
            Quaternion orientation)
        {
            foreach (var clone in _clones)
            {
                clone.SetGroupOrientation(orientation);
                clone.SetGroupPosition(position);
            }
        }
    
        public IEnumerator ReassembleSmooth(float durationPerPiece = 0.6f,
            float delayJitter = 0.2f, float animationSpeedFactor = 1f)
        {
            durationPerPiece /= animationSpeedFactor;

            float currentTotalDelay = 0f;
            foreach (var clone in _clones.OrderRandomly())
            {
                foreach (var frag in clone.GetFragments())
                {
                    Rigidbody rb = frag.Rigidbody;
                    rb.detectCollisions = false;
                    rb.useGravity = false;

                    currentTotalDelay += Random.Range(0f, delayJitter) / animationSpeedFactor;

                    frag.AnimateToTransform(clone.Source, currentTotalDelay, durationPerPiece);
                }
            }
            yield return new WaitForSeconds(currentTotalDelay + durationPerPiece);
        }

        private void SetOriginalVisible(bool visible)
        {
            foreach (var rend in _renderers)
            {
                rend.enabled = visible;
            }
        }

        private void OnDestroy()
        {
            foreach (var clone in _clones)
            {
                clone.Destroy();
            }

            if (_cloneRoot != null)
            {
                Destroy(_cloneRoot.gameObject);
            }
        }
    }
}