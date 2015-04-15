// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LodObjectChildSwitch.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The lod object child switch.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod
{
    using QuickLod.Attributes;

    using UnityEngine;

    /// <summary>
    /// The lod object child switch.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Quick Lod/Lod Object Child Switch")]
    public class LodObjectChildSwitch : LodObjectBase
    {
        #region Fields

        /// <summary>
        /// The active distance.
        /// </summary>
        [SerializeField]
        [LodObjectAddField("Active distance", "The maximum distance at which the child objects are still active.", 
            LodObjectAddFieldAttribute.Category.Lods)]
        private float activeDistance;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LodObjectChildSwitch"/> class.
        /// </summary>
        public LodObjectChildSwitch()
        {
            this.activeDistance = 10;
            this.CanGenerateLodLevels = false;
            this.CanCalculateDistances = true;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the active distance.
        /// </summary>
        public float ActiveDistance
        {
            get
            {
                return this.activeDistance;
            }

            set
            {
                this.activeDistance = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The force update all lods.
        /// </summary>
        public override void ForceUpdateAllLods()
        {
            this.SetChildrenActive();
        }

        /// <summary>
        /// The get largest lod distance.
        /// </summary>
        /// <returns>
        /// The <see cref="float"/>.
        /// </returns>
        public override float GetLargestLodDistance()
        {
            return this.activeDistance;
        }

        /// <summary>
        /// The get lod amount.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public override int GetLodAmount()
        {
            return 1;
        }

        /// <summary>
        /// Automatically recalculates the optimal distances of the lod levels
        /// </summary>
        /// <remarks>
        /// A lod object is free to support the automatic distance calculation.<br />
        /// The <see cref="CanCalculateDistances" /> tells you if this behavior is supported or not.
        /// </remarks>
        /// <example>
        /// This example shows a simple way to implement your own distance generator.
        /// <code>
        /// public override void RecalculateDistances()
        /// {
        /// base.RecalculateDistances();
        /// var maxDistance = 100;
        /// var distanceStep = maxDistance / this.lods.Length;
        /// for (var index = 0; index &lt; this.lods.Length; index++)
        /// {
        /// this.lods[index].Distance = (index + 1) * distanceStep;
        /// }
        /// }
        /// </code></example>
        [ContextMenu("Recalculate distances")]
        public override void RecalculateDistances()
        {
            base.RecalculateDistances();

            var bounds = new Bounds();
            var maxLodDist = 0f;
            var t = this.gameObject.transform;

            for (var index = 0; index < t.childCount; index++)
            {
                var ct = t.GetChild(index);
                bounds.Encapsulate(ct.position);

                var lodObject = ct.gameObject.GetComponent(typeof(LodObjectBase)) as LodObjectBase;
                if (lodObject != null)
                {
                    var dist = Vector3.Distance(ct.position, t.position);
                    maxLodDist = Mathf.Max(maxLodDist, lodObject.GetLargestLodDistance() + dist);
                }
            }

            var size = Mathf.Max(bounds.size.magnitude, maxLodDist);
            this.activeDistance = Mathf.Round(size);
        }

        /// <summary>
        /// The set new relative distance.
        /// </summary>
        /// <param name="distance">
        /// The distance.
        /// </param>
        public override void SetNewRelativeDistance(float distance)
        {
            base.SetNewRelativeDistance(distance);
            this.CurrentLodLevel = distance <= this.activeDistance ? 0 : -1;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on lod level changed.
        /// </summary>
        protected override void OnLodLevelChanged()
        {
            this.SetChildrenActive();
        }

        /// <summary>
        /// The set children active.
        /// </summary>
        private void SetChildrenActive()
        {
            var isActive = this.CurrentLodLevel == 0;
            var t = this.gameObject.transform;

            for (var i = 0; i < t.childCount; i++)
            {
                t.GetChild(i).gameObject.SetActive(isActive);
            }
        }

        #endregion
    }
}