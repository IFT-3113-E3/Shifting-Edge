using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeshVFX
{
    public class MeshTrailEffect : MonoBehaviour
    {
        public float timeToLive = 0.5f;
        public float baseAlpha = 1f;
        public float interval = 0.1f;
        public Material overrideMaterial;
        public string baseColorPropertyName = "_Color";
        public bool manualVelocity = false;
        public float velocityMagnitudeThreshold = 10f;
        
        private Vector3 _velocity;
        private bool _isTrailActive;
        private float _timeSinceLastSpawn;
        
        private MaterialPropertyBlock _mpb;
        
        private Vector3 _lastPosition;

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
        }

        private struct MeshTrailCloneGroup
        {
            public MeshRendererCloneBase[] Clones;
            public float SpawnTime;
            public float ExpireTime;
            
            public float NormalizedLifetime => Mathf.Clamp01((Time.time - SpawnTime) / (ExpireTime - SpawnTime));
            
            public void SetVisible(bool visible)
            {
                foreach (var clone in Clones)
                    clone.SetActive(visible);
            }
        }
        
        private List<Renderer> _meshRenderers = new();

        private Queue<MeshTrailCloneGroup> _meshTrailComponents = new();
        private readonly Queue<MeshRendererCloneBase[]> _clonePool = new();

        private void Start()
        {
            foreach (var rend in GetComponentsInChildren<Renderer>())
            {
                if (rend is MeshRenderer or SkinnedMeshRenderer)
                {
                    _meshRenderers.Add(rend);
                }
            }
        }

        private void Update()
        {
            if (!manualVelocity)
            {
                _velocity = (transform.position - _lastPosition) / Time.deltaTime;
            }
            
            if (_velocity.magnitude > velocityMagnitudeThreshold)
            {
                if (!_isTrailActive)
                {
                    _isTrailActive = true;
                }
            }
            else
            {
                if (_isTrailActive)
                {
                    _isTrailActive = false;
                }
            }

            _lastPosition = transform.position;
        }
        
        public void UpdateVelocity(Vector3 velocity)
        {
            _velocity = velocity;
        }

        private void LateUpdate()
        {

            _timeSinceLastSpawn += Time.deltaTime;
            if (_isTrailActive)
            {
                if (_timeSinceLastSpawn >= interval)
                {
                    _timeSinceLastSpawn = 0f;
                    SpawnTrailSnapshot();
                }
            }

            UpdateTrailLifetimes();
        }
        
        private void SpawnTrailSnapshot()
        {
            var clones = GetCloneFromPool();
            for (var i = 0; i < clones.Length; i++)
            {
                var clone = clones[i];
                clone.SetTransform(_meshRenderers[i].transform);
                clone.SetLayer(gameObject.layer);
                clone.UpdateMesh();
                
                clone.GetPropertyBlock(_mpb);
                var color = overrideMaterial.GetColor(baseColorPropertyName);
                color.a = baseAlpha;
                _mpb.SetColor(baseColorPropertyName, color);
                clone.SetPropertyBlock(_mpb);
                
                clone.SetActive(true);
            }

            _meshTrailComponents.Enqueue(new MeshTrailCloneGroup
            {
                Clones = clones,
                SpawnTime = Time.time,
                ExpireTime = Time.time + timeToLive
            });
        }
        
        private void UpdateTrailLifetimes()
        {
            int count = _meshTrailComponents.Count;
            for (int n = 0; n < count; n++)
            {
                var group = _meshTrailComponents.Peek();

                float fade = 1f - group.NormalizedLifetime;
                var color = overrideMaterial.GetColor(baseColorPropertyName);
                color.a = Mathf.Lerp(0f, baseAlpha, fade);

                for (int i = 0; i < group.Clones.Length; i++)
                {
                    var clone = group.Clones[i];
                    clone.GetPropertyBlock(_mpb);
                    _mpb.SetColor(baseColorPropertyName, color);
                    clone.SetPropertyBlock(_mpb);
                }

                if (Time.time >= group.ExpireTime)
                {
                    _meshTrailComponents.Dequeue();
                    ReturnCloneToPool(group.Clones);
                }
                else
                {
                    _meshTrailComponents.Enqueue(_meshTrailComponents.Dequeue());
                }
            }
        }

        
        private MeshRendererCloneBase[] GetCloneFromPool()
        {
            if (_clonePool.Count > 0)
            {
                return _clonePool.Dequeue();
            }

            MeshRendererCloneBase[] clones = new MeshRendererCloneBase[_meshRenderers.Count];
            for (var i = 0; i < _meshRenderers.Count; i++)
            {
                var rend = _meshRenderers[i];
                var clone = MeshRendererCloneBase.Create(rend);
                var cloneMat = new Material(overrideMaterial);
                clone.Material = cloneMat;
                clone.SetActive(false);
                clones[i] = clone;
            }

            return clones;
        }
        
        private void ReturnCloneToPool(MeshRendererCloneBase[] clones)
        {
            for (var i = 0; i < clones.Length; i++)
            {
                var clone = clones[i];
                clone.SetActive(false);
            }

            _clonePool.Enqueue(clones);
        }

        private void OnDestroy()
        {
            foreach (var group in _meshTrailComponents)
            {
                for (int i = 0; i < group.Clones.Length; i++)
                {
                    var clone = group.Clones[i];
                    clone.Destroy();
                }
            }

            foreach (var clones in _clonePool)
            {
                for (int i = 0; i < clones.Length; i++)
                {
                    var clone = clones[i];
                    clone.Destroy();
                }
            }
        }
    }
}