// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GsCollisionTrigger.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The gs collision trigger.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod.GroupSwitch
{
    using System.Collections.Generic;
    using System.Linq;

    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif


    /// <summary>
    /// The gs collision trigger.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Quick Lod/Group Switch/Colission Trigger")]
    [RequireComponent(typeof(Rigidbody))]
    public class GsCollisionTrigger : MonoBehaviour, IGsTrigger
    {
        #region Fields
        
        /// <summary>
        /// Contains all colliders inside any watched col.<br/>
        /// Using a dictionary is much faster than using a list duo to improved add() and remove() methods
        /// </summary>
        private List<Collider> collidersInside;

        /// <summary>
        /// Contains a list of colliders which meet the filter criterias<br/>
        /// Using a dictionary is much faster than using a list duo to improved add() and remove() methods
        /// </summary>
        private List<Collider> consideredColliders;

        /// <summary>
        /// The considered layers.
        /// </summary>
        [SerializeField]
        private LayerMask consideredLayers;

        /// <summary>
        /// The considered tags.
        /// </summary>
        [SerializeField]
        private string[] consideredTags;
        
        /// <summary>
        /// The filter by layers.
        /// </summary>
        [SerializeField]
        private bool filterByLayers;

        /// <summary>
        /// The filter by tags.
        /// </summary>
        [SerializeField]
        private bool filterByTags;
        
        /// <summary>
        /// The is triggered.
        /// </summary>
        [SerializeField]
        private bool isTriggered;

#if UNITY_EDITOR

        /// <summary>
        /// The draw colliders.
        /// </summary>
        [SerializeField]
        private bool drawColliders;

        /// <summary>
        /// The draw when deselected
        /// </summary>
        [SerializeField]
        private bool drawWhenDeselected;

        /// <summary>
        /// The draw colliders in debug.
        /// </summary>
        [SerializeField]
        private bool drawInDebug;

        /// <summary>
        /// The active trigger material.
        /// </summary>
        [SerializeField]
        private Material activeTriggerMaterial;

        /// <summary>
        /// The inactive trigger material.
        /// </summary>
        [SerializeField]
        private Material inactiveTriggerMaterial;

        private static GameObject cubePrimitive;

        private static Mesh cubeMesh;

        private static GameObject spherePrimitive;

        private static Mesh sphereMesh;

        private static GameObject capsulePrimitive;

        private static Mesh capsuleMesh;

        private static int primitiveUsers;

#endif

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GsCollisionTrigger"/> class.
        /// </summary>
        public GsCollisionTrigger()
        {
            this.filterByLayers = false;
            this.filterByTags = false;
            this.consideredLayers = ~0;
            this.consideredTags = new[] { "MainCamera", "Player" };
            this.collidersInside = new List<Collider>();
            this.consideredColliders = new List<Collider>();

#if UNITY_EDITOR

            this.drawColliders = true;
            this.drawInDebug = true;

#endif
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the considered layers.
        /// </summary>
        public LayerMask ConsideredLayers
        {
            get
            {
                return this.consideredLayers;
            }

            set
            {
                this.consideredLayers = value;
            }
        }

        /// <summary>
        /// Gets or sets the considered tags.
        /// </summary>
        public string[] ConsideredTags
        {
            get
            {
                return this.consideredTags;
            }

            set
            {
                this.consideredTags = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether filter by layers.
        /// </summary>
        public bool FilterByLayers
        {
            get
            {
                return this.filterByLayers;
            }

            set
            {
                this.filterByLayers = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether filter by tags.
        /// </summary>
        public bool FilterByTags
        {
            get
            {
                return this.filterByTags;
            }

            set
            {
                this.filterByTags = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether is triggered.
        /// </summary>
        public bool IsTriggered
        {
            get
            {
                return this.isTriggered;
            }

            private set
            {
                this.isTriggered = value;
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name
        {
            get
            {
                return "Colission Trigger";
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Applies the new filter settings.
        /// </summary>
        private void ApplyNewFilterSettings()
        {
            this.consideredColliders =
                this.collidersInside.Where(col => col != null && this.IsColliderConsidered(col.gameObject)).ToList();

            this.IsTriggered = this.consideredColliders.Count > 0;
        }
        
        /// <summary>
        /// The is col considered.
        /// </summary>
        /// <param name="go">
        /// The game object.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool IsColliderConsidered(GameObject go)
        {
            if (this.filterByLayers && (1 << go.layer & this.consideredLayers.value) == 0)
            {
                return false;
            }

            var objectTag = go.tag;
            if (this.filterByTags && this.consideredTags.All(s => s != objectTag))
            {
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// The on trigger enter.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        private void OnTriggerEnter(Collider other)
        {
            this.collidersInside.Add(other);

            if (this.IsColliderConsidered(other.gameObject))
            {
                this.consideredColliders.Add(other);
                this.IsTriggered = this.consideredColliders.Count > 0;
            }
        }

        /// <summary>
        /// The on trigger exit.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        private void OnTriggerExit(Collider other)
        {
            this.collidersInside.Remove(other);
            if (this.consideredColliders.Any(col => col == other))
            {
                this.consideredColliders.Remove(other);
                this.IsTriggered = this.consideredColliders.Count > 0;
            }
        }
        
#if UNITY_EDITOR
        
        /// <summary>
        /// The draw box collider.
        /// </summary>
        /// <param name="col">
        /// The col.
        /// </param>
        private static void DrawBoxCollider(BoxCollider col, Material material)
        {
            var t = col.transform;
            var matrix = Matrix4x4.TRS(
                col.bounds.center,
                t.rotation,
                new Vector3(t.lossyScale.x * col.size.x, t.lossyScale.y * col.size.y, t.lossyScale.z * col.size.z));
            Graphics.DrawMesh(cubeMesh, matrix, material, 0);
        }

        /// <summary>
        /// The draw capsule collider.
        /// </summary>
        /// <param name="col">
        ///     The col.
        /// </param>
        /// <param name="material"></param>
        private static void DrawCapsuleCollider(CapsuleCollider col, Material material)
        {
            var rotation = Vector3.zero;
            var size = col.transform.lossyScale;
            var radius = Mathf.Max(Mathf.Abs(size.x), Mathf.Abs(size.z)) * col.radius * 2;
            size.x = radius;
            size.y = Mathf.Max(Mathf.Abs(size.y * col.height), radius) / 2;
            size.z = radius;

            switch (col.direction)
            {
                case 0:
                    {
                        rotation.z = 90;
                        break;
                    }
                case 1:
                    {
                        break;
                    }
                case 2:
                    {
                        rotation.x = 90;
                        break;
                    }
            }

            var matrix = Matrix4x4.TRS(col.bounds.center, Quaternion.Euler(rotation) * col.transform.rotation, size);
            Graphics.DrawMesh(capsuleMesh, matrix, material, 0);
        }

        /// <summary>
        /// The draw mesh collider.
        /// </summary>
        /// <param name="col">
        /// The col.
        /// </param>
        /// <param name="material">
        /// The material.
        /// </param>
        private static void DrawMeshCollider(MeshCollider col, Material material)
        {
            var t = col.transform;
            Graphics.DrawMesh(col.sharedMesh, t.localToWorldMatrix, material, 0);
        }

        /// <summary>
        /// The draw other collider.
        /// </summary>
        /// <param name="col">
        /// The col.
        /// </param>
        private static void DrawOtherCollider(Collider col, Material material)
        {
            var t = col.transform;
            var matrix = Matrix4x4.TRS(col.bounds.center, t.rotation, col.bounds.size);
            Graphics.DrawMesh(cubeMesh, matrix, material, 0);
        }

        /// <summary>
        /// The draw sphere collider.
        /// </summary>
        /// <param name="col">
        ///     The col.
        /// </param>
        /// <param name="material"></param>
        private static void DrawSphereCollider(SphereCollider col, Material material)
        {
            var matrix = Matrix4x4.TRS(col.bounds.center, Quaternion.identity, col.bounds.size); 
            Graphics.DrawMesh(sphereMesh, matrix, material, 0);
        }
        
        /// <summary>
        /// The draw colliders.
        /// </summary>
        private void DrawColliders()
        {
            Gizmos.color = this.isTriggered ? Color.green : Color.red;
            var colliders = this.GetComponentsInChildren<Collider>(false).Where(col => col.isTrigger && col.enabled);
            var material = this.isTriggered ? this.activeTriggerMaterial : this.inactiveTriggerMaterial;

            if (material == null)
            {
                return;
            }

            foreach (var col in colliders)
            {
                var colType = col.GetType();

                if (colType == typeof(BoxCollider))
                {
                    var bc = col as BoxCollider;
                    DrawBoxCollider(bc, material);
                }
                else if (colType == typeof(SphereCollider))
                {
                    var sc = col as SphereCollider;
                    DrawSphereCollider(sc, material);
                }
                else if (colType == typeof(CapsuleCollider))
                {
                    var cc = col as CapsuleCollider;
                    DrawCapsuleCollider(cc, material);
                }
                else if (colType == typeof(MeshCollider))
                {
                    var mc = col as MeshCollider;
                    DrawMeshCollider(mc, material);
                }
                else
                {
                    DrawOtherCollider(col, material);
                }
            }
        }
        
        /// <summary>
        /// The on validate.
        /// </summary>
        private void OnValidate()
        {
            this.ApplyNewFilterSettings();
        }

        /// <summary>
        /// The update.
        /// </summary>
        private void Update()
        {
            if (this.drawColliders && (this.drawWhenDeselected || Selection.Contains(this.gameObject))
                && (!Application.isPlaying || drawInDebug))
            {
                this.DrawColliders();
            }
        }

        private void OnEnable()
        {
            if (primitiveUsers == 0)
            {
                cubePrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cubePrimitive.hideFlags = HideFlags.HideAndDontSave;
                cubePrimitive.SetActive(false);
                cubeMesh = ((MeshFilter)cubePrimitive.GetComponent(typeof(MeshFilter))).sharedMesh;

                spherePrimitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spherePrimitive.hideFlags = HideFlags.HideAndDontSave;
                spherePrimitive.SetActive(false);
                sphereMesh = ((MeshFilter)spherePrimitive.GetComponent(typeof(MeshFilter))).sharedMesh;

                capsulePrimitive = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                capsulePrimitive.hideFlags = HideFlags.HideAndDontSave;
                capsulePrimitive.SetActive(false);
                capsuleMesh = ((MeshFilter)capsulePrimitive.GetComponent(typeof(MeshFilter))).sharedMesh;
            }

            primitiveUsers++;
        }

        private void OnDisable()
        {
            primitiveUsers--;

            if (primitiveUsers == 0)
            {
                DestroyImmediate(cubePrimitive);
                DestroyImmediate(spherePrimitive);
                DestroyImmediate(capsulePrimitive);

                cubeMesh = null;
                cubePrimitive = null;
                sphereMesh = null;
                spherePrimitive = null;
                capsuleMesh = null;
                capsulePrimitive = null;
            }
        }
        
#endif

        #endregion
    }
}