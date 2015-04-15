// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LodSource.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   A lod source, used for each camera
//   It manages the registering and deregistering of the lod sources
//   It also manages the calculation for the distance and the gizmos
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod
{
    using System;
    using System.Linq;

    using QuickLod.Attributes;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    using UnityEngine;

    /// <summary>
    /// A lod source, used for each camera<br/>
    /// It manages the registering and deregistering of the lod sources<br/>
    /// It also manages the calculation for the distance and the gizmos
    /// </summary>
    [AddComponentMenu("Quick Lod/Lod Source")]
    [ExecuteInEditMode]
    [LodHelper(@"Add this component to each camera in the scene that should be taken into account by QuickLod.

You need to set the ""Max update distance"" value, for the source to work properly.
The value should be bellow the maximum view distance of the camera and above the largest used lod distance in the scene.
This maximum view distance can also be affected by view blockers like walls.

To prevent the camera from using the distance multiplier, you can activate the ""Use lod multiplier"" option.

To focus the performance to the front of the camera, you can activate the ""Use view angle"" option.
You should define the view angle (probably the same as for the camera component)
Also you can choose between 3 falloff modes, to get more informations to them, use the tooltip of the value.
To hide cells at the back, you can activate the ""Ignore hidden cells"" option.

Use the ""Draw gizmos"" option to visualize the priority level ranges.")]
    public class LodSource : MonoBehaviour
    {
        #region private fields

        /// <summary>
        /// Backing field for <see cref="MaxUpdateDistance"/>
        /// </summary>
        [SerializeField]
        private float maxUpdateDistance;

        /// <summary>
        /// Used to determine if the lod source has been moved
        /// </summary>
        private Vector3 prevPosition;

        /// <summary>
        /// Use to determine if the lod source has been rotated
        /// </summary>
        private Quaternion prevRotation;

        /// <summary>
        /// Backing field for <see cref="UseGlobalDistanceMultiplier"/>
        /// </summary>
        [SerializeField]
        private bool useGlobalDistanceMultiplier;

        /// <summary>
        /// Backing field for <see cref="UseViewAngle"/>
        /// </summary>
        [SerializeField]
        private bool useViewAngle;

        /// <summary>
        /// Backing field for <see cref="Falloff"/>
        /// </summary>
        [SerializeField]
        private FalloffMode falloff;

        /// <summary>
        /// Backing field for <see cref="IgnoreHidden"/>
        /// </summary>
        [SerializeField]
        private bool ignoreHidden;

        /// <summary>
        /// Backing field for <see cref="AngleMargin"/>
        /// </summary>
        [SerializeField]
        private float angleMargin;

        /// <summary>
        /// Backing field for <see cref="BorderWidth"/>
        /// </summary>
        [SerializeField]
        private float borderWidth;

        /// <summary>
        /// Backing field for <see cref="GlobalDistanceMultiplier"/>
        /// </summary>
        private float globalDistanceMultiplier;

        /// <summary>
        /// Backing field for <see cref="LocalDistanceMultiplier"/>
        /// </summary>
        [SerializeField]
        private float localDistanceMultiplier;

        /// <summary>
        /// Stores the actual distance multiplier to use for calculations.
        /// </summary>
        private float distanceMultiplier;

        /// <summary>
        /// Backing field for <see cref="ObserveCamera"/>
        /// </summary>
        [SerializeField]
        private bool observeCamera;

        /// <summary>
        /// Backing field for <see cref="ObservedCameraComponent"/>
        /// </summary>
        [SerializeField]
        private Camera observedCameraComponent;

        /// <summary>
        /// Backing field for <see cref="DefaultCamerFoV"/>
        /// </summary>
        [SerializeField]
        private float defaultCamerFoV;

        /// <summary>
        /// Backing field for <see cref="UpdateAngleMarginFromCamera"/>
        /// </summary>
        [SerializeField]
        private bool updateAngleMarginFromCamera;

        /// <summary>
        /// Backing field for <see cref="UpdateLocalMultiplierWithFoV"/>
        /// </summary>
        [SerializeField]
        private bool updateLocalMultiplierWithFoV;

        /// <summary>
        /// Stores a reference to the transform component for faster access
        /// </summary>
        private Transform cachedTransform;

#if UNITY_EDITOR

        /// <summary>
        /// Stores the previous value of <see cref="useGlobalDistanceMultiplier"/>
        /// </summary>
        private bool prevUGDM;

        /// <summary>
        /// Stores the previous value of <see cref="useViewAngle"/>
        /// </summary>
        private bool prevUVA;

        /// <summary>
        /// Stores the previous value of <see cref="falloff"/>
        /// </summary>
        private FalloffMode prevF;

        /// <summary>
        /// Stores the previous value of <see cref="ignoreHidden"/>
        /// </summary>
        private bool prevIH;

        /// <summary>
        /// Stores the previous value of <see cref="maxUpdateDistance"/>
        /// </summary>
        private float prevMUD;

        /// <summary>
        /// Stores the previous value of <see cref="angleMargin"/>
        /// </summary>
        private float prevAM;

        /// <summary>
        /// Stores the previous value of <see cref="borderWidth"/>
        /// </summary>
        private float prevBW;

        /// <summary>
        /// Stores the previos value of <see cref="localDistanceMultiplier"/>
        /// </summary>
        private float prevLDM;

        /// <summary>
        /// Defines whether the gizmos should be drawn or not
        /// </summary>
        [SerializeField]
        private bool drawGizmos;

        /// <summary>
        /// Defines whether the gizmos should be drawn when deselected too
        /// </summary>
        [SerializeField]
        private bool drawWhenDeselected;

        [SerializeField]
        private bool drawInDebug;

        /// <summary>
        /// The sphere mesh
        /// </summary>
        [SerializeField]
        private Mesh sphereMesh;

        [SerializeField]
        private Material distanceMaterial;

#endif

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LodSource"/> class.
        /// </summary>
        public LodSource()
        {
            this.maxUpdateDistance = 500;
            this.useGlobalDistanceMultiplier = true;
            this.observeCamera = false;
            this.observedCameraComponent = null;
            this.updateAngleMarginFromCamera = false;
            this.updateLocalMultiplierWithFoV = false;
            this.defaultCamerFoV = 60;
            this.useViewAngle = false;
            this.falloff = FalloffMode.Smooth;
            this.angleMargin = 30;
            this.globalDistanceMultiplier = 1f;
            this.localDistanceMultiplier = 1f;
            this.distanceMultiplier = 1f;
            this.LevelDistances = new float[LodManagerBase.PriorityLevelAmount];

#if UNITY_EDITOR
            this.IsEditorSource = false; 
            this.drawGizmos = false;
            this.prevUVA = this.useViewAngle;
            this.prevUGDM = this.useGlobalDistanceMultiplier;
            this.prevMUD = this.maxUpdateDistance;
            this.prevAM = this.angleMargin;
#endif
        }

        #endregion

        #region enums

        /// <summary>
        /// The falloff mode to use
        /// </summary>
        public enum FalloffMode
        {
            /// <summary>
            /// Use an angle based hard falloff
            /// </summary>
            Hard,

            /// <summary>
            /// Use an angle based hard falloff with border
            /// </summary>
            HardWithBorder,

            /// <summary>
            /// Use an angle based smoothed falloff
            /// </summary>
            Smooth
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the distance for top priority cells
        /// </summary>
        public float[] LevelDistances { get; private set; }

        /// <summary>
        /// Gets or sets the global distance multiplier.<br/>
        /// </summary>
        /// <value>
        /// The global distance multiplier.
        /// </value>
        /// <remarks>
        /// This property is managed by the current lod manager.<br/>
        /// Changing the value manually will have no effect.<br/>
        /// Use the <see cref="LocalDistanceMultiplier"/> in order to apply a distance multiplier
        /// </remarks>
        public float GlobalDistanceMultiplier
        {
            get
            {
                return this.globalDistanceMultiplier;
            }

            set
            {
                // caches the multiplier directly so that it can be used with a multiplicator
                this.globalDistanceMultiplier = value;
                this.UpdateDistanceMultiplier();
            }
        }

        /// <summary>
        /// Gets or sets the local distance multiplier.
        /// </summary>
        /// <value>
        /// The local distance multiplier.
        /// </value>
        /// <remarks>
        /// The value only affects this lod source.<br/>
        /// In order to define global distance multiplier, use the <see cref="LodManagerBase.LodDistanceMultiplier"/>.
        /// </remarks>
        public float LocalDistanceMultiplier
        {
            get
            {
                return this.localDistanceMultiplier;
            }

            set
            {
                if (value < 0)
                {
                    return;
                }

                this.localDistanceMultiplier = value;
                this.UpdateDistanceMultiplier();

                if (LodManagerBase.Instance != null)
                {
                    LodManagerBase.Instance.HasSourceUpdates = true;
                }
            }
        }

        /// <summary>
        /// Gets the resulting distance multiplier that contains the local and global distance multiplier
        /// </summary>
        /// <value>
        /// The distance multiplier.
        /// </value>
        public float DistanceMultiplier
        {
            get
            {
                return this.useGlobalDistanceMultiplier
                           ? this.globalDistanceMultiplier * this.localDistanceMultiplier
                           : this.localDistanceMultiplier;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the lod source should use the global distance multiplier
        /// </summary> 
        public bool UseGlobalDistanceMultiplier
        {
            get
            {
                return this.useGlobalDistanceMultiplier;
            }

            set
            {
                if (this.useGlobalDistanceMultiplier == value)
                {
                    return;
                }

                this.useGlobalDistanceMultiplier = value;
#if UNITY_EDITOR
                this.prevUGDM = this.useGlobalDistanceMultiplier;
#endif

                this.UpdateDistanceMultiplier();

                if (LodManagerBase.Instance != null)
                {
                    LodManagerBase.Instance.HasSourceUpdates = true;
                }
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether an available camera component should be observed or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if the camera should be observed; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Set this property to true, if you want to use the following functions:<br/>
        /// <list type="bullet">
        ///     <item><description><see cref="UpdateAngleMarginFromCamera"/></description></item>
        ///     <item><description><see cref="UpdateLocalMultiplierWithFoV"/></description></item>
        /// </list><br/><br/>
        /// In order to observe the camera component, it must be placed on the same game object as the <see cref="LodSource"/>.
        /// </remarks>
        public bool ObserveCamera
        {
            get
            {
                return this.observeCamera;
            }

            set
            {
                this.observeCamera = value;
            }
        }

        /// <summary>
        /// Gets the observed camera component.<br/>
        /// The property <see cref="ObserveCamera"/> must be set to true for a camera to be observed
        /// </summary>
        /// <value>
        /// The observed camera component, null if no camera component is observed.
        /// </value>
        public Camera ObservedCameraComponent
        {
            get
            {
                return this.observedCameraComponent;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="AngleMargin"/> should be taken from the observed camera.<br/>
        /// The property <see cref="ObserveCamera"/> must be set to true and a camera component must be available.
        /// </summary>
        /// <value>
        /// <c>true</c> if <see cref="AngleMargin"/> should be updated; otherwise, <c>false</c>.
        /// </value>
        public bool UpdateAngleMarginFromCamera
        {
            get
            {
                return this.updateAngleMarginFromCamera;
            }

            set
            {
                this.updateAngleMarginFromCamera = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="LocalDistanceMultiplier"/> should be adjusted depending on the field of view.<br/>
        /// Use the <see cref="DefaultCamerFoV"/> to define at which field of view the zoom factor is 1x.<br/>
        /// The property <see cref="ObserveCamera"/> must be set to true and a camera component must be available.
        /// </summary>
        /// <value>
        /// <c>true</c> if the <see cref="LocalDistanceMultiplier"/> should be adjusted; otherwise, <c>false</c>.
        /// </value>
        public bool UpdateLocalMultiplierWithFoV
        {
            get
            {
                return this.updateLocalMultiplierWithFoV;
            }

            set
            {
                this.updateLocalMultiplierWithFoV = value;
            }
        }

        /// <summary>
        /// Gets or sets the field at which the zoom factor is 1x
        /// </summary>
        /// <value>
        /// The camera field of view.<br/>
        /// Default: 60
        /// </value>
        public float DefaultCamerFoV
        {
            get
            {
                return this.defaultCamerFoV;
            }

            set
            {
                this.defaultCamerFoV = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum update distance.
        /// </summary>
        /// <remarks>
        /// This property 
        /// </remarks>
        public float MaxUpdateDistance
        {
            get
            {
                return this.maxUpdateDistance;
            }

            set
            {
                if (Mathf.Abs(this.maxUpdateDistance - value) < 0.001f)
                {
                    return;
                }

                this.maxUpdateDistance = value;

                if (this.maxUpdateDistance < 25)
                {
                    this.maxUpdateDistance = 25;
                }
#if UNITY_EDITOR
                this.prevMUD = this.maxUpdateDistance;
#endif

                this.RecalculateDistances();

                if (LodManagerBase.Instance != null)
                {
                    LodManagerBase.Instance.HasSourceUpdates = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the camera should use the view angle to prioritize cells in the front of it 
        /// </summary>
        public bool UseViewAngle
        {
            get
            {
                return this.useViewAngle;
            }

            set
            {
                if (this.useViewAngle == value)
                {
                    return;
                }

                this.useViewAngle = value;

#if UNITY_EDITOR
                this.prevUVA = this.useViewAngle;
#endif

                if (LodManagerBase.Instance != null)
                {
                    LodManagerBase.Instance.HasSourceUpdates = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the view angle should be smoothed or not
        /// </summary>
        public FalloffMode Falloff
        {
            get
            {
                return this.falloff;
            }

            set
            {
                if (this.falloff == value)
                {
                    return;
                }

                this.falloff = value;

#if UNITY_EDITOR
                this.prevF = this.falloff;
#endif

                if (LodManagerBase.Instance != null)
                {
                    LodManagerBase.Instance.HasSourceUpdates = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether cells behind the source should be ignored 
        /// </summary>
        public bool IgnoreHidden
        {
            get
            {
                return this.ignoreHidden;
            }

            set
            {
                if (this.ignoreHidden == value)
                {
                    return;
                }

                this.ignoreHidden = value;

#if UNITY_EDITOR
                this.prevIH = this.ignoreHidden;
#endif

                if (LodManagerBase.Instance != null)
                {
                    LodManagerBase.Instance.HasSourceUpdates = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the margin to use before the angle is taken into account
        /// </summary>
        public float AngleMargin
        {
            get
            {
                return this.angleMargin;
            }

            set
            {
                if (Math.Abs(this.angleMargin - value) < Mathf.Epsilon)
                {
                    return;
                }

                this.angleMargin = Mathf.Clamp(value, 0f, 180f);

#if UNITY_EDITOR
                this.prevAM = this.angleMargin;
#endif

                if (LodManagerBase.Instance != null)
                {
                    LodManagerBase.Instance.HasSourceUpdates = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the border width to use.<br/>
        /// Only applies when the <see cref="Falloff"/> is set to <see cref="FalloffMode.HardWithBorder"/>
        /// </summary>
        /// <value>
        /// The border width.
        /// </value>
        public float BorderWidth
        {
            get
            {
                return this.borderWidth;
            }

            set
            {
                if (Math.Abs(this.borderWidth - value) < Mathf.Epsilon)
                {
                    return;
                }

                this.borderWidth = Mathf.Max(this.borderWidth, 0);

#if UNITY_EDITOR
                this.prevBW = this.borderWidth;
#endif

                if (LodManagerBase.Instance != null)
                {
                    LodManagerBase.Instance.HasSourceUpdates = true;
                }
            }
        }

        /// <summary>
        /// Gets a reference to the local transform.<br/>
        /// Consider using this property instead of <see cref="Component.transform"/> as the access is faster
        /// </summary>
        /// <value>
        /// The transform.
        /// </value>
        public Transform Transform
        {
            get
            {
                if (this.cachedTransform == null)
                {
                    this.cachedTransform = this.transform;
                }

                return this.cachedTransform;
            }
        }

#if UNITY_EDITOR

        /// <summary>
        /// Gets or sets a value indicating whether this instance is used for the editor viewport<br/>
        /// EDITOR ONLY: This property can only be accessed in the Unity editor
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is an editor source; otherwise, <c>false</c>.
        /// </value>
        public bool IsEditorSource { get; set; }

#endif

        #endregion

        #region public methods

        /// <summary>
        /// Calculates the maximum lod distance used in the scene
        /// </summary>
        /// <returns>
        /// Returns the largest found distance
        /// </returns>
        public static float CalculateMaxDistance()
        {
            var lodObjects = (LodObjectBase[])FindObjectsOfType(typeof(LodObjectBase));
            var maxDistance = lodObjects.Select(lodObject => lodObject.GetLargestLodDistance()).Concat(new[] { 0f }).Max();
            var border = LodManagerBase.Instance != null ? LodManagerBase.Instance.OptimalSourceBorder : 0;
            return Mathf.CeilToInt(maxDistance + border);
        }

        /// <summary>
        /// Calculates an optimal value for the border width.<br/>
        /// Uses the mesh data of all lod objects as well as the cell size of the <see cref="LodManagerCubic"/> if available
        /// </summary>
        /// <returns>
        /// Returns the found value
        /// </returns>
        public static float CalculateOptimalBorderWidth()
        {
            var result = 0f;

            var lodObjects = FindObjectsOfType(typeof(LodObjectBase)) as LodObjectBase[];
            if (lodObjects != null)
            {
                result =
                    lodObjects.Select(lodObject => lodObject.gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter)
                              .Where(meshFilter => meshFilter != null && meshFilter.sharedMesh != null)
                              .Select(meshFilter => meshFilter.sharedMesh.bounds.size.magnitude)
                              .Concat(new[] { result })
                              .Max();
            }

            if (LodManagerBase.Instance != null)
            {
                var optimalBorder = LodManagerBase.Instance.OptimalSourceBorder;
                result = Mathf.Max(result, optimalBorder);
            }

            return result;
        }

        /// <summary>
        /// Calculates the distance to a lod object
        /// </summary>
        /// <param name="position">
        /// The position of the lod object
        /// </param>
        /// <param name="realDistance">
        /// The real Distance.
        /// </param>
        /// <returns>
        /// Returns the distance to the lod object
        /// </returns>
        public float GetDistanceToObject(Vector3 position, out float realDistance)
        {
            realDistance = Vector3.Distance(this.Transform.position, position);
            var relativeDistance = realDistance * this.distanceMultiplier;
            
            if (!this.ignoreHidden)
            {
                return relativeDistance;
            }

            // When the view angle is taken into account, extend the calculation
            if (this.useViewAngle && this.ignoreHidden && relativeDistance < this.maxUpdateDistance + this.borderWidth)
            {
                // Calculate the direction to the cell
                var direction = position - this.Transform.position;

                // Calculate the angle between the direction and the forward vector of the camera
                var angle = 2 * Vector3.Angle(direction, this.Transform.forward);

                // If the angle is outside the margin part and the cell is not near enough
                if (angle > this.angleMargin && realDistance > this.borderWidth)
                {
                    switch (this.falloff)
                    {
                        case FalloffMode.Hard:
                            {
                                // Modify the distance based on the angle and apply a hard falloff
                                return this.ignoreHidden ? float.PositiveInfinity : this.maxUpdateDistance - 1;
                            }

                        case FalloffMode.HardWithBorder:
                            {
                                // Special case when the angle is > angleMargin + 90°
                                if (angle > this.angleMargin + 180)
                                {
                                    return float.PositiveInfinity;
                                }

                                // Calculates the distance to the view border
                                var distToBorder = realDistance * Mathf.Sin((angle - this.angleMargin) * Mathf.Deg2Rad / 2f);

                                // Checks if the object is outside the border
                                if (distToBorder > this.borderWidth)
                                {
                                    return float.PositiveInfinity;
                                }

                                break;
                            }

                        case FalloffMode.Smooth:
                            {
                                // Modify the distance based on the angle and apply a smoothed out falloff
                                relativeDistance = relativeDistance * (this.angleMargin - 360f) / (angle - 360f);

                                if (!this.IgnoreHidden)
                                {
                                    return Mathf.Min(relativeDistance, this.maxUpdateDistance);
                                }

                                break;
                            }
                    }
                }
            }

            return relativeDistance;
        }

        /// <summary>
        /// Calculates the distance to a cell
        /// </summary>
        /// <param name="position">The center position of the cell</param>
        /// <param name="halfCellDiagonale">The diagonal length of a cell divided by two</param>
        /// <returns>Returns the distance to the cell</returns> 
        public float GetDistanceToCell(Vector3 position)
        {
            var realDistance = Vector3.Distance(this.Transform.position, position);
            var relativeDistance = realDistance * this.distanceMultiplier;

            // When the view angle is taken into account, extend the calculation
            if (this.useViewAngle && relativeDistance < this.maxUpdateDistance)
            {
                // Calculate the direction to the cell
                var direction = position - this.Transform.position;
                
                // Calculate the angle between the direction and the forward vector of the camera
                var angle = 2 * Vector3.Angle(direction, this.Transform.forward);
                var nearRadius = this.falloff == FalloffMode.HardWithBorder ? this.borderWidth : 0;
                
                // If the angle is outside the margin part and the cell is not near enough
                if (angle > this.angleMargin && realDistance > nearRadius)
                {
                    switch (this.falloff)
                    {
                        case FalloffMode.Hard:
                            {
                                // Modify the distance based on the angle and apply a hard falloff
                                return this.ignoreHidden ? float.PositiveInfinity : this.maxUpdateDistance - 1;
                            }

                        case FalloffMode.HardWithBorder:
                            {
                                // Special case when the angle is > angleMargin + 90°
                                if (angle > this.angleMargin + 180)
                                {
                                    return this.ignoreHidden ? float.PositiveInfinity : this.maxUpdateDistance - 1;
                                }

                                // Calculates the distance to the view border
                                var distToBorder = realDistance * Mathf.Sin((angle - this.angleMargin) * Mathf.Deg2Rad / 2f);

                                // Checks if the cell is outside the border
                                if (distToBorder > this.borderWidth)
                                {
                                    return this.ignoreHidden ? float.PositiveInfinity : this.maxUpdateDistance - 1;
                                }

                                break; 
                            }

                        case FalloffMode.Smooth:
                            {
                                // Modify the distance based on the angle and apply a smoothed out falloff
                                relativeDistance = relativeDistance * (this.angleMargin - 360f) / (angle - 360);
                                
                                if (!this.IgnoreHidden)
                                {
                                    return Mathf.Min(relativeDistance, this.maxUpdateDistance);
                                }

                                break;
                            }
                    }
                }
            }

            return relativeDistance;
        }

        /// <summary>
        /// Finds the priority level for the given distance
        /// </summary>
        /// <param name="distance">The distance.</param>
        /// <returns>Returns the priority level</returns>
        public int GetDistanceLevel(float distance)
        {
            for (var i = 0; i < this.LevelDistances.Length; i++)
            {
                if (distance < this.LevelDistances[i])
                {
                    return i;
                }
            }

            return LevelDistances.Length;
        }

        /// <summary>
        /// Gets the distance to chunk.<br/>
        /// OBSOLETE: Use the method <see cref="GetDistanceToCell(UnityEngine.Vector3)"/> instead.
        /// </summary>
        /// <param name="position">The center position of the cell</param>
        /// <param name="halfCellDiagonale">The diagonal length of a cell divided by two</param>
        /// <returns>Returns the distance to the cell</returns> 
        [Obsolete("This method is now obsolete because the chunk class has been renamed to cell.\nUse the method GetDistanceToCell(Vector3) instead.")]
        public float GetDistanceToChunk(Vector3 position, float halfCellDiagonale)
        {
            return this.GetDistanceToCell(position);
        }

        /// <summary>
        /// Gets the distance to cell.<br/>
        /// OBSOLETE: Use the method <see cref="GetDistanceToCell(UnityEngine.Vector3)"/> instead.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="halfCellSize">The half size of the cell.</param>
        /// <returns></returns>
        [Obsolete("This method is now obsolete because the half cell size is now replaced with LodManagerBase.OptimalSourceBorder:float.\nUse the method GetDistanceToCell(Vector3) instead.")]
        public float GetDistanceToCell(Vector3 position, float halfCellSize)
        {
            return this.GetDistanceToCell(position);
        }
        
        #endregion
         
        #region private methods

        /// <summary>
        /// Gets called in each frame when the logic gets updated
        /// </summary>
        private void Update()
        {
            // Check if the position has been changed and set the has source updates flag when necessary
            if (LodManagerBase.Instance != null)
            {
                // Let the LodManager check if there's a need for an update
                LodManagerBase.Instance.HandleSourceMovement(this, this.prevPosition);
                this.prevPosition = this.Transform.position;

                // When the camera uses the view angle and has rotatet mor than 2°
                if (this.useViewAngle && Quaternion.Dot(this.prevRotation, this.Transform.rotation) < 0.99985f)
                {
                    LodManagerBase.Instance.HasSourceUpdates = true;
                    this.prevRotation = this.Transform.rotation;
                }
            }

            // When the camera should be observed, perform camera releated tasks
            if (this.ObserveCamera)
            {
                // If no camera component has been cached, search for one
                if (this.observedCameraComponent == null)
                {
                    this.observedCameraComponent = this.gameObject.GetComponent(typeof(Camera)) as Camera;
                }

                // If a camera component has been found, observe it
                if (this.observedCameraComponent != null)
                {
                    // Update the AngleMargin property with the camera FoV
                    if (this.updateAngleMarginFromCamera)
                    {
                        var radAngle = this.observedCameraComponent.fieldOfView * Mathf.Deg2Rad;
                        var radHfov = 2 * Mathf.Atan(Mathf.Tan(radAngle / 2) * this.observedCameraComponent.aspect);
                        var horFoV = Mathf.Rad2Deg * radHfov;

                        // Get the larger fov (horizontal vs. vertical)
                        this.angleMargin = Mathf.Max(
                            this.observedCameraComponent.fieldOfView, horFoV);
                    }

                    // Update the local distance multiplier depending on the FoV
                    if (this.updateLocalMultiplierWithFoV)
                    {
                        this.LocalDistanceMultiplier = this.defaultCamerFoV / this.ObservedCameraComponent.fieldOfView;
                    }
                }
            }

#if UNITY_EDITOR

            if (this.drawGizmos && (this.drawWhenDeselected || Selection.Contains(this.gameObject))
                && (!Application.isPlaying || drawInDebug) && this.enabled)
            {
                this.DrawEditorGraphics();
            }

#endif
        }
        
        /// <summary>
        /// Gets called when the objects gets disabled or destroyed 
        /// </summary>
        private void OnDisable()
        {
            // Deregisters the source
            if (LodManagerBase.Instance != null)
            {
                LodManagerBase.Instance.DeregisterSource(this);
            }
        }

        /// <summary>
        /// Gets called when the object gets instantiated or enabled 
        /// </summary>
        private void OnEnable()
        {
            // Registers the source
            if (LodManagerBase.Instance != null)
            {
                LodManagerBase.Instance.RegisterSource(this);
            }

            this.LevelDistances = new float[LodManagerBase.PriorityLevelAmount];

            // Recalculate all priority distances
            this.RecalculateDistances();
        }

        /// <summary>
        /// Recalculates the priority distances depending on the max update distance
        /// </summary>
        private void RecalculateDistances()
        { 
            var partDistance = this.maxUpdateDistance / this.LevelDistances.Length;

            // Each priority distance covers 1/4 of the max update distance
            for (var i = 0; i < this.LevelDistances.Length; i++)
            {
                this.LevelDistances[i] = (i + 1) * partDistance;
            }
        }

        /// <summary>
        /// Updates the distance multiplier.
        /// </summary>
        private void UpdateDistanceMultiplier()
        {
            if (this.useGlobalDistanceMultiplier)
            {
                this.distanceMultiplier = 1f / (this.globalDistanceMultiplier * this.localDistanceMultiplier);
            }
            else
            {
                this.distanceMultiplier = 1f / this.localDistanceMultiplier;
            }
        }
        
#if UNITY_EDITOR

        /// <summary>
        /// Access this method only in the editor!
        /// Gets called when the gizmos is active and the object is selected
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (!this.drawWhenDeselected)
            {
                this.DrawGizmos();
            }
        }

        private void OnDrawGizmos()
        {
            if (this.drawWhenDeselected)
            {
                this.DrawGizmos();
            }
        }

        private void DrawGizmos()
        {
            if (!this.drawGizmos || (sphereMesh != null && this.distanceMaterial != null))
            {
                return;
            }

            // The multiplier for the gizmos
            var multiplier = this.DistanceMultiplier;

            for (var i = 0; i < this.LevelDistances.Length; i++)
            {
                var relativePos = (float)(i) / (this.LevelDistances.Length - 1);
                Gizmos.color = new Color(Mathf.Min(2 * relativePos, 1), Mathf.Min(2 * (1 - relativePos), 1), 0, 0.3f);

                Gizmos.DrawWireSphere(this.Transform.position, this.LevelDistances[i] * multiplier);
            }
        }

        private void DrawEditorGraphics()
        {
            if (this.sphereMesh == null || this.distanceMaterial == null)
            {
                return;
            }

            for (var i = 0; i < this.LevelDistances.Length; i++)
            {
                var relativePos = (float)(i) / (this.LevelDistances.Length - 1);
                var color = new Color(Mathf.Min(2 * relativePos, 1), Mathf.Min(2 * (1 - relativePos), 1), 0, 0.3f);

                var dist = this.LevelDistances[i] * this.DistanceMultiplier;
                var matrix = Matrix4x4.TRS(
                    this.Transform.position, Quaternion.identity, new Vector3(dist, dist, dist));

                var matBlock = new MaterialPropertyBlock();
                matBlock.SetColor("_Color", color);

                Graphics.DrawMesh(sphereMesh, matrix, distanceMaterial, 0, null, 0, matBlock);
            }
        }

        /// <summary>
        /// Gets called when the object gets instantiated or the gui has changed a value
        /// </summary>
        private void OnValidate()
        {
            var hasSourceUpdate = false;

            // Check if the max update distance has been changed
            if (Mathf.Abs(this.maxUpdateDistance - this.prevMUD) > Mathf.Epsilon)
            {
                // Restrict the value range
                if (this.maxUpdateDistance < 5)
                {
                    this.maxUpdateDistance = 5;
                }

                this.prevMUD = this.maxUpdateDistance;
                this.RecalculateDistances();

                // Set the has source updates flag
                hasSourceUpdate = true;
            }

            // Check if the use view angle has changed
            if (this.useViewAngle != this.prevUVA)
            {
                this.prevUVA = this.useViewAngle;

                hasSourceUpdate = true;
            }

            // Check if the smooth falloff has changed
            if (this.falloff != this.prevF)
            {
                this.prevF = this.falloff;

                hasSourceUpdate = true;
            }

            // Checks if the ignore hidden cells has changed
            if (this.ignoreHidden != this.prevIH)
            {
                this.prevIH = this.ignoreHidden;

                hasSourceUpdate = true;
            }

            // Check if the angle margin has changed
            if (Math.Abs(this.angleMargin - this.prevAM) > Mathf.Epsilon)
            {
                this.angleMargin = Mathf.Clamp(this.angleMargin, 0f, 180f);
                this.prevAM = this.angleMargin;

                hasSourceUpdate = true;
            }

            // Check if the border margin has changed
            if (Math.Abs(this.borderWidth - this.prevBW) > Mathf.Epsilon)
            {
                this.borderWidth = Mathf.Max(this.borderWidth, 0);
                this.prevBW = this.borderWidth;

                hasSourceUpdate = true;
            }

            // Check if the use lod multiplier has been changed
            if (this.useGlobalDistanceMultiplier != this.prevUGDM)
            {
                this.prevUGDM = this.useGlobalDistanceMultiplier;
                this.UpdateDistanceMultiplier();
                
                hasSourceUpdate = true;
            }
            
            if (Math.Abs(this.localDistanceMultiplier - this.prevLDM) > Mathf.Epsilon)
            {
                if (this.localDistanceMultiplier < 0)
                {
                    this.localDistanceMultiplier = 0;
                }

                this.prevLDM = this.localDistanceMultiplier;
                this.UpdateDistanceMultiplier();

                hasSourceUpdate = true;
            }

            if (hasSourceUpdate && LodManagerBase.Instance != null)
            {
                LodManagerBase.Instance.HasSourceUpdates = true;
            }
        }
#endif

        #endregion
    }
}