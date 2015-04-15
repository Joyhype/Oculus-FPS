// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LodManagerBase.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The base class for all lod managers.
//   Manages the registering and unregistering of lod objects and sources.
//   Singleton: Can be instantiated only once and can be directly accessed by the [Instance] property.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    using UnityEngine;

    using Object = UnityEngine.Object;

    /// <summary>
    /// The base class for all lod managers.<br/>
    /// Manages the registering and unregistering of lod objects and sources.<br/>
    /// Singleton: Can be instantiated only once and can be directly accessed by the <see cref="Instance"/> property.
    /// </summary>
    /// <remarks>
    /// When you want to create your own lod manager, you need to inherit from this base class.<br/><br/>
    /// <b>Methods</b><br/>
    /// You must implement the following methods to cover the basic needs:<br/>
    /// <list type="bullet">
    ///     <item>
    ///         <term><see cref="RegisterObject"/></term>
    ///         <description>Register a lod object to your lod manager</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="DeregisterObject"/></term>
    ///         <description>Deregister a lod object from your lod manager</description>
    ///     </item>     
    /// </list><br/>
    /// In order to support more functionality, you can implement the following methods:<br/>
    /// <list type="bullet">
    ///     <item>
    ///         <term><see cref="UpdateObjectPosition"/></term>
    ///         <description>Notifies that the overloaded object has moved around</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="DrawLodObjectGizmos"/></term>
    ///         <description>Is called by a lod object when it wants the lod manager to draw gizmos for it (Like the cell it is in)</description>
    ///     </item>  
    /// </list><br/>
    /// <b>Workflow</b><br/>
    /// In order to work in the editor too, you have to override the method <see cref="NextStep"/> as this is a reliable update notification.<br/>
    /// You have to register all lod objects in the method <see cref="RegisterObject"/>.<br/>
    /// You have to deregister all lod object in the method <see cref="DeregisterObject"/>.<br/><br/>
    /// To get all lod sources, you can use the property <see cref="Sources"/>.<br/>
    /// To get if a lod source is a viewport source (editor view only) you can look into the <see cref="ViewportSources"/>.<br/><br/>
    /// The actual distance calculation happens in the lod source.<br/>
    /// Use the method <see cref="LodSource.GetDistanceToPosition"/> in order to get the relative distance.<br/>
    /// The parameter "realDistance" from <see cref="LodSource.GetDistanceToPosition"/> returns the real distance.<br/>
    /// </remarks>
    [ExecuteInEditMode]
    public abstract class LodManagerBase : MonoBehaviour
    {
        #region constants

        /// <summary>
        /// The amount of cell priority levels used by QuickLod
        /// </summary>
        /// <value>
        /// Default: 4, Minimum 0
        /// </value>
        /// <remarks>
        /// This constant value is a fast way to change the amount of priority levels used in QuickLod.<br/>
        /// Do not set any negative numbers, as that would cause errors!
        /// </remarks>
        public const int PriorityLevelAmount = 4;

        /// <summary>
        /// The interval (in frames) at which the registered objects receive a position update call
        /// </summary>
        private const int PositionUpdateInterval = 60;

        #endregion

        #region private fields

        /// <summary>
        /// Backing field for <see cref="PauseCalculations"/>
        /// </summary>
        [SerializeField]
        private bool pauseCalculations;

        /// <summary>
        /// Backing field for <see cref="HasSourceUpdates"/>
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private bool hasSourceUpdates;

        /// <summary>
        /// Backing field for <see cref="UseOtherLodSystem"/>
        /// </summary>
        [SerializeField]
        private bool useOtherLodSystem;

        /// <summary>
        /// Backing field for <see cref="ActivateLightsOnSwitchToQuickLod"/>
        /// </summary>
        [SerializeField]
        private bool activateLightsOnSwitchToQuickLod;

        /// <summary>
        /// Backing field for <see cref="UpdatesPerFrame"/>
        /// </summary>
        [SerializeField]
        private int updatesPerFrame;

        /// <summary>
        /// The position update callback counter
        /// </summary>
        private int positionUpdateCallbackCounter;

        /// <summary>
        /// Backing field for <see cref="LodDistanceMultiplier"/>
        /// </summary>
        [SerializeField]
        private float lodDistanceMultiplier;

        /// <summary>
        /// The viewport objects.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private GameObject[] viewportObjects;

        /// <summary>
        /// The viewport sources.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private LodSource[] viewportSources;
        
        /// <summary>
        /// Backing field for <see cref="ActiveLodSystem"/>
        /// </summary>
        [SerializeField]
        private LodSystemSelection activeLodSystem;

        /// <summary>
        /// Contains all lod objects which want a position update callback
        /// </summary>
        private HashSet<LodObjectBase> positionUpdateCallbackReceivers;

        /// <summary>
        /// Stores a reference to the transform component for faster access
        /// </summary>  
        private Transform cachedTransform;

#if UNITY_EDITOR

        /// <summary>
        /// Stores the previous value of <see cref="lodDistanceMultiplier"/><br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private float prevLDM;

        /// <summary>
        /// Stores the previous value of use <see cref="useSceneCameraInEditMode"/><br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private bool prevUSCIEM;

        /// <summary>
        /// Stores the previous value of <see cref="useSourcesInEditMode"/><br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private bool prevUSIEM;

        /// <summary>
        /// Stores the previous value of <see cref="updatesPerFrame"/><br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private int prevUPF;

        /// <summary>
        /// Stores the previous value of <see cref="viewportDistanceMultiplier"/><br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private float prevVDM;

        /// <summary>
        /// Stores the previous value of <see cref="viewportMaxUpdateDistance"/><br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private float prevVMUD;

        /// <summary>
        /// Stores the previous value of <see cref="activeLodSystem"/><br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private LodSystemSelection prevALS;

        /// <summary>
        /// The viewport max update distance.<br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        private float viewportMaxUpdateDistance;

        /// <summary>
        /// The use scene camera in edit mode.<br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        private bool useSceneCameraInEditMode;

        /// <summary>
        /// The use sources in edit mode.<br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        private bool useSourcesInEditMode;

        /// <summary>
        /// The viewport distance multiplier.<br/>
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        [SerializeField]
        private float viewportDistanceMultiplier;
        
#endif

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LodManagerBase"/> class.
        /// </summary>
        public LodManagerBase()
        {
            this.lodDistanceMultiplier = 1f;
            this.Sources = new List<LodSource>();
            this.activeLodSystem = LodSystemSelection.QuickLod;
            this.activateLightsOnSwitchToQuickLod = true;
            this.positionUpdateCallbackCounter = 0;
            this.positionUpdateCallbackReceivers = new HashSet<LodObjectBase>();

#if UNITY_EDITOR
            this.useSourcesInEditMode = false;
            this.useSceneCameraInEditMode = true;
            this.viewportDistanceMultiplier = 1f;
            this.updatesPerFrame = 500;
            this.prevVDM = this.viewportDistanceMultiplier;
            this.prevUSCIEM = this.useSceneCameraInEditMode;
            this.prevUSIEM = this.useSourcesInEditMode;
            this.prevLDM = this.lodDistanceMultiplier;
            this.prevALS = this.activeLodSystem;
            this.viewportMaxUpdateDistance = 500;
#endif
        }

        #endregion 

        #region public enums

        /// <summary>
        /// OBSOLETE: The <see cref="Enum"/> is used for a functionality that is no longer supported.
        /// </summary>
        /// <remarks> 
        /// The <see cref="Enum"/> was originally used by the <see cref="LodObjectReplacement"/> in order to define whether the mesh or the object should be replaced.<br/> 
        /// As the <see cref="LodObjectMesh"/> now offers the functionality of mesh replacement, this functionality is obsolete.<br/>
        /// This <see cref="Enum"/> only exists for backwards compatibility and is no longer functional!
        /// </remarks>
        [Obsolete("The enum is used for a functionality that is no longer supported.")]
        public enum CullMode
        {
            /// <summary>
            /// Hide/show the object by switching the mesh renderer
            /// </summary>
            MeshRenderer, 

            /// <summary>
            /// Hide/show the object by switching the whole object
            /// </summary>
            Object
        }

        /// <summary>
        /// Contains all supported lod systems to choose from.
        /// </summary>
        /// <remarks>
        /// This <see cref="Enum"/> represents the current list of supported systems.<br/>
        /// All systems listed in this <see cref="Enum"/> can be managed by QuickLod.<br/>
        /// For more information, see the <a href="http://forum.unity3d.com/threads/quicklod-fast-asynchronous-lod-system.237196/">forum</a>
        /// or the <a href="http://tirelessart.ch/en/quickcode/quicklod/">website</a>
        /// </remarks>
        public enum LodSystemSelection
        {
            /// <summary>
            /// This system :)
            /// </summary>
            QuickLod, 

            /// <summary>
            /// The <a href="http://forum.unity3d.com/threads/instantoc-dynamic-occlusion-culling-lod.166748/">InstantOC</a> system
            /// </summary>
            InstantOc
        }

        #endregion

        #region static public properties

        /// <summary>
        /// Gets the single instance of <see cref="LodManagerBase"/>
        /// </summary>
        /// <remarks>
        /// This property stores the singleton instance of the currently used lod manager.<br/>
        /// It can be used to directly access the instance instead of searching for it.
        /// </remarks>
        public static LodManagerBase Instance { get; private set; }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets a value indicating whether the calculations should be paused or not
        /// </summary>
        /// <remarks>
        /// When this property is set to true, most functionality of this component will be paused.<br/>
        /// The registering, deregistering and moving of objects still works.<br/><br/>
        /// Consider using this flag instead of disabling the whole component.<br/>
        /// That prevents deregistering and registering all objects when you just want to pause the calculation.<br/>
        /// </remarks>
        public bool PauseCalculations
        {
            get
            {
                return this.pauseCalculations;
            }

            set
            {
                this.pauseCalculations = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum distance calculations per frame<br/>
        /// </summary>
        /// <remarks>
        /// This property can be used to control how much performance may use.<br/>
        /// It limits QuickLod in recalculating the distances from a lod object to the nearest lod source.<br/><br/>
        /// Best choose a number that makes the recalculations just slow enough for your needs.<br/>
        /// If the lod objects are updating to slow, try increasing this value<br/><br/>
        /// This property is a maximum, if there are less objects to calculate, then this value won't be reached.
        /// </remarks>
        /// <example>
        /// This example shows how to implement your own limiting functionality using this property<br/>
        /// <code>
        /// private void UpdateLodObjects()
        /// {
        ///     for (var index = 0; index &lt; this.AllLodObjects.Length &amp;&amp; index &lt; this.UpdatesPerFrame; index++)
        ///     {
        ///         // Calculate the distance for the lod object here...
        ///     }
        /// }
        /// </code>
        /// </example>
        public int UpdatesPerFrame
        {
            get
            {
                return this.updatesPerFrame;
            }

            set
            {
                this.updatesPerFrame = value;

#if UNITY_EDITOR
                this.prevUPF = this.updatesPerFrame;
#endif

                this.OnUpdatesPerFrameChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a source needs an update
        /// </summary>
        /// <remarks>
        /// This property is a flag that can be used to determine whether any source has moved or rotated.<br/>
        /// When this flag is set to true, at least one lod source was moved or rotated.<br/><br/>
        /// You can use this property in order to prevent performing unnecessary lod source related calculations.<br/><br/>
        /// <b>If you use this property, you must reset it to false after every usage.</b>
        /// </remarks>
        public bool HasSourceUpdates
        {
            get
            {
                return this.hasSourceUpdates;
            }

            set
            {
                this.hasSourceUpdates = value;
            }
        }

        /// <summary>
        /// Gets or sets the default hide mode to use.
        /// OBSOLETE: Use LodObjectReplacement, LodObjectMesh and LodObjectSkinnedMesh instead
        /// </summary>
        [Obsolete("The hide mode is not appliable to all lod objects and thus no longer a global variable")]
        public CullMode DefaultHideMode { get; set; }

        /// <summary>
        /// Gets or sets the global distance multiplier
        /// </summary>
        /// <remarks>
        /// You can use this property in order to change the lod distances defined in the lod objects.<br/>
        /// This can be used as a quality setting or as an individual setting for different platforms.<br/><br/>
        /// If you just want to change the distance of a single lod source, use the <see cref="LodSource.LocalDistanceMultiplier"/> instead.
        /// </remarks>
        public float LodDistanceMultiplier
        {
            get
            {
                return this.lodDistanceMultiplier;
            }

            set
            {
                if (Mathf.Abs(this.lodDistanceMultiplier - value) < Mathf.Epsilon)
                {
                    return;
                }

                this.lodDistanceMultiplier = value;
                if (this.lodDistanceMultiplier < 0)
                {
                    this.lodDistanceMultiplier = 0;
                }

                this.UpdateSourceDistanceMultipliers();

#if UNITY_EDITOR
                this.UpdateViewportSourceDistanceMultipliers();
                this.prevLDM = this.lodDistanceMultiplier;
#endif

                this.HasSourceUpdates = true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether another supported lod system is present
        /// </summary>
        public bool UseOtherLodSystem
        {
            get
            {
                return this.useOtherLodSystem;
            }
        }

        /// <summary>
        /// Gets or sets the currently active lod system
        /// </summary>
        public LodSystemSelection ActiveLodSystem
        {
            get
            {
                return this.activeLodSystem;
            }

            set
            {
                if (this.activeLodSystem == value)
                {
                    return;
                }

                this.activeLodSystem = value;
#if UNITY_EDITOR
                this.prevALS = this.activeLodSystem;
#endif
                this.UpdateActiveLodSystem();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether all light sources should be activated when switching the active lod system to QuickLod
        /// </summary>
        public bool ActivateLightsOnSwitchToQuickLod
        {
            get
            {
                return this.activateLightsOnSwitchToQuickLod;
            }

            set
            {
                this.activateLightsOnSwitchToQuickLod = value;
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

        /// <summary>
        /// Gets the optimal border width for source update distances
        /// </summary>
        /// <value>
        /// The optimal source border.
        /// </value>
        public virtual float OptimalSourceBorder
        {
            get
            {
                return 0;
            }
        }

#if UNITY_EDITOR

        /// <summary>
        /// Gets or sets the maximum update distance for the viewport source<br/>
        /// EDITOR ONLY: This property can only be accessed in the Unity editor
        /// </summary>
        public float ViewportMaxUpdateDistance
        {
            get
            {
                return this.viewportMaxUpdateDistance;
            }

            set
            {
                if (Math.Abs(this.viewportDistanceMultiplier - this.prevVMUD) < Mathf.Epsilon)
                {
                    return;
                }

                this.viewportMaxUpdateDistance = value;
                this.prevVMUD = this.viewportDistanceMultiplier;

                this.UpdateViewportSourceDistanceMultipliers();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the source cameras should be used<br/>
        /// EDITOR ONLY: This property can only be accessed in the Unity editor
        /// </summary>
        public bool UseSourcesInEditMode
        {
            get
            {
                return this.useSourcesInEditMode;
            }

            set
            {
                this.useSourcesInEditMode = value;
                this.prevUSIEM = this.useSourcesInEditMode;

                this.UpdateRegisteredLodSources();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the viewport camera should be used<br/>
        /// EDITOR ONLY: This property can only be accessed in the Unity editor
        /// </summary>
        public bool UseSceneCameraInEditMode
        {
            get
            {
                return this.useSceneCameraInEditMode;
            }

            set
            {
                if (value == this.useSceneCameraInEditMode)
                {
                    return;
                }

                this.useSceneCameraInEditMode = value;
                this.prevUSCIEM = this.useSceneCameraInEditMode;

                this.RemoveViewportObject();

                if (this.useSceneCameraInEditMode)
                {
                    this.SetupViewportObject();
                }
            }
        }

        /// <summary>
        /// The distance multiplier for the viewport source<br/>
        /// EDITOR ONLY: This property can only be accessed in the Unity editor
        /// </summary>
        public float ViewportDistanceMultiplier
        {
            get
            {
                return this.viewportDistanceMultiplier;
            }

            set
            {
                if (Mathf.Abs(this.viewportDistanceMultiplier - value) < Mathf.Epsilon)
                {
                    return;
                }

                this.viewportDistanceMultiplier = value;
                this.prevVDM = value;

                this.UpdateViewportSourceDistanceMultipliers();

                this.HasSourceUpdates = true;
            }
        }
#endif

        #endregion

        #region protected properties

        /// <summary>
        /// Gets all registered sources
        /// </summary>
        protected List<LodSource> Sources { get; private set; }

        /// <summary>
        /// Gets all viewport objects
        /// </summary>
        protected GameObject[] ViewportObjects
        {
            get
            {
                return this.viewportObjects;
            }

            private set
            {
                this.viewportObjects = value;
            }
        }

        /// <summary>
        /// Gets all viewport sources
        /// </summary>
        protected LodSource[] ViewportSources
        {
            get
            {
                return this.viewportSources;
            }

            private set
            {
                this.viewportSources = value;
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Registers a new source, ignores already registered sources
        /// </summary>
        /// <param name="source">
        /// The source that should be registered
        /// </param>
        public void RegisterSource(LodSource source)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (!this.UseSourcesInEditMode && !source.IsEditorSource
                || !this.UseSceneCameraInEditMode && source.IsEditorSource)
                {
                    return;
                }
            }
            else if (source.IsEditorSource)
            {
                return;
            }
#endif

            // First check if the source is not yet registered
            if (!this.Sources.Contains(source))
            {
                this.Sources.Add(source);
                this.HasSourceUpdates = true;

                source.GlobalDistanceMultiplier = this.lodDistanceMultiplier;
            }
        }

        /// <summary>
        /// Deregister a source
        /// </summary>
        /// <param name="source">
        /// The source that should be deregistered
        /// </param>
        public void DeregisterSource(LodSource source)
        {
            this.Sources.Remove(source);
            this.HasSourceUpdates = true;
        }

        /// <summary>
        /// Registers to the position update callback, which will be called when the lod objects need to check for position changes
        /// </summary>
        /// <param name="lodObject">
        /// The lod object.
        /// </param>
        /// <returns>Returns a value indicating whether the object is now in the callback system</returns>
        public bool RegisterToPositionUpdateCallback(LodObjectBase lodObject)
        {
            return this.positionUpdateCallbackReceivers.Add(lodObject);
        }

        /// <summary>
        /// Deregisters from the position update callback
        /// </summary>
        /// <param name="lodObject">The lod object.</param>
        public void DeregisterFromPositionUpdateCallback(LodObjectBase lodObject)
        {
            this.positionUpdateCallbackReceivers.Remove(lodObject);
        }
        
        /// <summary>
        /// Gets called when a lod object has changed it's position
        /// </summary>
        /// <param name="lodObject">
        /// The lod object that has changed it's position
        /// </param>
        public virtual void UpdateObjectPosition(LodObjectBase lodObject)
        {
        }

        /// <summary>
        /// Handles source movement
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="previousPosition">
        /// The previous position.
        /// </param>
        public virtual void HandleSourceMovement(LodSource source, Vector3 previousPosition)
        {
        }

        /// <summary>
        /// Registers a new lod object
        /// </summary>
        /// <param name="lodObject">
        /// The lod object that should be registered
        /// </param>
        public abstract void RegisterObject(LodObjectBase lodObject);

        /// <summary>
        /// Deregisters a lod object
        /// </summary>
        /// <param name="lodObject">
        /// The lod object that should be deregistered
        /// </param>
        public abstract void DeregisterObject(LodObjectBase lodObject);

#if UNITY_EDITOR

        /// <summary>
        /// Draws the gizmos for a lod object<br/>
        /// EDITOR ONLY: This method can only be accessed in the Unity editor
        /// </summary>
        /// <param name="lodObject">
        /// The lod object to draw
        /// </param>
        public virtual void DrawLodObjectGizmos(LodObjectBase lodObject)
        {
        }

        /// <summary>
        /// Shows the lod manager in the inspector<br/>
        /// EDITOR ONLY: This method can only be accessed in the Unity editor
        /// </summary>
        [MenuItem("Edit/Quick Lod/Select QuickLod Manager")]
        public static void SelectLodManager()
        {
            if (Instance != null)
            {
                Selection.objects = new Object[] { Instance };
            }
            else
            {
                Debug.LogWarning("No lod manager found.\nAdd a lod manager to the scene to use this feature.");
            }
        }

        /// <summary>
        /// Selects the object that contains the lod manager<br/>
        /// EDITOR ONLY: This method can only be accessed in the Unity editor
        /// </summary>
        [MenuItem("Edit/Quick Lod/Select QuickLod Manager Container")]
        public static void SelectLodManagerObject()
        {
            if (Instance != null)
            {
                Selection.objects = new Object[] { Instance.gameObject };
            }
            else
            {
                Debug.LogWarning("No lod manager found.\nAdd a lod manager to the scene to use this feature.");
            }
        }

        /// <summary>
        /// Prepares the scene for light mapping, activates all level 0 lods and pauses the calculation<br/>
        /// EDITOR ONLY: This method can only be accessed in the Unity editor
        /// </summary>
        [MenuItem("Edit/Quick Lod/Prepare For Lightmapping")]
        public static void PrepareForLightmapping()
        {
            if (
                !EditorUtility.DisplayDialog(
                    "Setup for Lightmapping", 
                    "This will pause the lod manager and set all lod objects to the highest resolution.\n"
                    + "To reactivate the lod system, you need to unpause the lod manager\n\n" + "WARNING:\n"
                    + "If you have a large scene, consider saving the scene first, as this operation can cause a serious lag.", 
                    "Ok", 
                    "Cancel"))
            {
                return;
            }

            if (Instance != null)
            {
                Instance.pauseCalculations = true;
            }

            var lodObjs = FindObjectsOfType(typeof(LodObjectBase)) as LodObjectBase[];
            if (lodObjs != null)
            {
                foreach (var lodObj in lodObjs)
                {
                    lodObj.CurrentLodLevel = 0;
                }
            }
        }

#endif

        #endregion

        #region protected methods

        /// <summary>
        /// Updates the absolute and relative distance for a lod object
        /// </summary>
        /// <param name="lodObjectBase">The lod object base.</param>
        protected static void UpdateLodObjectDistance(LodObjectBase lodObjectBase)
        {
            if (Instance == null)
            {
                return;
            }

            UpdateLodObjectDistance(lodObjectBase, Instance.Sources);
        }
        
        /// <summary>
        /// Updates the absolute and relative distance for a lod object using the given lod sources
        /// </summary>
        /// <param name="lodObjectBase">The lod object base.</param>
        /// <param name="lodSources">The lod sources.</param>
        protected static void UpdateLodObjectDistance(LodObjectBase lodObjectBase, IEnumerable<LodSource> lodSources)
        {
            // The shortest distance found
            var relativeDist = Single.PositiveInfinity;
            var absoluteDist = Single.PositiveInfinity;

            foreach (var source in lodSources)
            {
                float realDist;

                // Get the distance (already multiplied with the distance multiplier when needed)
                var dist = source.GetDistanceToObject(lodObjectBase.Transform.position, out realDist);

                if (dist < relativeDist)
                {
                    relativeDist = dist;
                }

                if (realDist < absoluteDist)
                {
                    absoluteDist = realDist;
                }
            }

            // Apply the new distances to the lod object
            lodObjectBase.SetNewRelativeDistance(relativeDist);
            lodObjectBase.SetNewAbsoluteDistance(absoluteDist);
        }
        
        /// <summary>
        /// Gets called when the object was enabled
        /// </summary>
        protected virtual void OnEnable()
        {
            // When a new instance should be added but there's already an existing instance
            if (Instance != null && Instance != this)
            {
                // Writes a warning and if possible also show a warning window
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorUtility.DisplayDialog(
                        "Single instance only", "There can be only one LodManager at the same time.", "OK");
                }
                else
#endif
                {
                    Debug.LogWarning("There can be only one LodManager at the same time!");
                }

                // Remove the new instance
                DestroyImmediate(this);
                return;
            }

            Instance = this;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // Register to the editor update event (in the editor, the usual update method is not called frequently)
                EditorApplication.update += this.EditorUpdate;
            }
#endif

            this.InitializeLodManager();
        }

        /// <summary>
        /// Gets called when the object was disabled<br/>
        /// DON'T USE: This method must only be called by the Unity engine
        /// </summary>
        protected virtual void OnDisable()
        {
#if UNITY_EDITOR

            // Unregister from the editor update event
            EditorApplication.update -= this.EditorUpdate;

#endif
        }

        /// <summary>
        /// Gets called when the object was destroyed
        /// </summary>
        protected virtual void OnDestroy()
        {
            // Free the instance so a new lod manager can be added
            if (Instance == this)
            { 
                Instance = null;
            }

            // Remove the viewport object
            this.RemoveViewportObject();
        }

        /// <summary>
        /// Is called when this instance is reseted
        /// </summary>
        protected virtual void Reset()
        {
            this.InitializeLodManager();
        }

        /// <summary>
        /// Gets called when the object is started the first time
        /// </summary>
        protected virtual void Awake() 
        {
            // When a new instance should be added but there's already an existing instance
            if (Instance != null && Instance != this)
            {
                // Writes a warning and if possible also show a warning window
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorUtility.DisplayDialog(
                        "Single instance only", "There can be only one LodManager at the same time.", "OK");
                }
                else
#endif
                {
                    Debug.LogWarning("There can be only one LodManager at the same time!");
                }

                // Remove the new instance
                DestroyImmediate(this);
                return;
            }

            Instance = this;

            // Removes the viewport object (used when you start a standalone game where the viewport object was not removed
            this.RemoveViewportObject();
        }

        /// <summary>
        /// Is called once per frame<br/>
        /// This method is not reliable in the editor.<br/>
        /// Use <see cref="NextStep"/> for a reliable update notification instead.
        /// </summary>
        protected virtual void Update()
        {
            if (Application.isPlaying && !this.pauseCalculations)
            {
                this.NextStep();
            }
        }

        /// <summary>
        /// Can be used as "Update()" method for both the editor and the game
        /// </summary>
        protected virtual void NextStep()
        {
            if (this.positionUpdateCallbackCounter < PositionUpdateInterval)
            {
                this.positionUpdateCallbackCounter++;
                return;
            }

            this.positionUpdateCallbackCounter = 0;
            foreach (var lodObject in this.positionUpdateCallbackReceivers)
            {
                lodObject.HandlePositionChange();
            }
        }

        /// <summary>
        /// Called when the amount of allowed updates per frame has changed
        /// </summary>
        protected virtual void OnUpdatesPerFrameChanged()
        {
            
        }

#if UNITY_EDITOR

        /// <summary>
        /// Gets called when the editor update event was thrown<br/>
        /// This method is only called in the editor, use <see cref="NextStep"/> for a reliable update notification instead.
        /// EDITOR ONLY: This method can only be accessed in the Unity editor 
        /// </summary>
        protected virtual void EditorUpdate()
        {
            if (!Application.isPlaying)
            {
                var cameras = SceneView.GetAllSceneCameras();

                // Check if viewport cameras should be updated (destroy and recreate)
                if (this.useSceneCameraInEditMode != this.prevUSCIEM || this.viewportObjects == null
                    || (this.useSceneCameraInEditMode && cameras.Length != this.viewportObjects.Length))
                {
                    this.prevUSCIEM = this.useSceneCameraInEditMode;

                    this.RemoveViewportObject();

                    if (this.useSceneCameraInEditMode)
                    {
                        this.SetupViewportObject();
                    }

                    this.HasSourceUpdates = true;
                }

                // When something else (user...) has deleted the viewport object or source, recreate them
                if (this.useSceneCameraInEditMode)
                {
                    if (this.ViewportObjects == null || this.ViewportSources == null)
                    {
                        this.RemoveViewportObject();
                        this.SetupViewportObject();
                    }
                    else if (this.viewportObjects.Any(obj => obj == null)
                             || this.viewportSources.Any(source => source == null))
                    {
                        this.RemoveViewportObject();
                        this.SetupViewportObject();
                    }
                }

                // Update viewport object placement
                if (this.ViewportObjects != null && !this.PauseCalculations)
                {
                    for (int i = 0; i < this.viewportSources.Length && i < cameras.Length; i++)
                    {
                        var viewSource = this.viewportSources[i];
                        var viewObject = viewSource.gameObject;
                        var currentCamera = cameras[i];

                        viewObject.transform.position = currentCamera.transform.position;
                        viewObject.transform.rotation = currentCamera.transform.rotation;
                        viewSource.AngleMargin = Mathf.Max(currentCamera.fieldOfView, currentCamera.fieldOfView * currentCamera.aspect);
                    }
                }

                if (!this.pauseCalculations)
                {
                    this.NextStep();
                }
            }
        }

        /// <summary>
        /// Gets called when a field has been changed or the object has been enabled<br/>
        /// EDITOR ONLY: This method can only be accessed in the Unity editor
        /// </summary>
        protected virtual void OnValidate()
        {
            // Update the use sources in edit mode when necessary
            if (this.UseSourcesInEditMode != this.prevUSIEM)
            {
                this.prevUSIEM = this.useSourcesInEditMode;
                this.UpdateRegisteredLodSources();
            }

            // Update the viewport distance multiplier when necessary
            if (Mathf.Abs(this.viewportDistanceMultiplier - this.prevVDM) > Mathf.Epsilon)
            {
                this.prevVDM = this.viewportDistanceMultiplier;
                this.UpdateViewportSourceDistanceMultipliers();
                this.HasSourceUpdates = true;
            }

            // Update the lod distance multiplier when necessary
            if (Mathf.Abs(this.lodDistanceMultiplier - this.prevLDM) > Mathf.Epsilon)
            {
                this.prevLDM = this.lodDistanceMultiplier;
                this.UpdateSourceDistanceMultipliers();
                this.HasSourceUpdates = true;
            }

            // Update the currently active lod system
            if (this.activeLodSystem != this.prevALS)
            {
                this.prevALS = this.activeLodSystem;
                this.UpdateActiveLodSystem();
            }

            // update the viewport max update distance when necessary
            if (Math.Abs(this.ViewportMaxUpdateDistance - this.prevVMUD) > Mathf.Epsilon)
            {
                if (this.ViewportMaxUpdateDistance < 5)
                {
                    this.ViewportMaxUpdateDistance = 5;
                }

                this.prevVMUD = this.ViewportMaxUpdateDistance;
                if (this.ViewportSources != null)
                {
                    foreach (var viewport in this.viewportSources)
                    {
                        viewport.MaxUpdateDistance = this.ViewportMaxUpdateDistance;
                    }
                }
            }

            // Notify when the updates per frame has changed
            if (this.updatesPerFrame != this.prevUPF)
            {
                this.prevUPF = this.updatesPerFrame;
                this.OnUpdatesPerFrameChanged();
            }
        }

#endif

        #endregion

        #region private methods

        /// <summary>
        /// Updates the currently active lod system
        /// </summary>
        private void UpdateActiveLodSystem()
        {
            if (!this.useOtherLodSystem || !Application.isPlaying)
            {
                return;
            }

            var lodObjects = FindObjectsOfType(typeof(LodObjectBase)) as LodObjectBase[];
            var iocCams = FindObjectsOfType(Type.GetType("IOCcam")) as MonoBehaviour[];

            if (lodObjects == null || iocCams == null)
            {
                return;
            }

            switch (this.activeLodSystem)
            {
                case LodSystemSelection.QuickLod:
                    {
                        this.pauseCalculations = false;

                        foreach (var iocCam in iocCams)
                        {
                            iocCam.enabled = false;
                        }

                        foreach (var lodObject in lodObjects)
                        {
                            lodObject.ActivateQuickLod();
                        }

                        if (this.activateLightsOnSwitchToQuickLod)
                        {
                            var lights = FindObjectsOfType(typeof(Light)) as Light[];
                            if (lights != null)
                            {
                                foreach (var l in lights)
                                {
                                    if (l.gameObject.GetComponent("IOClight") != null)
                                    {
                                        l.enabled = true;
                                    }
                                }
                            }
                        }

                        break;
                    }

                case LodSystemSelection.InstantOc:
                    {
                        this.pauseCalculations = true;

                        foreach (var iocCam in iocCams)
                        {
                            iocCam.enabled = true;
                        }

                        foreach (var lodObject in lodObjects)
                        {
                            lodObject.DeactivateQuickLod();
                        }

                        break;
                    }
            }
        }

        /// <summary>
        /// Removes the existing viewport object and source
        /// </summary>
        private void RemoveViewportObject()
        {
            if (this.viewportObjects != null)
            {
                // If the whole object still exists, remove it
                foreach (var viewObject in this.viewportObjects.Where(viewObject => viewObject != null))
                {
                    DestroyImmediate(viewObject);
                }
            }

            if (this.viewportSources != null)
            {
                foreach (var viewSource in this.viewportSources.Where(viewSource => viewSource != null))
                {
                    // If only the component still exists (why ever...) just remove it
                    DestroyImmediate(viewSource.gameObject);
                }
            }

            // Clear the references
            this.ViewportObjects = new GameObject[0];
            this.ViewportSources = new LodSource[0];

            // Remove other children with the name "ViewportLodSource"
            for (var i = this.Transform.childCount - 1; i >= 0; i--)
            {
                var child = this.Transform.GetChild(i);
                if (child.name == "ViewportLodSource")
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Updates the source distance multipliers.
        /// </summary>
        private void UpdateSourceDistanceMultipliers()
        {
            foreach (var lodSource in this.Sources)
            {
                lodSource.GlobalDistanceMultiplier = this.LodDistanceMultiplier;
            }
        }

        /// <summary>
        /// Initializes the lod manager.
        /// </summary>
        private void InitializeLodManager()
        {
            // Clear all sources and register all existing sources again
            this.Sources = new List<LodSource>();
            var sources = (LodSource[])FindObjectsOfType(typeof(LodSource));
            foreach (var source in sources)
            {
                // The editor sources are initialized later, so don't use them here
                if (source.enabled && !source.IsEditorSource)
                {
                    this.RegisterSource(source);
                }
            }

#if UNITY_EDITOR
            if (!Application.isPlaying && this.useSceneCameraInEditMode)
            {
                // Setup the viewport object when needed
                this.RemoveViewportObject();
                this.SetupViewportObject();
            }
#endif

            // Check if InstantOC can be used or not
            this.useOtherLodSystem = Type.GetType("IOCcam") != null && Type.GetType("IOClod") != null;
            if (this.useOtherLodSystem)
            {
                this.UpdateActiveLodSystem();
            }
        }

#if UNITY_EDITOR

        /// <summary>
        /// Setup a new viewport object and source<br/>
        /// EDITOR ONLY: This method can only be accessed in the Unity editor
        /// </summary>
        private void SetupViewportObject()
        {
            var sceneCameras = SceneView.GetAllSceneCameras();
            this.viewportObjects = new GameObject[sceneCameras.Length];
            this.viewportSources = new LodSource[this.viewportObjects.Length];

            for (int i = 0; i < this.viewportObjects.Length; i++)
            {
                var newObject = new GameObject("ViewportLodSource");
                newObject.transform.parent = this.Transform;
                this.viewportObjects[i] = newObject;

                var newSource = newObject.AddComponent<LodSource>();
                newSource.MaxUpdateDistance = this.ViewportMaxUpdateDistance;
                newSource.LocalDistanceMultiplier = this.viewportDistanceMultiplier;
                newSource.IsEditorSource = true;
                this.ViewportSources[i] = newSource;
                this.RegisterSource(newSource);
            }
        }
        
        /// <summary>
        /// Updates the viewport source distance multipliers.
        /// </summary>
        private void UpdateViewportSourceDistanceMultipliers()
        {
            foreach (var lodSource in this.ViewportSources)
            {
                lodSource.LocalDistanceMultiplier = this.viewportDistanceMultiplier;
            }
        }

        /// <summary>
        /// Updates the registered lod sources so that only the needed lod sources are registered.
        /// </summary>
        private void UpdateRegisteredLodSources()
        {
            this.Sources = new List<LodSource>();
            this.hasSourceUpdates = true;

            var sources = FindObjectsOfType(typeof(LodSource)) as LodSource[];
            if (sources == null)
            { 
                return;
            }

            foreach (var source in sources)
            {
                if (source.enabled)
                {
                this.RegisterSource(source);}
            }
        }

#endif

        #endregion
    }
}