// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LodStructureMesh.cs" company="Jasper Ermatinger">
//   Copyright © 2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The lod structure mesh.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod.Containers
{
    using System;
    using UnityEngine;

    /// <summary>
    /// The lod structure mesh.
    /// </summary>
    [Serializable]
    public class LodStructureMesh : LodStructure<Mesh>
    {
        // This class is only for the serializer of unity, because it cannot directly serialize generic classes
    }
}