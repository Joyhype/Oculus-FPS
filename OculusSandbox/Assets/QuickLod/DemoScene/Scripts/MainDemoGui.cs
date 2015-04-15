// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestGui.cs" company="Jasper Ermatinger">
//   Copyright © 2014 Jasper Ermatinger
//   Do not distribute or publish this code or parts of it in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The test gui.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace QLDemo
{
    using System.Collections.Generic;
    using System.Linq;

    using QuickLod;

    using UnityEngine;

    /// <summary>
    /// The test gui.
    /// </summary>
    public class MainDemoGui : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// The update interval.
        /// </summary>
        private const float UpdateInterval = 0.5f;

        #endregion

        #region Fields

        /// <summary>
        /// The materials.
        /// </summary>
        public MaterialSetting[] Materials;

        /// <summary>
        /// The placement controller.
        /// </summary>
        public TestPlacement PlacementController;

        public LodSource MouseCamera;

        public LodSource MainCamera;

        public GameObject[] LodObjectReplacements;

        public GameObject[] LodObjectMeshes;

        public CameraBehaviour CameraBehaviour;

        /// <summary>
        /// The current fps.
        /// </summary>
        private float currentFps;

        /// <summary>
        /// The show colors.
        /// </summary>
        private bool showColors = true;

        /// <summary>
        /// The show lod manager.
        /// </summary>
        private bool showLodManager = false;

        /// <summary>
        /// The show object placement.
        /// </summary>
        private bool showObjectPlacement = false;

        /// <summary>
        /// The show scene managing.
        /// </summary>
        private bool showSceneManaging = false;

        private bool showQuality = false;

        /// <summary>
        /// The style.
        /// </summary>
        private GUIStyle style = new GUIStyle();

        /// <summary>
        /// The texture.
        /// </summary>
        private Texture2D texture;

        /// <summary>
        /// The update timer.
        /// </summary>
        private float updateTimer;

        /// <summary>
        /// The use manager.
        /// </summary>
        private bool useManager = true;

        /// <summary>
        /// The used lod level.
        /// </summary>
        private int usedLodLevel;

        private int currentAmount;

        private int levels = 5;

        private enum PlacementType{ObjectReplacement, MeshReplacement}

        private PlacementType placementType = PlacementType.MeshReplacement;

        private int currentHideMode;

        #endregion

        #region Methods

        /// <summary>
        /// The on destroy.
        /// </summary>
        private void OnDestroy()
        {
            foreach (var mat in this.Materials)
            {
                mat.Material1.color = mat.Color;
            }
        }

        /// <summary>
        /// The on gui.
        /// </summary>
        private void OnGUI()
        {
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.contentColor = Color.black;

            Vector2 startPos;
            var rectangles = new List<Rect>();

            // GUI for the placement controller
            if (this.PlacementController != null)
            {
                startPos = new Vector2(20, 20);

                var placementRect = new Rect(startPos.x - 5, startPos.y - 5, 260, this.showObjectPlacement ? 165 : 35);
                rectangles.Add(placementRect);
                GUI.Box(placementRect, string.Empty, this.style);

                if (this.showObjectPlacement)
                {
                    GUI.Label(new Rect(startPos.x, startPos.y + 25, 70, 24), "X Amount");
                    this.PlacementController.XAmount =
                        (int)
                        GUI.HorizontalSlider(
                            new Rect(startPos.x + 70, startPos.y + 33, 140, 20),
                            this.PlacementController.XAmount,
                            1,
                            TestPlacement.MaxAmount);
                    GUI.Label(
                        new Rect(startPos.x + 220, startPos.y + 25, 30, 24), this.PlacementController.XAmount.ToString());

                    GUI.Label(new Rect(startPos.x, startPos.y + 50, 70, 24), "Y Amount");
                    this.PlacementController.ZAmount =
                        (int)
                        GUI.HorizontalSlider(
                            new Rect(startPos.x + 70, startPos.y + 58, 140, 20),
                            this.PlacementController.ZAmount,
                            1,
                            TestPlacement.MaxAmount);
                    GUI.Label(
                        new Rect(startPos.x + 220, startPos.y + 50, 30, 24), this.PlacementController.ZAmount.ToString());

                    GUI.Label(new Rect(startPos.x, startPos.y + 75, 70, 24), "Levels");
                    this.levels =
                        (int)
                        GUI.HorizontalSlider(new Rect(startPos.x + 70, startPos.y + 83, 140, 20), this.levels, 3, 5);
                    GUI.Label(new Rect(startPos.x + 220, startPos.y + 75, 30, 24), this.levels.ToString());

                    GUI.Label(new Rect(startPos.x, startPos.y + 100, 70, 24), "Type");
                    this.placementType =
                        (PlacementType)
                        GUI.SelectionGrid(
                            new Rect(startPos.x + 70, startPos.y + 100, 180, 20),
                            (int)this.placementType,
                            new[] { "Object", "Mesh" },
                            2);

                    if (GUI.Button(new Rect(startPos.x, startPos.y + 125, 250, 30), "Update"))
                    {
                        if (this.placementType == PlacementType.ObjectReplacement)
                        {
                            this.PlacementController.PlacementObject = this.LodObjectReplacements[this.levels - 3];
                        }
                        else
                        {
                            this.PlacementController.PlacementObject = this.LodObjectMeshes[this.levels - 3];
                        }
                        this.currentAmount = this.PlacementController.XAmount * this.PlacementController.ZAmount;
                        this.PlacementController.ForceUpdate();
                    }
                }

                this.showObjectPlacement = GUI.Toggle(
                    new Rect(startPos.x, startPos.y, 250, 24),
                    this.showObjectPlacement,
                    "Lod objects - " + currentAmount + " ("
                    + (this.PlacementController.XAmount * this.PlacementController.ZAmount) + ")" + (this.placementType == PlacementType.ObjectReplacement ? " * " + (this.levels + 1) : string.Empty));
            }

            // Draw the quality tab
            startPos = new Vector2(20, this.showObjectPlacement ? 190 : 60);

            var qualityRect = new Rect(startPos.x - 5, startPos.y - 5, 260, this.showQuality ? 80 : 35);
            rectangles.Add(qualityRect);
            GUI.Box(qualityRect, string.Empty, this.style);

            if (this.showQuality)
            {

                GUI.Label(new Rect(startPos.x, startPos.y + 25, 70, 24), "AntiAliasing");
                QualitySettings.antiAliasing =
                    (int)
                    GUI.HorizontalSlider(
                        new Rect(startPos.x + 70, startPos.y + 33, 140, 20), QualitySettings.antiAliasing, 0, 8);
                GUI.Label(new Rect(startPos.x + 220, startPos.y + 25, 30, 24), QualitySettings.antiAliasing.ToString());

                GUI.Label(new Rect(startPos.x, startPos.y + 50, 70, 24), "Shadow");
                QualitySettings.shadowDistance =
                    (int)
                    GUI.HorizontalSlider(
                        new Rect(startPos.x + 70, startPos.y + 58, 140, 20), QualitySettings.shadowDistance, 0, 150);
                GUI.Label(
                    new Rect(startPos.x + 220, startPos.y + 50, 30, 24), QualitySettings.shadowDistance.ToString());
            }

            this.showQuality = GUI.Toggle(new Rect(startPos.x, startPos.y, 250, 24), this.showQuality, "Quality");

            // Draw the fps
            GUI.Box(new Rect((Screen.width / 2) - 35, 20, 70, 30), string.Empty, this.style);
            GUI.Label(new Rect((Screen.width / 2) - 30, 20, 60, 30), "FPS: " + this.currentFps.ToString("0.0"));

            // Edit for the lod manager
            if (LodManagerBase.Instance != null)
            {
                startPos = new Vector2(Screen.width - 345, 20);

                var lodRect = new Rect(startPos.x - 5, startPos.y - 5, 335, this.showLodManager ? 135 : 35); 
                rectangles.Add(lodRect);
                GUI.Box(lodRect, string.Empty, this.style);

                if (this.showLodManager)
                {
                    LodManagerBase.Instance.PauseCalculations =
                        GUI.Toggle(
                            new Rect(startPos.x, startPos.y + 25, 320, 24),
                            LodManagerBase.Instance.PauseCalculations,
                            "Pause");

                    if (LodManagerBase.Instance.PauseCalculations)
                    {
                        GUI.enabled = false;
                    }
                    else
                    {
                        this.useManager = true;
                    }

                    GUI.Label(new Rect(startPos.x, startPos.y + 50, 120, 24), "Obj/Frame");
                    LodManagerBase.Instance.UpdatesPerFrame =
                        (int)
                        GUI.HorizontalSlider(
                            new Rect(startPos.x + 120, startPos.y + 58, 160, 20),
                            LodManagerBase.Instance.UpdatesPerFrame,
                            1,
                            9999);
                    GUI.Label(
                        new Rect(startPos.x + 290, startPos.y + 50, 30, 24),
                        LodManagerBase.Instance.UpdatesPerFrame.ToString());
                    
                    GUI.Label(new Rect(startPos.x, startPos.y + 75, 120, 24), "Multiplier");
                    LodManagerBase.Instance.LodDistanceMultiplier =
                        ((int)
                         (GUI.HorizontalSlider(
                             new Rect(startPos.x + 120, startPos.y + 83, 160, 20),
                             LodManagerBase.Instance.LodDistanceMultiplier,
                             0.1f,
                             5f) * 10f)) / 10f;
                    GUI.Label(
                        new Rect(startPos.x + 290, startPos.y + 75, 30, 24),
                        LodManagerBase.Instance.LodDistanceMultiplier.ToString());

                    var lmc = LodManagerBase.Instance as LodManagerCubic;
                    if (lmc != null)
                    {
                        GUI.Label(new Rect(startPos.x, startPos.y + 100, 120, 24), "Cell update interval");
                        lmc.CellUpdateInterval =
                            (int)
                            GUI.HorizontalSlider(
                                new Rect(startPos.x + 120, startPos.y + 108, 160, 20), lmc.CellUpdateInterval, 0, 100);
                        GUI.Label(
                            new Rect(startPos.x + 290, startPos.y + 100, 30, 24), lmc.CellUpdateInterval.ToString());
                    }

                    GUI.enabled = true;
                }

                this.showLodManager = GUI.Toggle(
                    new Rect(startPos.x, startPos.y, 295, 24), this.showLodManager, "Lod manager");
            }

            startPos = new Vector2(
                Screen.width - 345, (this.showLodManager && LodManagerBase.Instance != null) ? 160 : 60);

            var sceneRect = new Rect(startPos.x - 5, startPos.y - 5, 335, this.showSceneManaging ? 160 : 35);
            rectangles.Add(sceneRect);
            GUI.Box(sceneRect, string.Empty, this.style);

            // Scene managing
            if (this.showSceneManaging)
            {
                GUI.enabled = this.MouseCamera != null;
                if (this.MouseCamera != null)
                {
                    this.MouseCamera.enabled = GUI.Toggle(
                        new Rect(startPos.x, startPos.y + 25, 330, 24), this.MouseCamera.enabled, "Use mouse camera");
                }
                else
                {
                    GUI.Toggle(new Rect(startPos.x, startPos.y + 50, 330, 24), false, "No mouse camera available");
                }

                GUI.enabled = this.MainCamera != null;
                if (this.MainCamera != null)
                {
                    this.MainCamera.enabled = GUI.Toggle(
                        new Rect(startPos.x, startPos.y + 50, 330, 24), this.MainCamera.enabled, "Use main camera");
                }
                else
                {
                    GUI.Toggle(new Rect(startPos.x, startPos.y + 75, 330, 24), false, "No main camera available");
                }

                var newUseManager = GUI.Toggle(
                    new Rect(startPos.x, startPos.y + 75, 330, 24), this.useManager, "Use manager");

                var needsUpdate = false;
                if (newUseManager != this.useManager)
                {
                    needsUpdate = !newUseManager;

                    if (LodManagerBase.Instance != null)
                    {
                        if (!newUseManager)
                        {
                            LodManagerBase.Instance.PauseCalculations = true;
                        }
                        else
                        {
                            LodManagerBase.Instance.PauseCalculations = false;
                        }
                    }
                }

                this.useManager = newUseManager;

                GUI.enabled = !this.useManager;

                GUI.Label(new Rect(startPos.x, startPos.y + 100, 120, 24), "Lod level");

                var newUsedLodLevel =
                    (int)
                    GUI.HorizontalSlider(new Rect(startPos.x + 120, startPos.y + 108, 160, 20), this.usedLodLevel, 0, 5);

                if (newUsedLodLevel != this.usedLodLevel || needsUpdate)
                {
                    var lodObjects = (LodObjectBase[])FindObjectsOfType(typeof(LodObjectBase));
                    foreach (var lodObject in lodObjects)
                    {
                        lodObject.CurrentLodLevel = newUsedLodLevel;
                    }
                }

                this.usedLodLevel = newUsedLodLevel;

                GUI.enabled = true;

                var newUseColors = GUI.Toggle(
                    new Rect(startPos.x, startPos.y + 125, 330, 20), this.showColors, "Use colors");

                if (newUseColors != this.showColors)
                {
                    if (!newUseColors)
                    {
                        foreach (var mat in this.Materials)
                        {
                            mat.Material1.color = Color.white;
                        }
                    }
                    else
                    {
                        foreach (var mat in this.Materials)
                        {
                            mat.Material1.color = mat.Color;
                        }
                    }
                }

                this.showColors = newUseColors;
            }

            this.showSceneManaging = GUI.Toggle(
                new Rect(startPos.x, startPos.y, 320, 24), this.showSceneManaging, "Scene manager");

            if (this.CameraBehaviour != null)
            {
                this.CameraBehaviour.enabled = !rectangles.Any(rect => rect.Contains(Event.current.mousePosition));
            }
        }

        /// <summary>
        /// The start.
        /// </summary>
        private void Start()
        {
            this.texture = new Texture2D(32, 32);

            for (int y = 0; y < this.texture.height; y++)
            {
                for (int x = 0; x < this.texture.width; x++)
                {
                    var color = new Color(135 / 255f, 206 / 255f, 235 / 255f, 210 / 255f);
                    this.texture.SetPixel(x, y, color);
                }
            }

            this.texture.Apply();
            this.style.normal.background = this.texture;

            foreach (var mat in this.Materials)
            {
                mat.Color = mat.Material1.color;
            }

            this.currentAmount = this.PlacementController.XAmount * this.PlacementController.ZAmount;
        }

        /// <summary>
        /// The update.
        /// </summary>
        private void Update()
        {
            this.updateTimer += Time.deltaTime;
            if (this.updateTimer > UpdateInterval)
            {
                this.updateTimer = 0;
                this.currentFps = 1f / Time.deltaTime;
            }
        }

        #endregion

        /// <summary>
        /// The material setting.
        /// </summary>
        [Serializable]
        public class MaterialSetting
        {
            #region Fields

            /// <summary>
            /// The color.
            /// </summary>
            public Color Color;

            /// <summary>
            /// The material 1.
            /// </summary>
            public Material Material1;

            #endregion
        }
    }
}