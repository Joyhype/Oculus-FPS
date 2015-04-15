// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GsDistanceTrigger.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The gs distance trigger.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod.GroupSwitch
{
    using QuickLod.Attributes;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    using UnityEngine;

    /// <summary>
    /// The distance trigger.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Quick Lod/Group Switch/Distance Trigger")]
    public class GsDistanceTrigger : LodObjectBase, IGsTrigger
    {
        #region Fields

        /// <summary>
        /// The is triggered.
        /// </summary>
        [SerializeField]
        [LodObjectAddField("Is triggered", "Shows whether the distance trigger is currently triggered or not.", 
            LodObjectAddFieldAttribute.Category.General)]
        private bool isTriggered;

        /// <summary>
        /// The sphere mesh
        /// </summary>
        [SerializeField]
        [LodObjectAddField("Sphere mesh", 
            "The sphere mesh to use for displaying the trigger distance.\nLeave empty when you want the distance to be shown with default gizmos graphics."
            , LodObjectAddFieldAttribute.Category.Editor)]
        private Mesh sphereMesh;

        /// <summary>
        /// The trigger distance.
        /// </summary>
        [SerializeField]
        [LodObjectAddField("Trigger distance", "Define the relative distance in which the trigger is triggered.", 
            LodObjectAddFieldAttribute.Category.General)]
        private float triggerDistance;

        /// <summary>
        /// The triggered material.
        /// </summary>
        [SerializeField]
        [LodObjectAddField("Triggered material", 
            "The material to use when triggered.\nLeave empty to display this state with the default gizmos graphic.", 
            LodObjectAddFieldAttribute.Category.Editor)]
        private Material triggeredMaterial;

        /// <summary>
        /// The un triggered material.
        /// </summary>
        [SerializeField]
        [LodObjectAddField("Untriggered material", 
            "The material to use when not triggered.\nLeave empty to display this state with the default gizmos graphic."
            , LodObjectAddFieldAttribute.Category.Editor)]
        private Material untriggeredMaterial;

        /// <summary>
        /// The use relative distance 
        /// </summary>
        [SerializeField]
        [LodObjectAddField("Use relative distance",
            "Set if the relative distance should be used. If set to true, the relative distance will be used, otherwise the absolute distance will be used."
            , LodObjectAddFieldAttribute.Category.General)]
        private bool useRelativeDistance;

#if UNITY_EDITOR

        /// <summary>
        /// The draw gizmos.
        /// </summary>
        [SerializeField]
        [LodObjectAddField("Draw gizmos", "Draws the trigger distance.", LodObjectAddFieldAttribute.Category.Editor)]
        private bool drawGizmos;

        /// <summary>
        /// The draw when deselected.
        /// </summary>
        [SerializeField]
        [LodObjectAddField("Draw when deselected", "Draws the gizmos also when deselected",
            LodObjectAddFieldAttribute.Category.Editor)]
        private bool drawWhenDeselected;

        /// <summary>
        /// The draw in debug
        /// </summary>
        [SerializeField]
        [LodObjectAddField("Draw in debug", "Draws the gizmos in the game view",
            LodObjectAddFieldAttribute.Category.Editor)]
        private bool drawInDebug;

#endif

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether this instance is triggered.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is triggered; otherwise, <c>false</c>.
        /// </value>
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

        public string Name
        {
            get
            {
                return "Distance Trigger";
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use the relative distance or not
        /// </summary>
        /// <value>
        ///   <c>true</c> if the relative distance should be used; otherwise, <c>false</c>.
        /// </value>
        public bool UseRelativeDistance
        {
            get
            {
                return this.useRelativeDistance;
            }
            set
            {
                this.useRelativeDistance = value;
            }
        }

        /// <summary>
        /// Gets or sets the trigger distance.
        /// </summary>
        /// <value>
        /// The trigger distance.
        /// </value>
        public float TriggerDistance
        {
            get
            {
                return this.triggerDistance;
            }

            set
            {
                this.triggerDistance = value;
            }
        }

        #endregion

        public GsDistanceTrigger()
        {
            this.TriggerDistance = 50;
            this.UseRelativeDistance = true;
        }

        #region Public Methods and Operators

        /// <summary>
        /// Is called when all lod levels needs to be recalculated, usually in the initialization process
        /// </summary>
        public override void ForceUpdateAllLods()
        {
            this.isTriggered = this.CurrentLodLevel >= 0;
        }

        /// <summary>
        /// Gets the largest used lod distance
        /// </summary>
        /// <returns>
        /// Returns the largest found distance
        /// </returns>
        public override float GetLargestLodDistance()
        {
            return this.triggerDistance;
        }

        /// <summary>
        /// Gets the amount of lod levels that are setup for this lod level
        /// </summary>
        /// <returns>
        /// Returns the amount
        /// </returns>
        public override int GetLodAmount()
        {
            return 1;
        }

        /// <summary>
        /// Set the new distance for this object that uses distance multipliers
        /// </summary>
        /// <param name="newDistance">
        /// The new relative Distance
        /// </param>
        public override void SetNewRelativeDistance(float newDistance)
        {
            base.SetNewRelativeDistance(newDistance);

            if (this.useRelativeDistance)
            {
                this.CurrentLodLevel = newDistance > this.triggerDistance ? -1 : 0;
            }
        }

        public override void SetNewAbsoluteDistance(float newRealDistance)
        {
            base.SetNewAbsoluteDistance(newRealDistance);

            if (!this.useRelativeDistance)
            {
                this.CurrentLodLevel = newRealDistance > this.triggerDistance ? -1 : 0;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets called when the current lod level was changed
        /// </summary>
        protected override void OnLodLevelChanged()
        {
            this.OnActiveLodLevelChanged();

            this.isTriggered = this.CurrentLodLevel >= 0;
        }

#if UNITY_EDITOR

        private void Update()
        {
            // Draw the range
            if (!this.drawGizmos || (!this.drawWhenDeselected && !Selection.Contains(this.gameObject))
                || (!this.drawInDebug && Application.isPlaying) || this.sphereMesh == null)
            {
                return;
            }

            var usedMaterial = this.isTriggered ? this.triggeredMaterial : this.untriggeredMaterial;
            if (usedMaterial == null)
            {
                return;
            }

            var matrix = Matrix4x4.TRS(this.Transform.position, Quaternion.identity, Vector3.one * this.triggerDistance);
            Graphics.DrawMesh(this.sphereMesh, matrix, usedMaterial, 0);
        }

        /// <summary>
        /// The on draw gizmos.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (drawGizmos && this.drawWhenDeselected)
            {
                this.DrawGizmos();
            }
        }

        /// <summary>
        /// The on draw gizmos selected.
        /// </summary>
        private new void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (drawGizmos && !this.drawWhenDeselected)
            {
                this.DrawGizmos();
            }
        }

        /// <summary>
        /// The draw gizmos.
        /// </summary>
        private void DrawGizmos()
        {
            var usedMaterial = this.isTriggered ? this.triggeredMaterial : this.untriggeredMaterial;

            if (this.sphereMesh == null || usedMaterial == null)
            {
                Gizmos.color = this.isTriggered ? Color.green : Color.red;
                Gizmos.DrawWireSphere(this.Transform.position, this.triggerDistance);
            }
        }

#endif

        #endregion
    }
}