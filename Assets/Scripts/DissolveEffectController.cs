using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public abstract class DissolveCloneBase
{
    public GameObject GameObject { get; protected set; }
    public Renderer Renderer { get; protected set; }
    protected MeshFilter MeshFilter;

    public virtual void UpdateMesh()
    {
    }

    public Bounds GetWorldBounds()
    {
        var mesh = MeshFilter?.sharedMesh;
        if (!mesh || mesh.vertexCount == 0)
            return new Bounds(GameObject.transform.position, Vector3.zero);

        var vertices = mesh.vertices;
        var transform = GameObject.transform;

        var min = transform.TransformPoint(vertices[0]);
        var max = min;

        for (int i = 1; i < vertices.Length; i++)
        {
            var v = transform.TransformPoint(vertices[i]);
            min = Vector3.Min(min, v);
            max = Vector3.Max(max, v);
        }

        return new Bounds((min + max) / 2f, max - min);
    }

    public Bounds GetLocalBounds()
    {
        var mesh = MeshFilter?.sharedMesh;
        return mesh ? mesh.bounds : new Bounds();
    }

    public void Destroy()
    {
        if (GameObject)
            Object.Destroy(GameObject);
    }
}

public class MeshDissolveClone : DissolveCloneBase
{
    public MeshDissolveClone(MeshRenderer source)
    {
        var sourceMeshFilter = source.GetComponent<MeshFilter>();
        if (!sourceMeshFilter)
        {
            Debug.LogError("Source MeshRenderer does not have a MeshFilter component.");
            return;
        }

        var sourceMesh1 = sourceMeshFilter.sharedMesh;
        // if (sourceMesh1.subMeshCount > 1)
        // {
        //     sourceMesh1.SetTriangles(sourceMesh1.GetTriangles(0), 0);
        //     sourceMesh1.RecalculateBounds();
        // }

        GameObject = new GameObject("MeshDissolveClone");
        GameObject.transform.SetPositionAndRotation(source.transform.position,
            source.transform.rotation);
        GameObject.transform.localScale = source.transform.lossyScale;

        MeshFilter = GameObject.AddComponent<MeshFilter>();
        MeshFilter.sharedMesh = sourceMesh1;

        Renderer = GameObject.AddComponent<MeshRenderer>();
    }
}


public class SkinnedMeshDissolveClone : DissolveCloneBase
{
    private readonly SkinnedMeshRenderer _source;
    private readonly Mesh _bakedMesh;

    public SkinnedMeshDissolveClone(SkinnedMeshRenderer sourceRenderer)
    {
        _source = sourceRenderer;
        _bakedMesh = new Mesh();

        GameObject = new GameObject("SkinnedDissolveClone");
        GameObject.transform.SetPositionAndRotation(_source.transform.position,
            _source.transform.rotation);
        GameObject.transform.localScale = _source.transform.lossyScale;

        MeshFilter = GameObject.AddComponent<MeshFilter>();
        Renderer = GameObject.AddComponent<MeshRenderer>();

        _source.BakeMesh(_bakedMesh);
        MeshFilter.sharedMesh = _bakedMesh;
    }

    public override void UpdateMesh()
    {
        if (_source)
        {
            _source.BakeMesh(_bakedMesh);
            MeshFilter.sharedMesh = _bakedMesh;
        }
    }
}

public class DissolveEffectController : MonoBehaviour, IVisibilityTransitionEffect
{
    private static readonly int DissolveHeight = Shader.PropertyToID("_DissolveHeight");
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int TransitionPhase = Shader.PropertyToID("_TransitionPhase");
    private static readonly int EdgeWidth = Shader.PropertyToID("_EdgeWidth");
    private static readonly int EdgeColor = Shader.PropertyToID("_EdgeColor");
    private static readonly int Alpha = Shader.PropertyToID("_Alpha");
    private static readonly int BoundsMaxY = Shader.PropertyToID("_BoundsMaxY");
    private static readonly int BoundsMinY = Shader.PropertyToID("_BoundsMinY");

