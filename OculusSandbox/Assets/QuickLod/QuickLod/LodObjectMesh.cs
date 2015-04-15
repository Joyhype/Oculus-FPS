// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LodObjectMesh.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The lod object mesh
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using QuickLod.Attributes;
    using QuickLod.Containers;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    using UnityEngine;

    using Object = UnityEngine.Object;

    /// <summary>
    /// A lod object that replaces the mesh in the mesh filter.<br/>
    /// It's very performant as it hasn't memory overhead.<br/>
    /// Requirement: MeshFilter
    /// </summary>
    [AddComponentMenu("Quick Lod/Lod Object Mesh")]
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteInEditMode]
    [LodHelper(@"This component manages the mesh in the mesh filter depending on the distance.
It can be used if the object needs different meshes for different distances.

You need to define the lod levels in the ""Lods"" field.
To automaticly setup the meshes, you can use the ""Generate all"" function.
To optimize the distance based on the mesh size, you can use the ""Optimize distances"" option.

When the object does not move, you should activate the ""Is static"" option.
When you want to set the current lod level manualy, you must activate the ""Exclude from manager"" option.

The relative distance is the distance to the nearest lod source including the lod multiplier.
The absolute distance is the real distance to the nearest lod source.")]
    public class LodObjectMesh : LodObjectBase
    {
        #region Fields

        /// <summary>
        /// Backing field for <see cref="Lods"/>
        /// </summary>
        [SerializeField]
        private LodStructureMesh[] lods;

        /// <summary>
        /// Stores whether the mesh renderers were active or not
        /// </summary>
        private bool isMeshrendererActive;

        /// <summary>
        /// Caches the mesh filter for direct access
        /// </summary>
        private MeshFilter meshFilter;

#if UNITY_EDITOR

        /// <summary>
        /// Stores the length of the previous lods collection
        /// </summary>
        private int prevLodsLength;

#endif

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LodObjectMesh"/> class.
        /// </summary>
        public LodObjectMesh()
        {
            this.lods = new LodStructureMesh[3];
            this.lods[0] = new LodStructureMesh();
            this.lods[1] = new LodStructureMesh();
            this.lods[2] = new LodStructureMesh();

            this.CanCalculateDistances = true;
            this.CanGenerateLodLevels = true;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the lods.
        /// </summary>
        public LodStructureMesh[] Lods
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

                foreach (LodStructureMesh lod in this.Lods.Where(lod => lod != null))
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
                if (LodManagerBase.Instance != null)
                {
                    this.SetNewRelativeDistance(this.RelativeDistance);
                }
                this.ForceUpdateAllLods();
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Tries to automatically assign the lod level objects with the child game objects
        /// </summary>
        public override void AutoAssignLodLevels()
        {
#if !UNITY_EDITOR
            Debug.LogWarning("The lod object mesh component only supports automatic lod generation in the editor.");
#else
            Mesh baseMesh;

            var meshFilter = this.gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                // Get the mesh from the mesh filter
                baseMesh = meshFilter.sharedMesh;
            }
            else if (Lods.Length > 0 && Lods[0].Lod != null)
            {
                // If not possible, then get it from the first lod level entry
                baseMesh = Lods[0].Lod;
            }
            else
            {
                // If not possible, throw an error
                const string ErrorLog = "You need to reference a mesh for this to work.\nEither define a mesh in the mesh renderer or in the first entry of the lod list.";
                Debug.LogWarning(ErrorLog);
                return;
            }

            var meshes = GetAllRelevantMeshes(baseMesh);

            if (meshes == null)
            {
                this.lods = new[] { new LodStructureMesh { Lod = baseMesh } };
                this.RecalculateDistances();

                return;
            }

            var numberedMeshes =
                (meshes.Select(mesh => new { mesh, value = GetNumberAtEndOfString(mesh.name) })
                       .Where(@t => @t.value != int.MinValue)
                       .Select(@t => new KeyValuePair<Mesh, int>(@t.mesh, @t.value))).ToList();

            var orderedMeshes = numberedMeshes.OrderBy(x => x.Value).ThenByDescending(x => x.Key.vertexCount).ToArray();

            this.lods = new LodStructureMesh[orderedMeshes.Length];
            for (int index = 0; index < orderedMeshes.Length; index++)
            {
                this.lods[index] = new LodStructureMesh();
                this.lods[index].Lod = orderedMeshes[index].Key;
            }

            this.RecalculateDistances();
            this.ForceUpdateAllLods();
#endif
        }

        /// <summary>
        /// Gets the amount of lod levels that are setup for this lod level
        /// </summary>
        /// <returns>
        /// Returns the amount
        /// </returns>
        public override int GetLodAmount()
        {
            return this.lods.Length;
        }

        /// <summary>
        ///     Force updates all lod levels, usually called when the object gets initialized
        /// </summary>
        public override void ForceUpdateAllLods()
        {
            this.OnLodLevelChanged();
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
            var maxBounds = Vector3.Scale(this.Lods[0].Lod.bounds.size, this.transform.lossyScale);
            var distances = GetOptimalLevelDistances(maxBounds.magnitude, this.lods.Length);

            for (int index = 0; index < this.lods.Length; index++)
            {
                this.lods[index].Distance = distances[index];
            }
        }

        /// <summary>
        /// Set the new distance for this object that uses distance multipliers
        /// </summary>
        /// <param name="newDistance">The new relative Distance</param>
        /// <remarks>
        /// LodObjects should overwrite this method in order to get a notification when a new distance has been calculated.
        /// </remarks>
        /// <example>
        /// This example shows a general usage of this method
        /// <code>
        /// public override void SetNewRelativeDistance(float newDistance)
        /// {
        /// // Calls the base method first
        /// base.SetNewRelativeDistance(newDistance);
        /// // Other code here
        /// }
        /// </code><br />
        /// This example shows how to get the correct lod level from a standard lods collection.<br /><i>In this example the lods collection is called lods.</i><code>
        /// public override void SetNewRelativeDistance(float newDistance)
        /// {
        /// base.SetNewRelativeDistance(newDistance);
        /// for (var index = 0; index &lt; this.lods.Length; index++)
        /// {
        /// // Get the first lod where the distance is over the relative distance
        /// if (this.lods[index].Distance &gt; newDistance)
        /// {
        /// this.CurrentLodLevel = index;
        /// return;
        /// }
        /// }
        /// // If all lod distances are smaller than the relative distance, hide all lods
        /// this.CurrentLodLevel = -1;
        /// }
        /// </code></example> 
        public override void SetNewRelativeDistance(float newDistance)
        {
            base.SetNewRelativeDistance(newDistance);

            for (var index = 0; index < this.lods.Length; index++)
            {
                // Get the first lod where the distance is over the relative distance
                if (this.lods[index].Distance > newDistance)
                {
                    this.CurrentLodLevel = index;
                    return;
                }
            }

            // If all lod distances are smaller than the relative distance, hide all lods
            this.CurrentLodLevel = -1;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Is called when the object gets enabled or instantiated
        /// </summary>
        /// <remarks>
        /// If you override this method, you must call the base method.
        /// </remarks>
        protected override void OnEnable()
        {
            base.OnEnable();

            this.isMeshrendererActive = this.CurrentLodLevel >= 0 && this.CurrentLodLevel < this.lods.Length;
        }

        /// <summary>
        /// Makes all lods visible
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            if (this.Lods.Length > 0 && this.gameObject != null)
            {
                this.SetMeshFilter(this.Lods[0].Lod);
            }
        }

        /// <summary>
        ///     Updates the lod levels depending on the current lod level
        /// </summary>
        protected override void OnLodLevelChanged()
        {
            if (this.CurrentLodLevel < 0 || this.CurrentLodLevel >= this.Lods.Length)
            {
                this.SetMeshFilter(null);

                // Prevents overhead duo to renderering an non existant mesh
                if (this.isMeshrendererActive)
                {
                    var meshRenderer = this.gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
                    if (meshRenderer != null)
                    {
                        meshRenderer.enabled = false;
                        this.isMeshrendererActive = false;
                    }
                }
            }
            else
            {
                this.SetMeshFilter(this.Lods[this.CurrentLodLevel].Lod);

                if (!this.isMeshrendererActive)
                {
                    var meshRenderer = this.gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
                    if (meshRenderer != null)
                    {
                        meshRenderer.enabled = true;
                        this.isMeshrendererActive = true;
                    }
                }
            }
        }

        /// <summary>
        /// The set mesh filter.
        /// </summary>
        /// <param name="newMesh">
        /// The new mesh.
        /// </param>
        private void SetMeshFilter(Mesh newMesh)
        {
            if (this.meshFilter == null)
            {
                this.meshFilter = this.GetComponent(typeof(MeshFilter)) as MeshFilter;
            }

            this.meshFilter.sharedMesh = newMesh;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Gets called when the object is instantiated or a value has changed in the gui<br />
        /// EDITOR ONLY: This method can only be accessed in the Unity editor
        /// </summary>
        private new void OnValidate()
        {
            if (!this.enabled)
            {
                return;
            }

            base.OnValidate();

            // Something else is changed, probably a lod setting (distance), let them all update
            if (this.Lods.Any(lod => Mathf.Abs(lod.Distance - lod.PreviousDistance) > 0.001f || lod.Lod != lod.PreviousLod) || this.prevLodsLength != lods.Length)
            {
                foreach (var lod in Lods)
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

        /// <summary>
        /// Gets all relevant meshes.
        /// </summary>
        /// <param name="baseMesh">The base mesh.</param>
        /// <returns></returns>
        private static IEnumerable<Mesh> GetAllRelevantMeshes(Mesh baseMesh)
        {
            var path = AssetDatabase.GetAssetPath(baseMesh).Trim(new[] { '/' });

            if (!path.StartsWith("Assets/"))
            {
                return null;
            }
            
            var modelMeshes = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Mesh>().ToArray();

            var number = GetNumberAtEndOfString(baseMesh.name);
            if (number == int.MinValue)
            {
                return null;
            }

            var numberString = number.ToString(CultureInfo.InvariantCulture);
            var usedName = baseMesh.name.Substring(0, baseMesh.name.Length - numberString.Length);

            // When the model contains multiple meshes with the correct name, take only these
            var namedModelMeshes =
                modelMeshes.Where(
                    mesh =>
                    mesh.name
                    == usedName + GetNumberAtEndOfString(mesh.name).ToString(CultureInfo.InvariantCulture))
                           .ToArray();

            if (namedModelMeshes.Length >= 2)
            {
                return namedModelMeshes;
            }

            // When the model only contains one mesh, then get all meshes from the folder where the model contains only one mesh

            // Remove the Assets/ part as it is already in the Application.dataPath -> Used for Directory.GetFiles
            path = Path.GetDirectoryName(path.Replace("Assets/", string.Empty));

            var files =
                Directory.GetFiles(Application.dataPath + "/" + path).Where(file => !file.EndsWith("meta")).ToArray();
            var pathMeshes = new List<Mesh>();

            foreach (var file in files)
            {
                // Get the relative path back
                var assetPath = "Assets" + file.Replace(Application.dataPath, string.Empty).Replace('\\', '/');
                var localMeshes = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Mesh>().ToArray();

                var cleanedLocalMeshes =
                    localMeshes.Where(
                        mesh =>
                        mesh.name
                        == usedName + GetNumberAtEndOfString(mesh.name).ToString(CultureInfo.InvariantCulture));

                if (cleanedLocalMeshes.Count() == 1)
                {
                    pathMeshes.AddRange(cleanedLocalMeshes);
                }
            }

            return pathMeshes.ToArray();
        }

#endif

        #endregion
    }
}