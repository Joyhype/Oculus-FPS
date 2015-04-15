// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LodHelperAttribute.cs" company="Jasper Ermatinger">
//   Copyright © 2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The lod object helper attribute.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod.Attributes
{
    using UnityEngine;

    /// <summary>
    /// The lod object helper attribute.
    /// </summary>
    public class LodHelperAttribute : System.Attribute
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LodHelperAttribute"/> class.
        /// </summary>
        /// <param name="helpText">
        /// The help text.
        /// </param>
        public LodHelperAttribute(string helpText)
        {
            this.HelpText = helpText;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the help text.
        /// </summary>
        public string HelpText { get; private set; }

        #endregion
    }
}