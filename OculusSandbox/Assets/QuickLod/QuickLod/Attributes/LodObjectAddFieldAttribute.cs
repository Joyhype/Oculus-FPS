// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LodObjectAddFieldAttribute.cs" company="Jasper Ermatinger">
//   Copyright © 2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The overwrite global attribute.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod.Attributes
{
    using System;

    using UnityEngine;

    /// <summary>
    /// An attribute used to edit the inspector behaviour for a given field.<br/>
    /// Should only be used in a subclass of <see cref="LodObjectBase"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class LodObjectAddFieldAttribute : PropertyAttribute
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LodObjectAddFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="tooltip">
        /// The tooltip that is shown when the user hovers the mouse over the field
        /// </param>
        /// <param name="category">
        /// The category in which the field is placed
        /// </param>
        public LodObjectAddFieldAttribute(string name, string tooltip, Category category)
        {
            this.Name = name;
            this.Tooltip = tooltip;
            this.FieldCategory = category;
        }

        #endregion

        #region Enums

        /// <summary>
        /// All fieldtypes the property could be
        /// </summary>
        public enum Category
        {
            /// <summary>
            /// The field is placed in the global settings category
            /// </summary>
            General,

            /// <summary>
            /// The field is placed in the global settings category but only visible when the OverwriteGlobalSettings is active
            /// </summary>
            OverwriteGlobal,

            /// <summary>
            /// The field is placed in the lod category
            /// </summary>
            Lods,

            /// <summary>
            /// The field is placed in the editor settings category
            /// </summary>
            Editor,

            /// <summary>
            /// The field is placed in the information category
            /// </summary>
            Information
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the name of the field
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the tooltip of the field
        /// </summary>
        public string Tooltip { get; private set; }

        /// <summary>
        /// Gets the category in which the property should be placed.
        /// </summary>
        /// <value>
        /// The field category.
        /// </value>
        public Category FieldCategory { get; private set; }

        #endregion
    }
}