// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GridRow.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   A row that stores a collection of '
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod.Containers
{
    using System.Collections.Generic;

    /// <summary>
    /// A row that stores a collection of <see cref="GridCell"/>'s
    /// </summary>
    public class GridRow
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GridRow"/> class.
        /// </summary>
        /// <param name="initialAmount">
        /// The initial amount of <see cref="GridCell"/> places to reserve
        /// </param>
        /// <param name="index">
        /// The index of this <see cref="GridRow"/>
        /// </param>
        /// <param name="parent">
        /// The <see cref="GridColumn"/> that contains this <see cref="GridRow"/>
        /// </param>
        public GridRow(int initialAmount, int index, GridColumn parent)
        {
            this.Cells = new Dictionary<int, GridCell>(initialAmount);
            this.Index = index;
            this.Parent = parent;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the collection of <see cref="GridCell"/> in this row
        /// </summary>
        public Dictionary<int, GridCell> Cells { get; private set; }

        /// <summary>
        /// Gets the index of this <see cref="GridRow"/>
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the parent <see cref="GridColumn"/>
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public GridColumn Parent { get; private set; }

        #endregion
    }
}