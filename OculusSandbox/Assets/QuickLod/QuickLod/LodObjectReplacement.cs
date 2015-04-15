// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LodObjectReplacement.cs" company="Jasper Ermatinger">
//   Copyright © 2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   A lod object that enables and disables referenced gameobjects depending on the current lod level.
//   Has a large memory overhead as each lod level requires a new object.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using QuickLod.Attributes;
    using QuickLod.Containers;

    using UnityEngine;

    /// <summary>
    /// A lod object that enables and disables referenced <see cref="GameObject"/> depending on the <see cref="LodObjectBase.CurrentLodLevel"/><br/>
    /// Has a large memory overhead as each lod level requires a new object.<br/>
    /// Use <see cref="LodObjectMesh"/> if you don't need to switch components or materials
    /// </summary>
    [AddComponentMenu("Quick Lod/Lod Object Replacement")]
    [ExecuteInEditMode]
    [LodHelper(@"This component manages game objects depending on the distance.
It can be used if the object has multiple subobjects which represents the different lod levels.

You need to define the lod levels in the ""Lods"" field.
To automaticly setup the objects, you can use the ""Generate all"" function.
To optimize the distance based on the mesh size, you can use the ""Optimize distances"" option.

When the object does not move, you should activate the ""Is static"" option.
When you want to set the current lod level manualy, you must activate the ""Exclude from manager"" option.

The relative distance is the distance to the nearest lod source including the lod multiplier.
The absolute distance is the real distance to the nearest lod source.")]
    public class LodObjectReplacement : LodObjectBase
    {
        #region private fields

        /// <summary>
        /// Backing field for <see cref="Lods"/>
        /// </summary>
        [SerializeField]
        private LodStructureGameobject[] lods;

#if UNITY_EDITOR

        /// <summary>
        /// Stores the length of the previous lods
        /// </summary>
        private int prevLodsLength;
#endif

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LodObjectReplacement"/> class.
        /// </summary>
        public LodObjectReplacement()
        {
            this.lods = new LodStructureGameobject[3];
            this.lods[0] = new LodStructureGameobject();
            this.lods[1] = new LodStructureGameobject();
            this.lods[2] = new LodStructureGameobject();

            this.CanCalculateDistances = true;
            this.CanGenerateLodLevels = true;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets all lod levels of this object
        /// </summary>
        public LodStructureGameobject[] Lods
        {
            get
            {
                return this.lods;
            }

            set
            {
                if (this.lods == value)
                {
                    return;
                }

                this.lods = value;

                foreach (var lod in this.Lods.Where(lod => lod != null))
                {
                    lod.IsVisible = true;
#if UNITY_EDITOR
                    lod.PreviousDistance = lod.Distance;
                    lod.PreviousLod = lod.Lod;
#endif
                }

#if UNITY_EDITOR
                this.prevLodsLength = this.lods.Length;
#endif

                // Forces the update with distance recalculation
                this.SetNewRelativeDistance(this.RelativeDistance);
                this.ForceUpdateAllLods();
            }
        }

        /// <summary>
        /// Gets or sets the local hide mode
        /// </summary>
        [Obsolete("The hide mode is obsolete.\nUse the LodObjectMesh or LodObjectSkinnedMesh instead.")]
        public LodManagerBase.CullMode HideMode
        {
            get
            {
                return LodManagerBase.CullMode.Object;
            }

            set
            {
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// The get lod amount.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public override int GetLodAmount()
        {
            return this.lods.Length;
        }

        /// <summary>
        /// Force updates all lod levels, usually called when the object gets initialized
        /// </summary>
        public override void ForceUpdateAllLods()
        {
            // Loop through all lods and update them
            for (var i = 0; i < this.Lods.Length; i++)
            {
                // Ignore lods that don't have a game object reference
                if (this.Lods[i].Lod == null)
                {
                    continue;
                }

                // Get if the lod is visible or not
                var isLodVisible = i == this.CurrentLodLevel;
                this.Lods[i].Lod.SetActive(isLodVisible);
                this.Lods[i].IsVisible = isLodVisible;
            }
        }

        /// <summary>
        /// Gets the largest used lod distance used in this lod object
        /// </summary>
        /// <returns>
        /// Returns the largest found distance
        /// </returns>
        public override float GetLargestLodDistance()
        {
            return this.Lods.Select(lod => lod.Distance).Concat(new[] { 0f }).Max();
        }

        /// <summary>
        /// Set a new distance for this object that uses the distance multiplier
        /// </summary>
        /// <param name="newDistance">
        /// The new distance.
        /// </param>
        public override void SetNewRelativeDistance(float newDistance)
        {
            base.SetNewRelativeDistance(newDistance);

            // Loop though all lods
            for (var lodIndex = 0; lodIndex < this.Lods.Length; lodIndex++)
            {
                // Get the first lod where the distance is over the relative distance
                if (this.lods[lodIndex].Distance > newDistance)
                {
                    this.CurrentLodLevel = lodIndex;
                    return;
                }
            }

            // If all lod distances are smaller than the relative distance, hide all lods
            this.CurrentLodLevel = -1;
        }

        /// <summary>
        /// Tries to automatically assign the lod level objects with the child game objects
        /// </summary>
        public override void AutoAssignLodLevels()
        {
            this.lods = new LodStructureGameobject[0];

            // Get all children
            var childs = new List<GameObject>();
            for (var index = 0; index < this.gameObject.transform.childCount; index++)
            {
                childs.Add(this.gameObject.transform.GetChild(index).gameObject);
            }

            // Check if children names ends with numbers, if yes, sort them by number and add them to the list
            var numberedChilds = (from child in childs
                                  let value = GetNumberAtEndOfString(child.name)
                                  where value != int.MinValue
                                  select new KeyValuePair<GameObject, int>(child, value)).ToList();

            numberedChilds.Sort((x, y) => x.Value.CompareTo(y.Value));

            this.lods = new LodStructureGameobject[numberedChilds.Count];
            for (int index = 0; index < numberedChilds.Count; index++)
            {
                this.lods[index] = new LodStructureGameobject { Lod = numberedChilds[index].Key };
            }

            this.RecalculateDistances();
        }

        /// <summary>
        /// Automatic recalculates the optimal distances of the lod levels
        /// </summary>
        [ContextMenu("Recalculate distances")]
        public override void RecalculateDistances()
        {
            base.RecalculateDistances();

            if (this.lods.Length == 0)
            {
                return;
            }

            // Get the boundary of the first lod level mesh
            var maxBounds = new Bounds(Vector3.zero, Vector3.zero);

            var meshFilters = this.lods[0].Lod.GetComponentsInChildren<MeshFilter>(true);
            if (meshFilters != null)
            {
                foreach (var meshFilter in meshFilters)
                {
                    maxBounds.Encapsulate(meshFilter.sharedMesh.bounds);
                }
            }
            else
            {
                return;
            }

            var scale = this.gameObject.transform.lossyScale;

            var diagonale = Vector3.Scale(maxBounds.size, scale).magnitude;

            var distances = GetOptimalLevelDistances(diagonale, this.lods.Length);

            for (int index = 0; index < this.lods.Length; index++)
            {
                this.lods[index].Distance = distances[index];
            }
        }

        /// <summary>
        /// The activate quick lod.
        /// </summary>
        public override void ActivateQuickLod()
        {
            base.ActivateQuickLod();

            var iocLod = this.gameObject.GetComponent("IOClod") as MonoBehaviour;
            if (iocLod != null)
            {
                iocLod.enabled = false;
            }

            foreach (var lod in this.lods)
            {
                lod.Lod.SetActive(true);
                var renderers = lod.Lod.GetComponentsInChildren<Renderer>(true);
                if (renderers != null)
                {
                    foreach (var r in renderers)
                    {
                        r.enabled = true;
                    }
                }
            }

            this.enabled = true;
        }

        /// <summary>
        /// The deactivate quick lod.
        /// </summary>
        public override void DeactivateQuickLod()
        {
            base.DeactivateQuickLod();

            foreach (var lod in this.lods)
            {
                lod.Lod.SetActive(true);
                lod.Lod.GetComponent<Renderer>().enabled = true;
            }

            this.enabled = false;

            var iocLod = this.gameObject.GetComponent("IOClod");
            if (iocLod != null)
            {
                var type = Type.GetType("IOClod");
                if (type != null)
                {
                    type.GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(iocLod, null);
                    type.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(iocLod, null);
                    type.GetProperty("enabled").SetValue(iocLod, true, null);
                }
            }
        }

        #endregion

        #region protected methods

        /// <summary>
        /// Makes all lods visible
        /// </summary>
        protected new void OnDisable()
        {
            base.OnDisable();

            // Loops through all lods and makes them visible
            foreach (var lod in this.lods.Where(lod => lod.Lod != null))
            {
                lod.Lod.SetActive(true);
                lod.IsVisible = true;
            }
        }

        /// <summary>
        /// Updates the lod levels depending on the current lod level
        /// </summary>
        protected override void OnLodLevelChanged()
        {
            // Hide all lod levels
            this.HideAllLods();

            // Only update when necessary and possible
            if (this.CurrentLodLevel > -1 && this.CurrentLodLevel < this.Lods.Length
                && this.Lods[this.CurrentLodLevel].Lod != null)
            {
                // Make the used lod visible
                this.Lods[this.CurrentLodLevel].Lod.SetActive(true);
                this.Lods[this.CurrentLodLevel].IsVisible = true;
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Hides all visible lod levels
        /// </summary>
        private void HideAllLods()
        {
            // Loop through all lods that are currently visible (Stored in the is visible flag)
            foreach (var lod in this.Lods.Where(lod => lod.IsVisible && lod.Lod != null))
            {
                // Hide the lod
                lod.Lod.SetActive(false);
                lod.IsVisible = false;
            }
        }

#if UNITY_EDITOR

        /// <summary>
        /// Is called when the object is instantiated or a gui value has changed
        /// </summary>
        private new void OnValidate()
        {
            if (!this.enabled)
            {
                return;
            }

            base.OnValidate();

            // Something else is changed, probably a lod setting (distance), let them all update
            if (
                this.Lods.Any(
                    lod => Mathf.Abs(lod.Distance - lod.PreviousDistance) > 0.001f || lod.Lod != lod.PreviousLod)
                || this.prevLodsLength != this.lods.Length)
            {
                foreach (var lod in this.Lods)
                {
                    lod.IsVisible = true;
                    lod.PreviousDistance = lod.Distance;
                    lod.PreviousLod = lod.Lod;
                }

                this.prevLodsLength = this.lods.Length;

                // Forces the update with distance recalculation
                if (LodManagerBase.Instance != null)
                {
                    this.CurrentLodLevel = -1;
                    this.SetNewRelativeDistance(this.RelativeDistance);
                }
                else
                {
                    this.ForceUpdateAllLods();
                }
            }
        }

#endif

        #endregion
    }
}