    [Header("Effect Settings")] public Material dissolveMaterialTemplate;
    public Color materializeColor = Color.white;
    public float edgeWidth = 0.1f;
    public Color edgeColor = Color.white;
    public float dissolveDuration = 1.0f;
    public float fadeOutDuration = 0.5f;

    private Coroutine _dissolveCoroutine;

    public bool IsDissolved => _isDissolved;
    public bool IsDissolving => _isDissolving;

    // buttons to trigger the effect
    [ContextMenu("Materialize")]
    public void Materialize()
    {
        if (_isDissolving) return;
        PlayEffect(EffectMode.Materialize);
    }

    [ContextMenu("Dematerialize")]
    public void Dematerialize()
    {
        if (_isDissolving) return;
        PlayEffect(EffectMode.Dematerialize);
    }

    [ContextMenu("Fade in")]
    public void FadeIn()
    {
        if (_isDissolving) return;
        PlayFadeEffect(EffectMode.Materialize);
    }

    [ContextMenu("Fade out")]
    public void FadeOut()
    {
        if (_isDissolving) return;
        PlayFadeEffect(EffectMode.Dematerialize);
    }


    [Header("Optional VFX")] public VisualEffect dissolveVFXPrefab; // Optional VFX prefab
    public VisualEffect dissolveVFXPrefab2; // Optional VFX prefab
    private VisualEffect _dissolveVFX; // Optional VFX for additional effects

    [Header("Mesh Renderers")]
    public List<Renderer> renderersToCopy; // Leave empty to auto-collect from children

    private readonly List<DissolveCloneBase> _dissolveCopies = new();
    private Material _dissolveMaterial;

    private bool _isDissolved;
    private bool _isDissolving;

    private float _currentHeight;
    private Vector3 _worldMin;
    private Vector3 _worldMax;

