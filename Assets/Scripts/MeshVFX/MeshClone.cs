using UnityEngine;

namespace MeshVFX
{
    public abstract class MeshRendererCloneBase
    {
        protected GameObject GameObject;
        protected Renderer Renderer;
        protected MeshFilter MeshFilter;

        public static MeshRendererCloneBase Create(Renderer sourceRenderer)
        {
            if (sourceRenderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                return new SkinnedMeshRendererClone(skinnedMeshRenderer);
            }

            if (sourceRenderer is MeshRenderer meshRenderer)
            {
                return new MeshFilterRendererClone(meshRenderer);
            }

            Debug.LogWarning($"Unsupported renderer type: {sourceRenderer.GetType()}");
            return null;
        }
        
        public Material Material
        {
            get => Renderer.material;
            set => Renderer.material = value;
        }
        
        public Material SharedMaterial
        {
            get => Renderer.sharedMaterial;
            set => Renderer.sharedMaterial = value;
        }
        
        public Material[] SharedMaterials
        {
            get => Renderer.sharedMaterials;
            set => Renderer.sharedMaterials = value;
        }
        
        public void GetPropertyBlock(MaterialPropertyBlock block)
        {
            if (Renderer)
                Renderer.GetPropertyBlock(block);
        }
        
        public void SetPropertyBlock(MaterialPropertyBlock block)
        {
            if (Renderer)
                Renderer.SetPropertyBlock(block);
        }
        
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
        
        public bool IsActive()
        {
            return GameObject && GameObject.activeSelf;
        }
        
        public void SetActive(bool isActive)
        {
            if (GameObject)
                GameObject.SetActive(isActive);
        }
        
        public void SetLayer(int layer)
        {
            if (GameObject)
                GameObject.layer = layer;
        }
        
        public void SetParent(Transform parent)
        {
            if (GameObject)
                GameObject.transform.SetParent(parent);
        }
        
        public void SetTransform(Transform transform)
        {
            if (GameObject)
            {
                GameObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
                GameObject.transform.localScale = transform.lossyScale;
            }
        }
    }

    public class MeshFilterRendererClone : MeshRendererCloneBase
    {
        public MeshFilterRendererClone(MeshRenderer source)
        {
            var sourceMeshFilter = source.GetComponent<MeshFilter>();
            if (!sourceMeshFilter)
            {
                Debug.LogError("Source MeshRenderer does not have a MeshFilter component.");
                return;
            }

            var sourceMesh1 = sourceMeshFilter.sharedMesh;

            GameObject = new GameObject("MeshFilterRendererClone");
            GameObject.transform.SetPositionAndRotation(source.transform.position,
                source.transform.rotation);
            GameObject.transform.localScale = source.transform.lossyScale;

            MeshFilter = GameObject.AddComponent<MeshFilter>();
            MeshFilter.sharedMesh = sourceMesh1;

            Renderer = GameObject.AddComponent<MeshRenderer>();
        }
    }


    public class SkinnedMeshRendererClone : MeshRendererCloneBase
    {
        private readonly SkinnedMeshRenderer _source;
        private readonly Mesh _bakedMesh;

        public SkinnedMeshRendererClone(SkinnedMeshRenderer sourceRenderer)
        {
            _source = sourceRenderer;
            _bakedMesh = new Mesh();

            GameObject = new GameObject("SkinnedMeshRendererClone");
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
}