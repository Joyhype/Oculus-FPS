// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GridCell.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   A grid cell that stores all  references which are managed by the
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod.Containers
{
    using System.Collections.Generic;

    /// <summary>
    /// A grid cell that stores all <see cref="LodObjectBase"/> references which are managed by the <see cref="LodManagerCubic"/>
    /// </summary>
    public class GridCell
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GridCell"/> class.
        /// </summary>
        /// <param name="index">
        /// The index of this <see cref="GridCell"/>.
        /// </param>
        /// <param name="parent">
        /// The parent <see cref="GridRow"/> of this cell
        /// </param>
        public GridCell(int index, GridRow parent)
        {
            this.Content = new List<LodObjectBase>();
            this.PreviousIndex = -1;
            this.Index = index;
            this.Parent = parent;
            this.SourcesInRange = new List<LodSource>();
            this.PriorityLevel = 4;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a collection of all <see cref="LodObjectBase"/> that are inside this <see cref="GridCell"/>
        /// </summary>
        public List<LodObjectBase> Content { get; private set; }

        /// <summary>
        /// Gets or sets the sources in range.
        /// </summary>
        /// <value>
        /// The sources in range.
        /// </value>
        public List<LodSource> SourcesInRange { get; set; } 

        /// <summary>
        /// Gets the index of this <see cref="GridCell"/>
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the parent <see cref="GridRow"/> of this <see cref="GridCell"/>
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public GridRow Parent { get; private set; }

        /// <summary>
        /// Gets or sets the index of the last updated <see cref="LodObjectBase"/>
        /// </summary>
        public int PreviousIndex { get; set; }
        
        /// <summary>
        /// Gets or sets the priority level of this cell
        /// </summary>
        /// <value>
        /// The priority level.
        /// </value>
        public int PriorityLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is marked for hiding.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is marked for hiding; otherwise, <c>false</c>.
        /// </value>
        public bool IsMarkedForHiding { get; set; } 

        /// <summary>
        /// Hides the content.
        /// </summary>
        public void HideContent()
        {
            foreach (var lodObject in this.Content)
            {
                if (lodObject != null)
                {
                    lodObject.CurrentLodLevel = -1;
                }
            }
        }

        #endregion
    }
}