    public enum EffectMode
    {
        Materialize,
        Dematerialize
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Materialize();
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            Dematerialize();
        }
    }
    
    public void PlayEffect(EffectMode mode)
    {
        if (_dissolveCoroutine != null)
        {
            StopCoroutine(_dissolveCoroutine);
            _dissolveCoroutine = null;
            Cleanup();
        }

        _dissolveCoroutine = StartCoroutine(DoHeightDissolveEffect(mode));
    }

    public void PlayFadeEffect(EffectMode mode)
    {
        if (_dissolveCoroutine != null)
        {
            StopCoroutine(_dissolveCoroutine);
            _dissolveCoroutine = null;
        }

        _dissolveCoroutine = StartCoroutine(DoFadeDissolveEffect(mode));
    }

    public Bounds GetTransformedBounds(Bounds bounds)
    {
        var corners = new Vector3[8];
        corners[0] = bounds.min;
        corners[1] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
        corners[2] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
        corners[3] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
        corners[4] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
        corners[5] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
        corners[6] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
        corners[7] = bounds.max;

        var center = transform.TransformPoint(bounds.center);

        // Transform the corners to world space
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = transform.TransformPoint(corners[i]);
        }

        // Find the min and max points
        var minX = corners[0];
        var maxX = corners[0];
        var minY = corners[0];
        var maxY = corners[0];
        var minZ = corners[0];
        var maxZ = corners[0];

        for (int i = 1; i < corners.Length; i++)
        {
            minX = minX.x < corners[i].x ? minX : corners[i];
            maxX = maxX.x > corners[i].x ? maxX : corners[i];
            minY = minY.y < corners[i].y ? minY : corners[i];
            maxY = maxY.y > corners[i].y ? maxY : corners[i];
            minZ = minZ.z < corners[i].z ? minZ : corners[i];
            maxZ = maxZ.z > corners[i].z ? maxZ : corners[i];
        }
        
        var min = new Vector3(minX.x, minY.y, minZ.z);
        var max = new Vector3(maxX.x, maxY.y, maxZ.z);
        
        var transformedBounds = new Bounds();
        transformedBounds.SetMinMax(min, max);
        transformedBounds.center = center;
        transformedBounds.size = max - min;
        return transformedBounds;
    }


    private DissolveCloneBase CreateDissolveClone(Renderer sourceRenderer)
    {
        if (sourceRenderer is SkinnedMeshRenderer skinnedMeshRenderer)
        {
            return new SkinnedMeshDissolveClone(skinnedMeshRenderer);
        }

        if (sourceRenderer is MeshRenderer meshRenderer)
        {
            return new MeshDissolveClone(meshRenderer);
        }

        Debug.LogWarning($"Unsupported renderer type: {sourceRenderer.GetType()}");
        return null;
    }

    private Material CreateDissolveMaterial(float transitionPhase)
    {
        Material mat;

        if (dissolveMaterialTemplate)
        {
            mat = new Material(dissolveMaterialTemplate);
        }
        else
        {
            mat = new Material(Shader.Find("Unlit/SimpleDissolve"));
        }

        mat.SetColor(BaseColor, materializeColor);
        mat.SetColor(EdgeColor, edgeColor);
        mat.SetFloat(EdgeWidth, edgeWidth);
        mat.SetFloat(TransitionPhase, transitionPhase);
        return mat;
    }

    private IEnumerator AnimateDissolveHeight(float startY, float endY, float duration)
    {
        if (_dissolveVFX)
        {
            _dissolveVFX.Play();
        }

        float t = 0;
        float startAlpha = startY > endY ? 1f : 0f;
        float endAlpha = startY > endY ? 0f : 1f;
        while (t < duration)
        {
            float progress = t / duration;
            progress = 1f - Mathf.Cos(progress * Mathf.PI / 2f);
            float currentHeight = Mathf.Lerp(startY, endY, progress);
            float alpha = Mathf.Lerp(startAlpha, endAlpha, progress);

            // start and stop the particles a little after start and before end
            float progressPercentage = Mathf.InverseLerp(startY, endY, currentHeight);
            if (progressPercentage < 0.1f && _dissolveVFX)
            {
                _dissolveVFX.Play();
            }
            else if (progressPercentage >= 0.9f && _dissolveVFX)
            {
                _dissolveVFX.Stop();
            }


            // foreach (var rend in _dissolveCopies)
            // {
            _dissolveMaterial.SetFloat(DissolveHeight, currentHeight);
            _dissolveMaterial.SetFloat(Alpha, alpha);
            _currentHeight = currentHeight;
            // }


            t += Time.deltaTime;
            yield return null;
        }

        // foreach (var rend in _dissolveCopies)
        // {
        _dissolveMaterial.SetFloat(DissolveHeight, endY);
        _dissolveMaterial.SetFloat(Alpha, endAlpha);

        _currentHeight = endY;
        // }

        // if (_dissolveVFX)
        // {
        //     _dissolveVFX.SetFloat("height", endY);
        //     _dissolveVFX.SetFloat("dissolveHeight", endY);
        // }
    }


    private IEnumerator FadeDissolveCopies(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            float progress = t / duration;
            float alpha = Mathf.SmoothStep(from, to, progress);

            _dissolveMaterial.SetFloat(Alpha, alpha);

            foreach (var rend in _dissolveCopies)
            {
                rend.UpdateMesh();
            }

            t += Time.deltaTime;
            yield return null;
        }
    }

    private void Cleanup()
    {
        if (_dissolveVFX)
        {
            Destroy(_dissolveVFX.gameObject);
            _dissolveVFX = null;
        }

        foreach (var go in _dissolveCopies)
            go.Destroy();

        _dissolveCopies.Clear();
    }

    private IEnumerator DoHeightDissolveEffect(EffectMode mode)
    {
        _isDissolving = true;
        _isDissolved = mode == EffectMode.Materialize;

        if (renderersToCopy == null || renderersToCopy.Count == 0)
        {
            renderersToCopy = new();
            renderersToCopy.AddRange(GetComponentsInChildren<MeshRenderer>());
            renderersToCopy.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>());
        }

        Bounds combinedBounds = new Bounds(transform.position, Vector3.zero);

        _dissolveMaterial = CreateDissolveMaterial(1f);

        // Create dissolve clones
        _dissolveCopies.Clear();
        foreach (var rend in renderersToCopy)
        {
            var cloneRenderer = CreateDissolveClone(rend);
            if (cloneRenderer != null)
            {
                cloneRenderer.GameObject.transform.SetParent(null);
                cloneRenderer.GameObject.transform.position = rend.transform.position;
                cloneRenderer.GameObject.transform.rotation = rend.transform.rotation;
                cloneRenderer.GameObject.transform.localScale = rend.transform.lossyScale;
                cloneRenderer.UpdateMesh();
                // Set the dissolve material
                // cloneRenderer.Renderer.material = mat;
                cloneRenderer.Renderer.sharedMaterials = new Material[2]
                {
                    _dissolveMaterial,
                    _dissolveMaterial
                };
                combinedBounds = cloneRenderer.GetLocalBounds();
                _dissolveCopies.Add(cloneRenderer);
            }
        }

        Bounds transformedBounds = GetTransformedBounds(combinedBounds);

        Vector3 center = transformedBounds.center;
        _worldMin = transformedBounds.min;
        _worldMax = transformedBounds.max;

        float height = _worldMax.y - _worldMin.y;
        edgeWidth = height * 0.2f;

        float minY = _worldMin.y - edgeWidth / 2f;
        float maxY = _worldMax.y + edgeWidth / 2f;


        float startHeight = (mode == EffectMode.Materialize) ? 0f : 1f;
        float endHeight = (mode == EffectMode.Materialize) ? 1f : 0f;
        float startAlpha = (mode == EffectMode.Materialize) ? 1f : 0f;
        float endAlpha = (mode == EffectMode.Materialize) ? 0f : 1f;
        // the circle radius area that the plane will cover with the transformed bounds
        float radius =
            Mathf.Max(transformedBounds.extents.x, transformedBounds.extents.z);


        // Create VFX if assigned
        if (mode == EffectMode.Materialize)
        {
            if (dissolveVFXPrefab)
            {
                _dissolveVFX =
                    Instantiate(dissolveVFXPrefab, transform.position, Quaternion.identity);
            }
        }
        else
        {
            if (dissolveVFXPrefab2)
            {
                _dissolveVFX =
                    Instantiate(dissolveVFXPrefab2, transform.position, Quaternion.identity);
            }
        }

        if (_dissolveVFX)
        {
            _dissolveVFX.transform.SetParent(transform);
            // position at the center of the plane
            _dissolveVFX.transform.position =
                new Vector3(center.x, minY, center.z);
            _dissolveVFX.SetFloat("rad", radius);
        }

        _currentHeight = startHeight;
        // Prepare material for animation
        // foreach (var rend in _dissolveCopies)
        // {
        var mat = _dissolveMaterial;
        mat.SetFloat(BoundsMinY, minY);
        mat.SetFloat(BoundsMaxY, maxY);
        mat.SetFloat(DissolveHeight, startHeight);
        mat.SetFloat(Alpha, startAlpha);
        mat.SetFloat(EdgeWidth, edgeWidth);
        mat.SetColor(EdgeColor, edgeColor);
        // }

        if (mode == EffectMode.Dematerialize)
        {
            SetOriginalVisible(true);
            yield return FadeDissolveCopies(from: startAlpha, to: endAlpha,
                duration: fadeOutDuration);
            SetOriginalVisible(false);
            yield return AnimateDissolveHeight(startHeight, endHeight, dissolveDuration);
            yield return new WaitUntil(() =>
                (_dissolveVFX && _dissolveVFX.aliveParticleCount == 0) || !_dissolveVFX);
        }
        else
        {
            SetOriginalVisible(false);
            yield return AnimateDissolveHeight(startHeight, endHeight, dissolveDuration);
            SetOriginalVisible(true);
            yield return FadeDissolveCopies(from: startAlpha, to: endAlpha,
                duration: fadeOutDuration);
        }


        Cleanup();

        // // Dematerialize → stay hidden
        // if (mode == EffectMode.Dematerialize)
        // {
        //     SetOriginalVisible(false);
        // }

        _isDissolved = (mode == EffectMode.Dematerialize);
        _isDissolving = false;
    }

    // A dissolve effect that fades to the transition color and then dissolves by fading back out
    private IEnumerator DoFadeDissolveEffect(EffectMode mode)
    {
        _isDissolving = true;
        _isDissolved = mode == EffectMode.Materialize;

        if (renderersToCopy.Count == 0)
        {
            renderersToCopy = new();
            renderersToCopy.AddRange(GetComponentsInChildren<MeshRenderer>());
            renderersToCopy.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>());
        }

        Bounds combinedBounds = new Bounds(transform.position, Vector3.zero);

        _dissolveMaterial = CreateDissolveMaterial(1f);

        // Create dissolve clones
        _dissolveCopies.Clear();
        foreach (var rend in renderersToCopy)
        {
            var cloneRenderer = CreateDissolveClone(rend);
            if (cloneRenderer != null)
            {
                cloneRenderer.GameObject.transform.SetParent(null);
                cloneRenderer.GameObject.transform.position = rend.transform.position;
                cloneRenderer.GameObject.transform.rotation = rend.transform.rotation;
                cloneRenderer.GameObject.transform.localScale = rend.transform.lossyScale;
                cloneRenderer.UpdateMesh();
                // Set the dissolve material
                // cloneRenderer.Renderer.material = mat;
                cloneRenderer.Renderer.sharedMaterial = _dissolveMaterial;
                for (int matIndex = 0;
                     matIndex < cloneRenderer.Renderer.sharedMaterials.Length;
                     matIndex++)
                {
                    cloneRenderer.Renderer.sharedMaterials[matIndex] = _dissolveMaterial;
                }

                combinedBounds = cloneRenderer.GetWorldBounds();
                _dissolveCopies.Add(cloneRenderer);
            }
        }

        // float minY = combinedBounds.min.y - edgeWidth/2f;
        float maxY = combinedBounds.max.y;

        // float startHeight = (mode == EffectMode.Materialize) ? minY : maxY;
        // float endHeight = (mode == EffectMode.Materialize) ? maxY : minY;
        float startAlpha = (mode == EffectMode.Materialize) ? 1f : 0f;
        float endAlpha = (mode == EffectMode.Materialize) ? 0f : 1f;
        // the circle radius area that the plane will cover
        // float radius =
        // Mathf.Min(combinedBounds.extents.x, combinedBounds.extents.z);


        // Create VFX if assigned
        // if (mode == EffectMode.Materialize)
        // {
        //     if (dissolveVFXPrefab)
        //     {
        //         _dissolveVFX =
        //             Instantiate(dissolveVFXPrefab, transform.position, Quaternion.identity);
        //     }
        // }
        // else
        // {
        //     if (dissolveVFXPrefab2)
        //     {
        //         _dissolveVFX =
        //             Instantiate(dissolveVFXPrefab2, transform.position, Quaternion.identity);
        //     }
        // }

        // if (_dissolveVFX)
        // {
        //     _dissolveVFX.transform.SetParent(transform);
        //     // position at the center of the plane
        //     _dissolveVFX.transform.position =
        //         new Vector3(combinedBounds.center.x, minY, combinedBounds.center.z);
        //     _dissolveVFX.SetFloat("rad", radius);
        // }

        // Prepare material for animation
        // foreach (var rend in _dissolveCopies)
        // {
        var mat = _dissolveMaterial;
        mat.SetFloat(DissolveHeight, maxY);
        mat.SetFloat(Alpha, startAlpha);
        mat.SetFloat(EdgeWidth, edgeWidth);
        mat.SetColor(EdgeColor, edgeColor);
        // }

        if (mode == EffectMode.Dematerialize)
        {
            SetOriginalVisible(true);
            yield return FadeDissolveCopies(from: startAlpha, to: endAlpha,
                duration: fadeOutDuration);
            SetOriginalVisible(false);
            yield return FadeDissolveCopies(from: endAlpha, to: startAlpha,
                duration: dissolveDuration);
        }
        else
        {
            SetOriginalVisible(false);
            yield return FadeDissolveCopies(from: endAlpha, to: startAlpha,
                duration: dissolveDuration);
            SetOriginalVisible(true);
            yield return FadeDissolveCopies(from: startAlpha, to: endAlpha,
                duration: fadeOutDuration);
        }

        //
        // // Destroy VFX
        // if (_dissolveVFX)
        // {
        //     Destroy(_dissolveVFX.gameObject);
        //     _dissolveVFX = null;
        // }

        // Destroy dissolve clones
        foreach (var go in _dissolveCopies)
            go.Destroy();

        _dissolveCopies.Clear();

        _isDissolved = (mode == EffectMode.Dematerialize);
        _isDissolving = false;
    }

    private void LateUpdate()
    {
        if (_isDissolving)
        {
            for (int copyIndex = 0; copyIndex < _dissolveCopies.Count; copyIndex++)
            {
                var clone = _dissolveCopies[copyIndex];
                var source = renderersToCopy[copyIndex];
                if (clone != null)
                {
                    clone.GameObject.transform.position = source.transform.position;
                    clone.GameObject.transform.rotation = source.transform.rotation;
                    clone.GameObject.transform.localScale = source.transform.lossyScale;

                    var localBounds = clone.GetLocalBounds();
                    var transformedBounds = GetTransformedBounds(localBounds);
                    _worldMin = transformedBounds.min;
                    _worldMax = transformedBounds.max;
                }
            }

            if (_dissolveVFX)
            {
                // Get the current height from the material

                float particleStopHeight = Mathf.Lerp(_worldMin.y, _worldMax.y, _currentHeight);
                float particleSpawnHeight = particleStopHeight + 1f;

                _dissolveVFX.SetFloat("height", particleSpawnHeight);
                _dissolveVFX.SetFloat("dissolveHeight", particleStopHeight);
            }

            _dissolveMaterial.SetFloat(BoundsMinY, _worldMin.y - edgeWidth / 2f);
            _dissolveMaterial.SetFloat(BoundsMaxY, _worldMax.y + edgeWidth / 2f);
        }
    }

    private void SetOriginalVisible(bool visible)
    {
        foreach (var rend in renderersToCopy)
        {
            rend.enabled = visible;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        LateUpdate();
        if (_dissolveCopies.Count == 0) return;

        var sampleRenderer = _dissolveCopies[0].Renderer;
        if (sampleRenderer == null) return;

        var mat = sampleRenderer.sharedMaterial;
        if (mat == null || !mat.HasProperty(DissolveHeight)) return;

        float y = mat.GetFloat(DissolveHeight);
        float minY = mat.GetFloat(BoundsMinY);
        float maxY = mat.GetFloat(BoundsMaxY);
        y = Mathf.Lerp(minY, maxY, y);

        // Draw a horizontal line across the object's bounds
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (var clone in _dissolveCopies)
        {
            bounds = clone.GetLocalBounds();
        }

        // Transform the bounds to world space
        Bounds transformedBounds = GetTransformedBounds(bounds);

        // Draw the bounds
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transformedBounds.center, transformedBounds.size);

        // DRAW MIN MAX SPHERES
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transformedBounds.min, 0.1f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transformedBounds.max, 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transformedBounds.center, 0.1f);

        // draw plane
        Vector3 left = new Vector3(bounds.min.x, y, bounds.center.z);
        Vector3 right = new Vector3(bounds.max.x, y, bounds.center.z);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(left, right);
        
        Gizmos.color = new Color(0, 1, 1, 0.1f);
        Vector3 center = new Vector3(bounds.center.x, y, bounds.center.z);
        Vector3 size = new Vector3(bounds.size.x, 0.01f, bounds.size.z);
        Gizmos.DrawCube(center, size);
    }
#endif
    
    public bool IsTransitioning => _isDissolving;
    public bool IsVisible => _isDissolved;
    public void Show()
    {
        if (_isDissolving) return;
        PlayEffect(EffectMode.Materialize);
    }

    public void Hide()
    {
        if (_isDissolving) return;
        PlayEffect(EffectMode.Dematerialize);
    }
    
    public void Cancel()
    {
        if (!_isDissolving) return;
        if (_dissolveCoroutine != null)
        {
            StopCoroutine(_dissolveCoroutine);
            _dissolveCoroutine = null;
        }

        Cleanup();
    }
    
}