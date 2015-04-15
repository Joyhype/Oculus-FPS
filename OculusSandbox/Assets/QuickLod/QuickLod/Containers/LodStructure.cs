// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LodStructure.cs" company="Jasper Ermatinger">
//   Copyright © 2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
// The base class for all lod structures.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod.Containers
{
    using System;

    using UnityEngine;

    /// <summary>
    /// The base class for all lod structures
    /// </summary>
    /// <typeparam name="T">
    /// The type of the lod class you want to store.
    /// </typeparam>
    /// <remarks>
    /// Unity can't serialize and deserialize generics directly.<br/>
    /// Using this class directly will result in data loss when the project is serialized<br/><br/>
    /// In order to use this class for your own lod data, create a new class.<br/>
    /// The new class must define the <see cref="T"/> statically and needs no additional code.<br/><br/>
    /// <example>
    /// This example shows a sample structure to store meshes (taken from <see cref="LodStructureMesh"/>)<br/>
    /// <code>
    /// [Serializable]
    /// public class LodStructureMesh : LodStructure&lt;Mesh&gt;
    /// {
    ///     // This class is only for the serializer of unity, because it cannot directly serialize generic classes
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    [Serializable]
    public class LodStructure<T>
    { 
        #region private fields

        /// <summary>
        /// Backing field for <see cref="Distance"/>
        /// </summary>
        [SerializeField]
        private float distance;

        /// <summary>
        /// Backing field for <see cref="IsVisible"/>
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private bool isVisible;

        /// <summary>
        /// Backing field for <see cref="Lod"/>
        /// </summary>
        [SerializeField]
        private T lod;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LodStructure{T}"/> class.
        /// </summary>
        public LodStructure()
        {
            this.Distance = 0f;
            this.IsVisible = true;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the lod object
        /// </summary>
        /// <value>
        /// The lod
        /// </value>
        public T Lod
        {
            get
            {
                return this.lod;
            }

            set
            {
                if (Equals(this.lod, value))
                {
                    return;
                }

                this.lod = value;

#if UNITY_EDITOR
                this.PreviousLod = this.lod;
#endif
            }
        }

        /// <summary>
        /// Gets or sets the distance for this lod
        /// </summary>
        public float Distance
        {
            get
            {
                return this.distance;
            }

            set
            {
#if UNITY_EDITOR
                if (Mathf.Abs(this.distance - value) < 0.001f)
                {
                    return;
                }

#endif
                this.distance = value;
#if UNITY_EDITOR
                this.PreviousDistance = this.distance;
#endif
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the lod is actually visible or not
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return this.isVisible;
            }

            set
            {
                this.isVisible = value;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// The previous distance
        /// </summary>
        public float PreviousDistance { get; set; }

        /// <summary>
        /// Gets or sets whether a change was made to any value
        /// </summary>
        public T PreviousLod { get; set; }
#endif

        #endregion
    }
}