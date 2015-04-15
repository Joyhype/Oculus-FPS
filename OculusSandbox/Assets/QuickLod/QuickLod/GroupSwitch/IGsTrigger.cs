// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGsTriggerBase.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The object enter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod.GroupSwitch
{
    /// <summary>
    /// The lod trigger base.
    /// </summary>
    public interface IGsTrigger
    {
        /// <summary>
        /// Gets a value indicating whether this instance is triggered.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is triggered; otherwise, <c>false</c>.
        /// </value>
        bool IsTriggered { get; }

        /// <summary>
        /// Gets the name for this trigger type
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }
    }
}