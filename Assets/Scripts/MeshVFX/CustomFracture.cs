using System.Collections.Generic;
using UnityEngine;

namespace MeshVFX
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class CustomFracture : MonoBehaviour
    {
        public FractureOptions fractureOptions;
    
        public GameObject fragmentTemplatePrefab;

        /// <summary>
        /// Collector object that stores the produced fragments
        /// </summary>
        private GameObject _fragmentRoot;
    
        private bool _isFractured = false;

        [ContextMenu("Print Mesh Info")]
        public void PrintMeshInfo()
        {
            var mesh = this.GetComponent<MeshFilter>().mesh;
            Debug.Log("Positions");

            var positions = mesh.vertices;
            var normals = mesh.normals;
            var uvs = mesh.uv;

            for (int i = 0; i < positions.Length; i++)
            {
                Debug.Log($"Vertex {i}");
                Debug.Log($"POS | X: {positions[i].x} Y: {positions[i].y} Z: {positions[i].z}");
                Debug.Log(
                    $"NRM | X: {normals[i].x} Y: {normals[i].y} Z: {normals[i].z} LEN: {normals[i].magnitude}");
                Debug.Log($"UV  | U: {uvs[i].x} V: {uvs[i].y}");
                Debug.Log("");
            }
        }

        public GameObject CauseFracture()
        {
            return ComputeFracture();
        }

        void OnValidate()
        {
            if (transform.parent != null)
            {
                // When an object is fractured, the fragments are created as children of that object's parent.
                // Because of this, they inherit the parent transform. If the parent transform is not scaled
                // the same in all axes, the fragments will not be rendered correctly.
                var scale = transform.parent.localScale;
                if (!Mathf.Approximately(scale.x, scale.y) || !Mathf.Approximately(scale.x, scale.z) || !Mathf.Approximately(scale.y, scale.z))
                {
                    Debug.LogWarning(
                        "Warning: Parent transform of fractured object must be uniformly scaled in all axes or fragments will not render correctly.",
                        transform);
                }
            }
        }

        /// <summary>
        /// Compute the fracture and create the fragments
        /// </summary>
        /// <returns></returns>
        private GameObject ComputeFracture()
        {
            if (_isFractured) return null;
        
            List<GameObject> fragments = new List<GameObject>();
        
            var mesh = GetComponent<MeshFilter>().sharedMesh;

            if (mesh != null)
            {
                // If the fragment root object has not yet been created, create it now
                if (_fragmentRoot == null)
                {
                    // Create a game object to contain the fragments
                    _fragmentRoot = new GameObject($"{name}Fragments");
                    _fragmentRoot.transform.SetParent(transform.parent);

                    // Each fragment will handle its own scale
                    _fragmentRoot.transform.position = transform.position;
                    _fragmentRoot.transform.rotation = transform.rotation;
                    _fragmentRoot.transform.localScale = Vector3.one;
                }

                var fragmentTemplate = CreateFragmentTemplate();

                {
                    Fragmenter.Fracture(gameObject,
                        fractureOptions,
                        fragmentTemplate,
                        _fragmentRoot.transform);

                    // Done with template, destroy it
                    Destroy(fragmentTemplate);

                    // Deactivate the original object
                    _isFractured = true;
                    // set the renderers invisible
                    foreach (var renderer1 in GetComponentsInChildren<Renderer>())
                    {
                        renderer1.enabled = false;
                    }
                    return _fragmentRoot;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a template object which each fragment will derive from
        /// </summary>
        /// <param name="preFracture">True if this object is being pre-fractured. This will freeze all of the fragments.</param>
        /// <returns></returns>
        private GameObject CreateFragmentTemplate()
        {
            var obj = fragmentTemplatePrefab != null ? Instantiate(fragmentTemplatePrefab) : new GameObject();
            obj.name = "Fragment";
            obj.tag = tag;

            // Update mesh to the new sliced mesh
            obj.AddComponent<MeshFilter>();

            // Add materials. Normal material goes in slot 1, cut material in slot 2
            var meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = new Material[2]
            {
                GetComponent<MeshRenderer>().sharedMaterial,
                GetComponent<MeshRenderer>().sharedMaterial
            };

            // Copy collider properties to fragment
            var thisCollider = GetComponent<Collider>();
            var fragmentCollider = obj.AddComponent<MeshCollider>();
            fragmentCollider.convex = true;
            if (thisCollider)
            {
                fragmentCollider.sharedMaterial = thisCollider.sharedMaterial;
                fragmentCollider.isTrigger = thisCollider.isTrigger;
            }


            // Copy rigid body properties to fragment
            var thisRigidBody = GetComponent<Rigidbody>();
            if (thisRigidBody)
            {
                // Copy rigid body properties to fragment
                var fragmentRigidBody = obj.AddComponent<Rigidbody>();
                fragmentRigidBody.collisionDetectionMode = thisRigidBody.collisionDetectionMode;
                fragmentRigidBody.interpolation = thisRigidBody.interpolation;
                fragmentRigidBody.linearVelocity = thisRigidBody.linearVelocity;
                fragmentRigidBody.angularVelocity = thisRigidBody.angularVelocity;
                fragmentRigidBody.linearDamping = thisRigidBody.linearDamping;
                fragmentRigidBody.angularDamping = thisRigidBody.angularDamping;
                // fragmentRigidBody.useGravity = thisRigidBody.useGravity;
                // fragmentRigidBody.isKinematic = true;
                // fragmentRigidBody.useGravity = false;
            }
            return obj;
        }
    }
}