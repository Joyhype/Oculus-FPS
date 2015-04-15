// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LodObjectBase.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The base class for all lod objects
//   It handles un-/registering and core features for all lod objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod
{
    using System;
    using System.Linq;

    using QuickLod.Attributes;
    using QuickLod.Containers;

    using UnityEngine;

    /// <summary>
    /// The base class for all lod objects<br/>
    /// It handles un-/registering and core features for all lod objects.<br/>
    /// </summary>
    /// <remarks>
    /// When you want to create your own lod object, you need to inherit from this base class.<br/><br/>
    /// <b>Methods</b><br/>
    /// You must implement the following methods to cover the basic needs:<br/>
    /// <list type="bullet">
    ///     <item>
    ///         <term><see cref="GetLargestLodDistance"/></term>
    ///         <description>Returns the largest lod distance used</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="GetLodAmount"/></term>
    ///         <description>Returns the amount of lod levels used</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="ForceUpdateAllLods"/></term>
    ///         <description>Completely reinitializes all lod objects</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="OnLodLevelChanged"/></term>
    ///         <description>Updates the current lod object</description>
    ///     </item>
    /// </list><br/>
    /// In order to support more functionality, you can implement the following methods:<br/>
    /// <list type="bullet">
    ///     <item>
    ///         <term><see cref="AutoAssignLodLevels"/></term>
    ///         <description>Automatically generate the lod levels (Set <see cref="CanGenerateLodLevels"/> to true)</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="RecalculateDistances"/></term>
    ///         <description>Calculate new distances for the lod levels (Set <see cref="CanCalculateDistances"/> to true)</description>
    ///     </item>
    /// </list><br/>
    /// <b>Workflow</b><br/>
    /// You can either override <see cref="SetNewRelativeDistance"/> or <see cref="SetNewAbsoluteDistance"/> in order to calculate the new <see cref="CurrentLodLevel"/>.<br/>
    /// Update the lod objects in the method <see cref="OnLodLevelChanged"/><br/><br/>
    /// To store the lod levels, you can use the class <see cref="LodStructure{T}"/>.<br/><br/>
    /// <b>Custom inspector editor:</b><br/>
    /// If you name the lods collection "lods", then the QuickLod inspector editor will recognize the collection and make it available.<br/>
    /// To  make a property available in the inspector editor, you can use the <see cref="LodObjectAddFieldAttribute"/><br/>
    /// To define a help text for your component, add the <see cref="LodHelperAttribute"/> to your class.
    /// </remarks>
    /// <example>
    /// Use the source code of the <see cref="LodObjectMesh"/> class for code examples.
    /// </example>
    [ExecuteInEditMode]
    public abstract class LodObjectBase : MonoBehaviour
    {
        #region protected fields

        /// <summary>
        /// The deactivation size to target when optimizing the distances
        /// </summary>
        protected const float TargetDeactivationSize = 0.015f;

        /// <summary>
        /// The field of view to use when optimizing the distances
        /// </summary>
        protected const float UsedFieldOfView = 60f;

        #endregion

        #region private fields

        /// <summary>
        /// Backing field for <see cref="OverwriteGlobalSettings"/>
        /// </summary>
        [SerializeField]
        private bool overwriteGlobalSettings;

        /// <summary>
        /// The previous position.
        /// </summary>
        private Vector3 previousPosition;

        /// <summary>
        /// Backing field for <see cref="CurrentLodLevel"/>
        /// </summary>
        [SerializeField]
        private int currentLodLevel;

        /// <summary>
        /// Backing field for <see cref="RelativeDistance"/>
        /// </summary>
        [SerializeField]
        private float relativeDistance;

        /// <summary>
        /// Backing field for <see cref="AbsoluteDistance"/>
        /// </summary>
        [SerializeField]
        private float absoluteDistance;

        /// <summary>
        /// Backing field for <see cref="IsStatic"/>
        /// </summary>
        [SerializeField]
        private bool isStatic;

        /// <summary>
        /// Backing field for <see cref="ExcludeFromManager"/>
        /// </summary>
        [SerializeField]
        private bool excludeFromManager;

        /// <summary>
        /// Flag used to determine whether the <see cref="AutoAssignLodLevels"/> method should be called in <see cref="OnEnable"/>
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private bool areLodsInitialized;

        /// <summary>
        /// Stores a reference to the transform component for faster access
        /// </summary> 
        private Transform cachedTransform;
        
#if UNITY_EDITOR

        /// <summary>
        /// Stores the value of <see cref="overwriteGlobalSettings"/><br />
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        private bool prevOGS;

        /// <summary>
        /// Stores the value of <see cref="currentLodLevel"/><br />
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        private int prevCLL;

        /// <summary>
        /// Stores the value of <see cref="excludeFromManager"/><br />
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        private bool prevEFM;

        /// <summary>
        /// Stores the value of <see cref="isStatic"/><br />
        /// EDITOR ONLY: This field can only be accessed in the Unity editor
        /// </summary>
        private bool prevIS;

#endif

        #endregion

        #region constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="LodObjectBase" /> class.
        /// </summary>
        protected LodObjectBase()
        {
            this.isStatic = true;
            this.ParentCell = null;
            this.overwriteGlobalSettings = false;
            this.relativeDistance = float.PositiveInfinity;
            this.absoluteDistance = float.PositiveInfinity;
            this.currentLodLevel = 0;

#if UNITY_EDITOR
            this.prevCLL = this.currentLodLevel;
            this.prevOGS = this.overwriteGlobalSettings;
            this.prevEFM = this.excludeFromManager;
            this.prevIS = this.isStatic;
#endif
        }

        #endregion

        #region events

        /// <summary>
        /// Occurs when the relative distance changed
        /// </summary>
        /// <remarks>
        /// This event only occurs when the lod object is enabled.<br/><br/>
        /// <b>Optional:</b><br/>
        /// In order to unsubscribe from the events when the lod object is being destroyed, use the <see cref="LodObjectDestroying"/> event as a notification.
        /// </remarks>
        public event EventHandler<EventArgs> RelativeDistanceChanged;

        /// <summary>
        /// Occurs when the absolute distance changed
        /// </summary>
        /// <remarks>
        /// This event only occurs when the lod object is enabled.<br/><br/>
        /// <b>Optional:</b><br/>
        /// In order to unsubscribe from the events when the lod object is being destroyed, use the <see cref="LodObjectDestroying"/> event as a notification.
        /// </remarks>
        public event EventHandler<EventArgs> AbsoluteDistanceChanged;

        /// <summary>
        /// Occurs when this component is getting destroyed.
        /// </summary> 
        /// <remarks>
        /// This event is called when the lod object is being destroyed.<br/>
        /// This event can be used to unsubscribe from all other events when the object is being destroyed.
        /// </remarks>
        public event EventHandler<EventArgs> LodObjectDestroying;
        
        /// <summary>
        /// Occurs when this component is getting disabled
        /// </summary>
        /// <remarks>
        /// <b>Optional:</b><br/>
        /// In order to unsubscribe from the events when the lod object is being destroyed, use the <see cref="LodObjectDestroying"/> event as a notification.
        /// </remarks>
        public event EventHandler<EventArgs> LodObjectDisabling;

        /// <summary>
        /// Occurs when this components is getting enabled, so other objects can register to events
        /// </summary>
        /// <remarks>
        /// <b>Optional:</b><br/>
        /// In order to unsubscribe from the events when the lod object is being destroyed, use the <see cref="LodObjectDestroying"/> event as a notification.
        /// </remarks>
        public event EventHandler<EventArgs> LodObjectEnabling;

        /// <summary>
        /// Occurs when the active lod level has changed
        /// </summary>
        /// <remarks>
        /// <b>Optional:</b><br/>
        /// In order to unsubscribe from the events when the lod object is being destroyed, use the <see cref="LodObjectDestroying"/> event as a notification.
        /// </remarks>
        public event EventHandler<EventArgs> ActiveLodLevelChanged;

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets a value indicating whether the object should overwrite global settings if there are any.
        /// </summary>
        public bool OverwriteGlobalSettings
        {
            get
            {
                return this.overwriteGlobalSettings;
            }

            set
            {
                if (this.overwriteGlobalSettings == value)
                {
                    return;
                }

                this.overwriteGlobalSettings = value;
#if UNITY_EDITOR
                this.prevOGS = this.overwriteGlobalSettings;
#endif
                if (LodManagerBase.Instance != null)
                {
                    this.ForceUpdateAllLods();
                }

                this.OnOverwriteGlobalChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the object can change the position or not
        /// </summary>
        /// <remarks>
        /// Setting this flag will prevent the automatic notification when the object moves.<br/><br/>
        /// If you only move this object by code, you can still set this flag and inform the LodManager manually.
        /// </remarks>
        /// <example>
        /// This example shows you how to move the object and inform the LodManger afterwards:
        /// <code>
        /// public void MoveObject(Vector3 v)
        /// {
        ///     this.gameObject.transform.position += v;
        ///     this.HandlePositionChange();
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="HandlePositionChange"/>
        public bool IsStatic
        {
            get
            {
                return this.isStatic;
            }

            set
            {
                if (this.isStatic == value)
                {
                    return;
                }

                this.isStatic = value;

#if UNITY_EDITOR
                this.prevIS = this.isStatic;
#endif

                this.RemoveFromPositionUpdateCallback();
                this.AddToPositionUpdateCallback();
            }
        }

        /// <summary>
        /// Gets the shortest relative Distance to any camera, using relativeDistance multiplier
        /// </summary>
        /// <remarks>
        /// The relative distance takes all distance multipliers into account.<br/>
        /// To get the real distance to the nearest LodSource, use <see cref="LodObjectBase.AbsoluteDistance"/> instead.
        /// </remarks>
        public float RelativeDistance
        {
            get
            {
                return this.relativeDistance;
            }

            private set
            {
                this.relativeDistance = value;
                this.OnRelativeDistanceChanged();
            }
        }

        /// <summary>
        /// Gets the shortest real distance to any camera
        /// </summary>
        /// <remarks>
        /// The absolute distance ignores all distance multipliers.<br/>
        /// To get the relative distance to the nearest LodSource, use <see cref="LodObjectBase.RelativeDistance"/> instead.
        /// </remarks>
        public float AbsoluteDistance
        {
            get
            {
                return this.absoluteDistance;
            }

            private set
            {
                this.absoluteDistance = value;
                this.OnAbsoluteDistanceChanged();
            }
        }

        /// <summary>
        /// Gets or sets the current lod level
        /// </summary>
        /// <remarks>
        /// To get change notification for this property, use the <see cref="ActiveLodLevelChanged"/> event.<br/><br/>
        /// To force a lod level, first set the property <see cref="ExcludeFromManager"/> to true.<br/>
        /// Else the active LodManager will reset your selection.
        /// </remarks>
        public int CurrentLodLevel
        {
            get
            {
                return this.currentLodLevel;
            }

            set
            {
                if (this.currentLodLevel == value)
                {
                    return;
                }

                this.currentLodLevel = value;
#if UNITY_EDITOR
                this.prevCLL = this.currentLodLevel;
#endif

                this.OnLodLevelChanged();
                this.OnActiveLodLevelChanged();
            }
        }

        /// <summary>
        /// Gets or sets the parent cell.<br/>
        /// DON'T USE: This property is managed by the <see cref="LodManagerBase"/> component.
        /// </summary>
        /// <value>
        /// The parent cell.
        /// </value>
        public GridCell ParentCell { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the object should be excluded from the LodManager or not
        /// </summary>
        /// <remarks>
        /// If you want to set the <see cref="LodObjectBase.CurrentLodLevel"/> manually, set this property to true first.<br/>
        /// Else the LodManager will reset the <see cref="LodObjectBase.CurrentLodLevel"/> property. <br/>
        /// This is helpful if you want to force a certain lod level to be used.
        /// </remarks>
        public bool ExcludeFromManager
        {
            get
            {
                return this.excludeFromManager;
            }

            set
            {
                if (this.excludeFromManager == value)
                {
                    return;
                }

                this.excludeFromManager = value;

#if UNITY_EDITOR
                this.prevEFM = this.excludeFromManager;
#endif

                this.Deregister();
                this.Register();

                this.RemoveFromPositionUpdateCallback();
                this.AddToPositionUpdateCallback();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the inherited class supports automatic generation of lod levels
        /// </summary>
        /// <remarks>
        /// To generate the lod levels, use the method <see cref="AutoAssignLodLevels"/>
        /// </remarks>
        public bool CanGenerateLodLevels { get; protected set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the inherited class supports automatic calculation of lod distances
        /// </summary>
        public bool CanCalculateDistances { get; protected set; }

        /// <summary>
        /// Gets the previous position.<br/>
        /// This value is stored to recognize object movement
        /// </summary>
        /// <value>
        /// The previous position.
        /// </value>
        public Vector3 PreviousPosition { get; private set; }

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

        #endregion

        #region protected properties

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="AutoAssignLodLevels"/> method should be called in <see cref="OnEnable"/>
        /// </summary>
        protected bool AreLodsInitialized
        {
            get
            {
                return this.areLodsInitialized;
            }

            set
            {
                this.areLodsInitialized = value;
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Calculates the optimal lod level distances for a given object size.
        /// </summary>
        /// <param name="bounds">The boundary size.</param>
        /// <param name="levelAmount">The amount of lod level distances you want.</param>
        /// <returns></returns>
        public static float[] GetOptimalLevelDistances(float size, int levelAmount)
        {
            if (levelAmount <= 0)
            {
                return new float[0];
            }

            var result = new float[levelAmount];

            var targetMaxDistance = size / (2.0f * Mathf.Tan(UsedFieldOfView * 0.5f * Mathf.Deg2Rad) * TargetDeactivationSize);
            var arrayLengthMultiplicator = Mathf.PI / ((levelAmount + 1) * 2);

            for (int index = 0; index < levelAmount; index++)
            {
                result[index] = Mathf.Round((1 - Mathf.Cos((index + 2f) * arrayLengthMultiplicator)) * targetMaxDistance);
            }

            return result;
        }
        
        /// <summary>
        /// Updates necessary information after the lod object has moved.
        /// </summary>
        /// <remarks>
        /// Call this method after moving the object when the <see cref="IsStatic"/> property is set to true.
        /// </remarks>
        /// <example>
        /// This example shows you how to move the object and inform the LodManger afterwards:
        /// <code>
        /// public void MoveObject(Vector3 v)
        /// {
        ///     this.gameObject.transform.position += v;
        ///     this.HandlePositionChange();
        /// }
        /// </code>
        /// </example> 
        public virtual void HandlePositionChange()
        {
            var pos = this.Transform.position;

            // Check if the position has been changed by over 1 unit
            if (Vector3.SqrMagnitude(pos - this.previousPosition) > 1)
            {
                // Updates the position for the lod manager
                LodManagerBase.Instance.UpdateObjectPosition(this);
                this.previousPosition = pos;
            }
        }

        /// <summary>
        /// Set the new distance for this object that uses distance multipliers
        /// </summary>
        /// <param name="newDistance">
        /// The new relative Distance
        /// </param>
        /// <remarks>
        /// By default, LodObjects overwrite this method in order to get notified when the relative distance has changed.<br/>
        /// By doing so, they get the relative distance, which takes distance multipliers into account.<br/><br/>
        /// The base method must be called when overwriting this method.
        /// </remarks>
        /// <example>
        /// This example shows a general usage of this method
        /// <code>
        /// public override void SetNewRelativeDistance(float newDistance)
        /// {
        ///     // Calls the base method first
        ///     base.SetNewRelativeDistance(newDistance);
        ///     
        ///     // Other code here
        /// }
        /// </code>
        /// <br/>
        /// This example shows how to get the correct lod level from a standard lods collection.<br/>
        /// <i>In this example the lods collection is called lods.</i>
        /// <code>
        /// public override void SetNewRelativeDistance(float newDistance)
        /// {
        ///     base.SetNewRelativeDistance(newDistance);
        ///  
        ///     for (var index = 0; index &lt; this.lods.Length; index++)
        ///     {
        ///         // Get the first lod where the distance is over the relative distance
        ///         if (this.lods[index].Distance &gt; newDistance)  
        ///         {
        ///             this.CurrentLodLevel = index;
        ///             return;
        ///         }
        ///     }  
        ///  
        ///     // If all lod distances are smaller than the relative distance, hide all lods    
        ///     this.CurrentLodLevel = -1;     
        /// }  
        /// </code>
        /// </example> 
        /// <seealso cref="SetNewAbsoluteDistance"/>
        public virtual void SetNewRelativeDistance(float newDistance)
        {
            this.RelativeDistance = newDistance;
        }

        /// <summary>
        /// Set the new distance for this object that uses no distance multipliers
        /// </summary>
        /// <param name="newRealDistance">
        /// The new real Distance
        /// </param>
        /// <remarks>
        /// LodObjects can also overwrite this method in order to get notified when the absolute distance has changed.<br/>
        /// By doing so, they get the absolute distance, which ignores all distance multipliers.<br/><br/>
        /// The base method must be called when overwriting this method. 
        /// </remarks>
        /// <example>
        /// This example shows a general usage of this method
        /// <code>
        /// public override void SetNewAbsoluteDistance(float newDistance)
        /// {
        ///     // Calls the base method first
        ///     base.SetNewAbsoluteDistance(newDistance);
        ///     
        ///     // Other code here
        /// }
        /// </code>
        /// For more examples, please see the <see cref="SetNewRelativeDistance"/> method documentation
        /// </example>
        /// <seealso cref="SetNewRelativeDistance"/>
        public virtual void SetNewAbsoluteDistance(float newRealDistance)
        {
            this.AbsoluteDistance = newRealDistance;
        }

        /// <summary>
        /// Is used to register the lod object to the LodManager.<br/>
        /// Won't register when the component is disabled, no LodManager is available or the <see cref="ExcludeFromManager"/> is set to true.
        /// </summary>
        /// <remarks> 
        /// The <see cref="LodObjectBase"/> manages all the de-/registering.<br/>
        /// Calling this method won't generate any bugs but causes unnecessary overhead.<br/><br/>
        /// Use <see cref="ExcludeFromManager"/> and <see cref="CurrentLodLevel"/> instead.
        /// </remarks>
        public virtual void Register()
        {
            // Don't register when not enabled
            if (!this.enabled || this.ExcludeFromManager || LodManagerBase.Instance == null)
            {
                return;
            }

            // Register the lod object
            LodManagerBase.Instance.RegisterObject(this);
            this.ForceUpdateAllLods();
        }

        /// <summary>
        /// Deregisters the lod object from the LodManager
        /// </summary>
        /// <remarks>
        /// This method is used by the <see cref="LodObjectBase"/>.<br/>
        /// Calling it won't generate any bugs but causes unnecessary overhead.
        /// </remarks>
        public virtual void Deregister()
        {
            // Deregister the lod object
            if (LodManagerBase.Instance != null)
            {
                LodManagerBase.Instance.DeregisterObject(this);
            }
        }
        
        /// <summary>
        /// Tries to automatically assign the lod level values<br/>
        /// </summary>
        /// <remarks>
        /// A lod object is free to support the automatic lod level generation.<br/>
        /// The <see cref="CanGenerateLodLevels"/> tells you if this behavior is supported or not.
        /// </remarks>
        /// <example>
        /// This example shows a simple way to implement your own lod level generator
        /// <code>
        /// public override void AutoAssignLodLevels()
        /// {
        ///     base.AutoAssignLodLevels();
        /// 
        ///     // Get all lod objects
        ///     this.lods = SearchAllLods(); 
        ///     this.RecalculateDistances();
        /// }
        /// 
        /// private Object[] SearchAllLods()
        /// {
        ///     // Search all lod objects you want to use here...
        /// }
        /// </code>
        /// </example>
        public virtual void AutoAssignLodLevels()
        {
        }

        /// <summary>
        /// Automatically recalculates the optimal distances of the lod levels
        /// </summary> 
        /// <remarks>
        /// A lod object is free to support the automatic distance calculation.<br/>
        /// The <see cref="CanCalculateDistances"/> tells you if this behavior is supported or not.
        /// </remarks> 
        /// <example>
        /// This example shows a simple way to implement your own distance generator.
        /// <code>
        /// public override void RecalculateDistances()
        /// {
        ///     base.RecalculateDistances();
        /// 
        ///     var maxDistance = 100;
        ///     var distanceStep = maxDistance / this.lods.Length;
        ///     for (var index = 0; index &lt; this.lods.Length; index++)
        ///     {
        ///         this.lods[index].Distance = (index + 1) * distanceStep;
        ///     }
        /// }
        /// </code>
        /// </example>
        public virtual void RecalculateDistances()
        {
        }

        /// <summary>
        /// Is called when another lod system is deactivated and QuickLod is activated
        /// </summary>
        /// <remarks>
        /// This method can be used by a lod object to get notified when QuickLod is reactivated after another lod system has been active.<br/>
        /// This can be used to initialize the functionality of the lod object.<br/><br/>
        /// This method is optional.
        /// </remarks>
        /// <example>
        /// This example shows a simple way to initialize
        /// <code>
        /// public override void ActivateQuickLod()
        /// {
        ///     base.ActivateQuickLod(); 
        ///     this.ForceUpdateAllLods();
        /// }
        /// </code>
        /// </example>
        public virtual void ActivateQuickLod()
        {
        }

        /// <summary>
        /// Is called when QuickLod is deactivated and another lod system is activated
        /// </summary>
        /// <remarks>
        /// This method can be used by a lod object to get notified when QuickLod is deactivated before another lod system will be activated.<br/>
        /// This can be used to cleanup.<br/><br/>
        /// This method is optional.
        /// </remarks>
        /// <example>
        /// This example shows a simple way to cleanup
        /// <code>
        /// public override void DeactivateQuickLod()
        /// {
        ///     base.DeactivateQuickLod(); 
        /// 
        ///     this.CurrentLodLevel = 0;
        /// }
        /// </code>
        /// </example>
        public virtual void DeactivateQuickLod()
        {
        }

        /// <summary>
        /// Adds this lod object to the position update callback when necessary 
        /// </summary>
        /// <remarks>
        /// If you want a custom behavior to determine whether the lod object may be registered, override this method.<br/>
        /// To register to the position update callback, use the method <see cref="LodManagerBase.RegisterToPositionUpdateCallback"/><br/>
        /// To not register, just do nothing.
        /// </remarks>
        public virtual void AddToPositionUpdateCallback()
        {
            if (LodManagerBase.Instance == null)
            {
                return;
            }

            if (!this.enabled || (this.isStatic && Application.isPlaying) || this.excludeFromManager)
            {
                return;
            }

            LodManagerBase.Instance.RegisterToPositionUpdateCallback(this);
        }

        /// <summary>
        /// Adds this lod object to the position update callback
        /// </summary>
        public virtual void RemoveFromPositionUpdateCallback()
        {
            if (LodManagerBase.Instance == null)
            {
                return;
            }

            LodManagerBase.Instance.DeregisterFromPositionUpdateCallback(this);
        }

        /// <summary>
        /// Gets the largest used lod distance
        /// </summary>
        /// <returns>
        /// Returns the largest found distance
        /// </returns>
        /// <remarks>
        /// This method can be used to find the largest lod distance in a bunch of lod objects.
        /// </remarks>
        /// <example>
        /// This example shows how to get the largest lod distance from a bunch of lod objects.
        /// <code>
        /// public float GetLargestDistance(LodObjectBase[] lodObjects)
        /// {
        ///     var largestDistance = 0f;    
        /// 
        ///     foreach (var lodObject in lodObjects)
        ///     {
        ///         largestDistance = Mathf.Max(lodObject.GetLargestLodDistance(), largestDistance);
        ///     }
        ///     
        ///     return largestDistance;
        /// }
        /// </code>
        /// </example>
        public abstract float GetLargestLodDistance();

        /// <summary>
        /// Gets the amount of lod levels that are setup for this lod level
        /// </summary>
        /// <returns>
        /// Returns the amount
        /// </returns>
        /// <remarks>
        /// This method can be used to get the amount of lod levels the lod object uses.<br/>
        /// This can be useful as the <see cref="LodObjectBase"/> has no direct access to the lod levels
        /// </remarks>
        public abstract int GetLodAmount();

        /// <summary>
        /// Is called when all lod levels needs to be recalculated, usually in the initialization process
        /// </summary>
        /// <remarks>
        /// This method is called, when the lod levels aren&apos;t setup.<br/>
        /// This normally happens in the initializing process.<br/><br/>
        /// That can be used to setup all lod levels.
        /// </remarks>
        /// <example>
        /// In this example, all lod objects are of type <see cref="GameObject"/> and need to be initialized.<br/>
        /// <i>The code is from the implementation in <see cref="LodObjectReplacement"/></i>
        /// <code>
        /// public override void ForceUpdateAllLods()
        /// {
        ///     // Loop through all lods and update them
        ///     for (var i = 0; i &lt; this.Lods.Length; i++)
        ///     {
        ///         // Ignore lods that don't have a game object reference
        ///         if (this.Lods[i].Lod == null)
        ///         {
        ///             continue;
        ///         }
        /// 
        ///         // Get if the lod is visible or not
        ///         var isLodVisible = i == this.CurrentLodLevel;
        ///         this.Lods[i].Lod.SetActive(isLodVisible);
        ///     }
        /// }
        /// </code>
        /// </example>
        public abstract void ForceUpdateAllLods();

#if UNITY_EDITOR

        /// <summary>
        /// Draws the gizmos for this object<br/>
        /// EDITOR ONLY: This method can only be accessed in the Unity editor
        /// </summary>
        /// <remarks>
        /// Calling this method manually can cause graphical bugs.
        /// </remarks>
        public void OnDrawGizmosSelected()
        {
            if (LodManagerBase.Instance != null)
            {
                LodManagerBase.Instance.DrawLodObjectGizmos(this);
            }
        }

#endif

        #endregion

        #region protected methods

        /// <summary>
        /// Extracts the integer at the end of the name, returns <see cref="int.MinValue"/> when no number was found
        /// </summary>
        /// <param name="text">
        /// The text in which the number should be searched
        /// </param>
        /// <returns>
        /// Returns the found number or <see cref="int.MinValue"/> when no number was found
        /// </returns>
        /// <remarks>
        /// This method is made for automatically setting up the lod levels. <br/>
        /// It can still be used in any case where the number at the end of a string is needed.
        /// </remarks>
        protected static int GetNumberAtEndOfString(string text)
        {
            var numberChars = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            bool mayContinue = true;
            int index = text.Length;

            while (mayContinue && index > 0)
            {
                if (numberChars.Contains(text[index - 1]))
                {
                    index--;
                }
                else
                {
                    mayContinue = false;
                }
            }

            return index < text.Length ? int.Parse(text.Substring(index, text.Length - index)) : int.MinValue;
        }

        /// <summary>
        /// Is called when the object gets enabled or instantiated
        /// </summary>
        /// <remarks>
        /// If you override this method, you must call the base method.
        /// </remarks> 
        protected virtual void OnEnable()
        {
            if (!this.AreLodsInitialized)
            {
                if (this.CanGenerateLodLevels)
                {
                    this.AutoAssignLodLevels();
                }

                this.AreLodsInitialized = true;
            }

            // Initializes the lod object
            this.previousPosition = this.Transform.position;

#if UNITY_EDITOR
            this.prevEFM = this.excludeFromManager;
#endif

            // When the "Exclude from manager" was active and the user reverted to prefab where it was not active, then deregister
            this.Deregister();
            this.Register();

            this.AddToPositionUpdateCallback();

            // Updates all lod levels, including hide mode
            this.ForceUpdateAllLods();

            this.OnLodObjectEnabling();
        } 

        /// <summary>
        /// Is called when the object gets disabled or destroyed
        /// </summary>
        /// <remarks>
        /// If you override this method, you must call the base method.
        /// </remarks>
        protected virtual void OnDisable()
        {
            // Deregister the lod object
            this.Deregister();
            this.RemoveFromPositionUpdateCallback();

            // Call the disable event
            this.OnLodObjectDisabling();
        }
        
        /// <summary>
        /// Called when the lod object gets destroyed
        /// </summary>
        protected virtual void OnDestroy()
        {
            this.OnLodObjectDestroying();
        }

        /// <summary>
        /// Gets called when the overwrite global changed value has changed
        /// </summary>
        /// <remarks>
        /// This method is called when the <see cref="OverwriteGlobalSettings"/> has changed.<br/>
        /// Override this method when any global settings are locally overridable.
        /// </remarks>
        protected virtual void OnOverwriteGlobalChanged()
        {
        }

        /// <summary>
        ///     Calls the event ActiveLodLevelChanged
        /// </summary>
        protected void OnActiveLodLevelChanged()
        {
            EventHandler<EventArgs> handler = this.ActiveLodLevelChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets called when the current lod level was changed
        /// </summary>
        /// <remarks>
        /// This method is normally called when the <see cref="CurrentLodLevel"/> has been set.<br/>
        /// Use this method to update the lod levels.
        /// </remarks>
        /// <example>
        /// This example shows a simple way to update the lods depending on the <see cref="CurrentLodLevel"/>
        /// <code>
        /// protected override void OnLodLevelChanged()
        /// {
        ///     for (var index = 0; index &lt; this.lods.Length; index++)
        ///     {
        ///         this.lods[index].lod.SetActive(index == this.CurrentLodLevel);
        ///     }
        /// }
        /// </code>
        /// </example>
        protected abstract void OnLodLevelChanged();

#if UNITY_EDITOR

        /// <summary>
        /// Gets called when the object is instantiated or a value has changed in the gui<br />
        /// EDITOR ONLY: This method can only be accessed in the Unity editor
        /// </summary>
        protected virtual void OnValidate()
        {
            if (!this.enabled)
            {
                return;
            }

            // Check if the overwrite global settings has changed
            if (this.prevOGS != this.overwriteGlobalSettings)
            {
                this.prevOGS = this.overwriteGlobalSettings;
                if (LodManagerBase.Instance != null)
                {
                    this.ForceUpdateAllLods();
                }

                this.OnOverwriteGlobalChanged();
            }

            // Check if the current lod level has changed
            if (this.prevCLL != this.currentLodLevel)
            {
                this.prevCLL = this.currentLodLevel;
                this.OnLodLevelChanged();
            }

            if (this.prevEFM != this.ExcludeFromManager)
            {
                this.prevEFM = this.ExcludeFromManager;

                this.Deregister();
                this.Register();

                this.RemoveFromPositionUpdateCallback();
                this.AddToPositionUpdateCallback();
            }

            if (this.prevIS != this.isStatic)
            {
                this.prevIS = this.isStatic;
                this.RemoveFromPositionUpdateCallback();
                this.AddToPositionUpdateCallback();
            }
        }

#endif

        #endregion

        #region private methods

        /// <summary>
        /// Invokes the <see cref="AbsoluteDistanceChanged"/> event
        /// </summary>
        private void OnAbsoluteDistanceChanged()
        {
            EventHandler<EventArgs> handler = this.AbsoluteDistanceChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Invokes the <see cref="RelativeDistanceChanged"/> event
        /// </summary>
        private void OnRelativeDistanceChanged()
        {
            EventHandler<EventArgs> handler = this.RelativeDistanceChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Invokes the <see cref="LodObjectDisabling"/> event
        /// </summary>
        private void OnLodObjectDisabling()
        {
            EventHandler<EventArgs> handler = this.LodObjectDisabling;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Invokes the <see cref="LodObjectEnabling"/> event
        /// </summary>
        private void OnLodObjectEnabling()
        {
            EventHandler<EventArgs> handler = this.LodObjectEnabling;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Invokes the <see cref="LodObjectDestroying"/> event
        /// </summary>
        private void OnLodObjectDestroying()
        {
            var handler = this.LodObjectDestroying;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}