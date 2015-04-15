// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestPlacement.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The test placement.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QLDemo
{
    using System;

    using QuickLod;

    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;

#endif

    /// <summary>
    /// The test placement.
    /// </summary>
    [ExecuteInEditMode]
    public class TestPlacement : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// The ground.
        /// </summary>
        public GameObject Ground;

        /// <summary>
        /// The lod manager.
        /// </summary>
        public LodManagerCubic LodManager;

        /// <summary>
        /// The placement object.
        /// </summary>
        public GameObject PlacementObject;

        /// <summary>
        /// The center.
        /// </summary>
        public Vector3 Center;

        /// <summary>
        /// The margin.
        /// </summary>
        public Vector3 Margin;

        /// <summary>
        /// The object size.
        /// </summary>
        public Vector3 ObjectSize;

        /// <summary>
        /// The x amount.
        /// </summary>
        public int XAmount;

        /// <summary>
        /// The z amount.
        /// </summary>
        public int ZAmount;

        /// <summary>
        /// Gets or sets a value indicating whether the target build is 64 bit or not
        /// </summary>
        public bool Is64BitDeployment;

        /// <summary>
        /// The maximum amount for the current system
        /// </summary>
        public static int MaxAmount { get; private set; }

        #endregion

        #region public methods

        /// <summary>
        /// The force update.
        /// </summary>
        [ContextMenu("Update placement")]
        public void ForceUpdate()
        {
            if (XAmount > MaxAmount)
            {
                XAmount = MaxAmount;
            }
            if (ZAmount > MaxAmount)
            { 
                ZAmount = MaxAmount;
            }

            for (var i = this.transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                {
                    Destroy(this.transform.GetChild(i).gameObject);
                }
                else
                {
                    DestroyImmediate(this.transform.GetChild(i).gameObject);
                }
            }

            if (this.PlacementObject == null || this.XAmount == 0 || this.ZAmount == 0
                || Math.Abs(this.ObjectSize.x - 0) < 0.001f || Math.Abs(this.ObjectSize.z - 0) < 0.001f)
            {
                return;
            }

            var startPosX = ((((this.XAmount - 1) * this.ObjectSize.x) + ((this.XAmount - 1) * this.Margin.x)) / 2);
            var startPosZ = ((((this.ZAmount - 1) * this.ObjectSize.z) + ((this.ZAmount - 1) * this.Margin.z)) / 2);

            var scaleX = (this.XAmount * this.ObjectSize.x) + ((this.XAmount - 1) * this.Margin.x);
            var scaleZ = (this.ZAmount * this.ObjectSize.z) + ((this.ZAmount - 1) * this.Margin.z);

            if (this.Ground != null)
            {
                this.Ground.transform.position = new Vector3(
                    this.Center.x, this.Center.y - (this.ObjectSize.y / 2), this.Center.z);

                this.Ground.transform.localScale = new Vector3(scaleX + 2, 1f, scaleZ + 2);
            }

            if (this.LodManager != null)
            {
                this.LodManager.GridStart = new Vector3(
                    -(scaleX / 2) - 1, this.LodManager.GridStart.y, -(scaleZ / 2) - 1);
                this.LodManager.GridSize = new Vector3(scaleX + 2, this.LodManager.GridSize.y, scaleZ + 2);
                this.LodManager.RecalculateCellSize();
            }

            for (var i = 0; i < this.XAmount; i++)
            {
                for (var j = 0; j < this.ZAmount; j++)
                {
                    GameObject obj;

#if UNITY_EDITOR
                    if (Application.isPlaying)
                    {
                        obj = (GameObject)PrefabUtility.InstantiatePrefab(this.PlacementObject);
                    }
                    else
#endif
                    {
                        obj = Instantiate(this.PlacementObject);
                    }

                    obj.transform.position = new Vector3(
                        startPosX - (i * (this.ObjectSize.x + this.Margin.x)),
                        this.Center.y,
                        startPosZ - (j * (this.ObjectSize.z + this.Margin.z)));
                    obj.transform.parent = this.transform;
                }
            }
        }

        #endregion

        #region private methods

        private void OnEnable()
        {
            if (Application.isEditor)
            {
                MaxAmount = 600;
            }
            else
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.LinuxPlayer:
                    case RuntimePlatform.OSXPlayer:
                    case RuntimePlatform.WindowsPlayer:
                        {
                            if (this.Is64BitDeployment)
                            {
                                MaxAmount = 1000;
                            }
                            else
                            {
                                MaxAmount = 250;
                            }

                            break;
                        }
                    default:
                        {
                            MaxAmount = 100;
                            break;
                        }
                }
            }
        }

        #endregion
    }
}