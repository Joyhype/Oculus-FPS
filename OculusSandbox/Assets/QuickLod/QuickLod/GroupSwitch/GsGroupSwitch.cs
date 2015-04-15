// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GsGroupSwitch.cs" company="Jasper Ermatinger">
//   Copyright © 2014 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The group switch.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod.GroupSwitch
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    using UnityEngine;

    /// <summary>
    /// The group switch.
    /// </summary>
    [AddComponentMenu("Quick Lod/Group Switch/Group Switch")]
    [ExecuteInEditMode]
    public class GsGroupSwitch : MonoBehaviour, ISerializationCallbackReceiver
    {
        #region Fields

        /// <summary>
        /// This field is used to store the registered triggers for serialization.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private MonoBehaviour[] temporarySerializationData;

        /// <summary>
        /// The managed objects.
        /// </summary>
        [SerializeField]
        private List<GameObject> managedObjects;

        /// <summary>
        /// The all managers.
        /// </summary>
        private List<IGsTrigger> triggers;

        /// <summary>
        /// The time span to wait before deactivating the objects
        /// </summary>
        [SerializeField]
        private float deactivationDelay;
        
        /// <summary>
        /// The force trigger state.
        /// </summary>
        [SerializeField]
        private bool forceTriggerState;

        /// <summary>
        /// The forced state.
        /// </summary>
        [SerializeField]
        private bool forcedState;

        /// <summary>
        /// The last time an object was active
        /// </summary>
        [SerializeField]
        private double lastActiveTime;

        [SerializeField]
        private bool isTriggered;

#if UNITY_EDITOR

        /// <summary>
        /// The object line color.
        /// </summary>
        [SerializeField]
        private Color objectLineColor = new Color(1f, 0.5f, 0f, 0.5f);

        /// <summary>
        /// The material used to highlight all managed objects
        /// </summary>
        [SerializeField]
        private Material selectionMaterial;
        
        [SerializeField]
        private bool drawLines;

        [SerializeField]
        private bool drawChildLines;

        [SerializeField]
        private bool highlightManagedObjects;

        [SerializeField]
        private bool drawWhenDeselected;
#endif
        
        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GsGroupSwitch"/> class.
        /// </summary>
        public GsGroupSwitch()
        {
            this.triggers = new List<IGsTrigger>();
            this.managedObjects = new List<GameObject>();

            this.deactivationDelay = 0.5f;
            this.forceTriggerState = false;
            this.forcedState = true;

#if UNITY_EDITOR
            this.drawLines = true;
            this.drawChildLines = false;
            this.drawChildLines = true;
            this.drawWhenDeselected = false;
#endif
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the managed objects
        /// </summary>
        public List<GameObject> ManagedObjects
        {
            get
            {
                return this.managedObjects;
            }

            set
            {
                this.managedObjects = value;
            }
        }

        public List<IGsTrigger> Triggers
        {
            get
            {
                return this.triggers;
            }
            
            set
            {
                this.triggers = value;
            }
        }

        public ReadOnlyCollection<MonoBehaviour> TriggerObjects
        {
            get
            {
                return this.triggers.OfType<MonoBehaviour>().ToList().AsReadOnly();
            }
        }

        public bool ForceTriggerState
        {
            get
            {
                return this.forceTriggerState;
            }

            set
            {
                this.forceTriggerState = value;
            }
        } 

        public bool ForcedState
        {
            get
            {
                return this.forcedState;
            }
            set
            {
                this.forcedState = value;
            }
        }

        public float DeactivationDelay
        {
            get
            {
                return this.deactivationDelay;
            }
            set
            {
                this.deactivationDelay = value;
            }
        }

        public bool IsTriggered
        {
            get
            {
                return this.isTriggered;
            }
        }

        public double LastActiveTime
        {
            get
            {
                return this.lastActiveTime;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// <para>Changes the control to manual and forces the trigger state with deactivation delay</para>
        /// <para>[You can also use the properties "ForceTriggerState" and "ForcedState" instead]</para>
        /// </summary> 
        /// <param name="triggerState">
        /// The trigger state that should be forced
        /// </param>
        public void ForceState(bool triggerState)
        {
            this.forceTriggerState = true;
            this.forcedState = triggerState;
        }

        /// <summary>
        /// <para>Changes the control to manual and forces the trigger state immediately</para>
        /// <para>[You can also use the properties "ForceTriggerState" and "ForcedState" instead]</para>
        /// </summary>
        /// <param name="triggerState">
        /// The trigger state that should be forced
        /// </param>
        public void ForceStateImmediate(bool triggerState)
        {
            this.ForceState(triggerState);
            this.isTriggered = triggerState;
            this.SetObjectsActive(triggerState);
        }

        /// <summary>
        /// <para>Changes the control to automatic with deactivation delay</para>
        /// <para>[You can also use the properties "ForceTriggerState" and "ForcedState" instead]</para>
        /// </summary>
        public void ReleaseForcedState()
        {
            this.forceTriggerState = false;
        }

        /// <summary>
        /// <para>Changes the control to automatic immediately</para>
        /// <para>[You can also use the properties "ForceTriggerState" and "ForcedState" instead]</para>
        /// </summary>
        public void ReleaseForcedStateImmediate()
        {
            this.ReleaseForcedState();

            if (this.Triggers.All(trigger => !trigger.IsTriggered))
            {
                this.isTriggered = false;
                this.SetObjectsActive(false);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Is called when the group switch gets activated
        /// </summary>
        private void OnEnable()
        {
#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
            {
                EditorApplication.update += this.EditorUpdate;
            }
#endif

            this.lastActiveTime = this.GetCurrentTimeInSeconds();
        }

        /// <summary>
        /// Is called when the group switch gets deactivated 
        /// </summary>
        private void OnDisable()
        {
#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
            {
                EditorApplication.update -= this.EditorUpdate;
            }
#endif

            this.SetObjectsActive(true);
            this.isTriggered = true;
        }

        /// <summary>
        /// Is called in every update cycle
        /// </summary>
        private void Update()
        {
            if (Application.isPlaying)
            {
                this.CheckForChanges();
            }
#if UNITY_EDITOR
            else if ((this.drawWhenDeselected ||Selection.Contains(this.gameObject)) && this.highlightManagedObjects && this.selectionMaterial != null)
            {
                foreach (var obj in this.managedObjects.Where(obj => obj != null))
                {
                    this.DrawChildObjects(obj);
                }
            }
#endif
        }

        /// <summary>
        /// Check whether the managed objects should be switched or not
        /// </summary>
        private void CheckForChanges()
        {
            var currentTime = this.GetCurrentTimeInSeconds();

            if ((this.forceTriggerState && this.forcedState)
                || (!this.forceTriggerState && this.GetIsTriggered()))
            {
                if (!this.isTriggered)
                {
                    // Activate triggered state
                    this.isTriggered = true;
                    this.SetObjectsActive(true);
                }

                this.lastActiveTime = currentTime;
            }
            else if (this.isTriggered && currentTime > this.lastActiveTime + this.deactivationDelay)
            {
                // Deactivate triggered state
                this.isTriggered = false;
                this.SetObjectsActive(false);
            }
        }

        private bool GetIsTriggered()
        {
            for (var i = this.triggers.Count - 1; i >= 0; i--)
            {
                if (this.triggers[i] == null)
                {
                    this.triggers.RemoveAt(i);
                    continue;
                }

                if (this.triggers[i].IsTriggered)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The set children active.
        /// </summary>
        /// <param name="isActive">
        /// The is active.
        /// </param>
        private void SetObjectsActive(bool isActive)
        {
            foreach (var obj in this.managedObjects.Where(obj  => obj != null))
            {
                obj.gameObject.SetActive(isActive);
            }
        }

        /// <summary>
        /// Converts the current time into seconds
        /// </summary>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        private double GetCurrentTimeInSeconds()
        {
            var result = DateTime.Now.Ticks / 10000000d;
            return result;
        }

#if UNITY_EDITOR

        private void EditorUpdate()
        {
            if (!Application.isPlaying)
            {
                this.CheckForChanges(); 
            }
        }

        /// <summary>
        /// The draw child objects.
        /// </summary>
        /// <param name="rootObject">
        /// The root object.
        /// </param>
        private void DrawChildObjects(GameObject rootObject)
        {
            // Draw the mesh in the mesh filter or skinned mesh renderer with the highlight material
            if (this.highlightManagedObjects)
            {
                var meshFilter = rootObject.gameObject.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    Graphics.DrawMesh(
                        meshFilter.sharedMesh, rootObject.transform.localToWorldMatrix, this.selectionMaterial, 0);
                }
                else
                {
                    var skinnedMeshRenderer = rootObject.gameObject.GetComponent<SkinnedMeshRenderer>();
                    if (skinnedMeshRenderer != null)
                    {
                        Graphics.DrawMesh(
                            skinnedMeshRenderer.sharedMesh, rootObject.transform.localToWorldMatrix, this.selectionMaterial, 0);
                    }
                }
            }

            // Repeat for children
            for (var i = 0; i < rootObject.transform.childCount; i++)
            {
                var child = rootObject.transform.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                this.DrawChildObjects(child.gameObject);
            }
        }

        /// <summary>
        /// Draws the child lines.
        /// </summary>
        /// <param name="rootObject">The root object.</param>
        private void DrawChildLines(GameObject rootObject)
        {
            for (var i = 0; i < rootObject.transform.childCount; i++)
            {
                var child = rootObject.transform.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (this.drawLines && this.drawChildLines)
                {
                    Handles.DrawLine(rootObject.transform.position, child.position);
                }

                this.DrawChildLines(child.gameObject);
            }
        }

        /// <summary>
        /// The draw gizmos.
        /// </summary>
        private void DrawGizmos()
        {
            if (!this.drawWhenDeselected && !Selection.Contains(this.gameObject))
            {
                return;
            }

            if (this.drawLines)
            {
                Handles.color = this.objectLineColor;
                foreach (var obj in this.managedObjects.Where(obj => obj != null))
                {
                    Handles.DrawLine(this.gameObject.transform.position, obj.transform.position);

                    if (drawChildLines)
                    {
                        this.DrawChildLines(obj);
                    }
                }
            }
        }

        /// <summary>
        /// The on draw gizmos.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (this.drawWhenDeselected)
            {
                this.DrawGizmos();
            }
        }

        /// <summary>
        /// The on draw gizmos selected.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (!this.drawWhenDeselected)
            {
                this.DrawGizmos();
            }
        }

#endif

        #endregion

        /// <summary>
        /// Called before unity starts serializing this object
        /// </summary>
        public void OnBeforeSerialize()
        {
            this.temporarySerializationData = this.triggers.OfType<MonoBehaviour>().ToArray();
        }

        /// <summary>
        /// Called when unity finished deserializing the component
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (temporarySerializationData == null)
            {
                return; 
            }

            this.triggers = this.temporarySerializationData.OfType<IGsTrigger>().ToList();
            this.temporarySerializationData = null;
        }
    }
}