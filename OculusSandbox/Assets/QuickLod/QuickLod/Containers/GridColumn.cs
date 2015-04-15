// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GridColumn.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   A grid column
//   It stores all s for this column
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod.Containers
{
    using System.Collections.Generic;

    /// <summary>
    /// A grid column<br/>
    /// It stores all <see cref="GridRow"/>s for this column
    /// </summary>
    public class GridColumn
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GridColumn"/> class.
        /// </summary>
        /// <param name="initialAmount">
        /// The initial amount of <see cref="GridRow"/> places to reserve
        /// </param>
        /// <param name="index">
        /// The index of this <see cref="GridColumn"/>
        /// </param>
        /// <param name="parent">
        /// The parent dictionary
        /// </param>
        public GridColumn(int initialAmount, int index, Dictionary<int, GridColumn> parent)
        {
            this.Rows = new Dictionary<int, GridRow>(initialAmount);
            this.Index = index;
            this.Parent = parent;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the index of this column
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the collection of <see cref="GridRow"/> in this <see cref="GridColumn"/>
        /// </summary>
        /// <value>
        /// The rows.
        /// </value>
        public Dictionary<int, GridRow> Rows { get; private set; }

        public Dictionary<int, GridColumn> Parent { get; private set; } 

        #endregion
    }
}