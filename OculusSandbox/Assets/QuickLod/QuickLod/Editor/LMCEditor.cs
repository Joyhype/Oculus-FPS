// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LMCEditor.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The lod manager cubic editor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod.Editor
{
    using System.Globalization;

    using QuickLod.Attributes;

    using UnityEditor;

    using UnityEngine;

    /// <summary>
    /// The lod manager cubic editor.
    /// </summary>
    [CustomEditor(typeof(LodManagerCubic))]
    public class LMCEditor : Editor
    {
        #region Constants

        /// <summary>
        /// The key to store a value indicating whether the general tab was opened or not
        /// </summary>
        private const string IsGeneralOpenKey = "QL_LMCEditor_IsGeneralOpen";

        /// <summary>
        /// The key to store a value indicating whether the gizmos tab was opened or not
        /// </summary>
        private const string IsGizmosOpenKey = "QL_LMCEditor_IsGizmosOpen";

        /// <summary>
        /// The key to store a value indicating whether the grid setup tab was opened or not
        /// </summary>
        private const string IsGridSetupOpenKey = "QL_LMCEditor_IsGridSetupOpen";

        /// <summary>
        /// The key to store a value indicating whether the helper tab was opened or not
        /// </summary>
        private const string IsHelperOpenKey = "QL_LMCEditor_IsHelperOpen";

        /// <summary>
        /// The key to store a value indicating whether the information tab was opened or not
        /// </summary>
        private const string IsInformationOpenKey = "QL_LMCEditor_IsInformationOpen";

        /// <summary>
        /// The key to store the selected tab index
        /// </summary>
        private const string SelectedTabIndexKey = "QL_LMCEditor_SelectedTabIndex";

        #endregion

        #region Fields

        /// <summary>
        /// The active lod system description.
        /// </summary>
        private readonly string[] activeLodSystemDescription = new[]
                                                                   {
                                                                       "QUICKLOD: Fast distance based lod system, best used in open environment."
                                                                       , 
                                                                       "INSTANTOC: Raybased occlussion culling system, best used in closed spaces."
                                                                   };

        /// <summary>
        /// The activate lights on switch to quick lod prop.
        /// </summary>
        private SerializedProperty activateLightsOnSwitchToQuickLodProp;

        /// <summary>
        /// The active lod system prop.
        /// </summary>
        private SerializedProperty activeLodSystemProp;

        /// <summary>
        /// The cell size prop.
        /// </summary>
        private SerializedProperty cellSizeProp;

        /// <summary>
        /// The cell update interval prop.
        /// </summary>
        private SerializedProperty cellUpdateIntervalProp;

        /// <summary>
        /// The clamp objects to grid prop.
        /// </summary>
        private SerializedProperty clampObjectsToGridProp;

        /// <summary>
        /// The draw cell gizmos prop.
        /// </summary>
        private SerializedProperty drawCellGizmosProp;

        /// <summary>
        /// The draw cell level gizmos prop.
        /// </summary>
        private SerializedProperty drawCellLevelGizmosProp;

        /// <summary>
        /// The draw disabled objects prop.
        /// </summary>
        private SerializedProperty drawDisabledObjectsProp;

        /// <summary>
        /// The draw grid gizmos prop.
        /// </summary>
        private SerializedProperty drawGridGizmosProp;

        /// <summary>
        /// The draw in debug prop.
        /// </summary>
        private SerializedProperty drawInDebugProp;

        /// <summary>
        /// The draw managed objects prop.
        /// </summary>
        private SerializedProperty drawManagedObjectsProp;

        /// <summary>
        /// The draw object cell gizmos prop.
        /// </summary>
        private SerializedProperty drawObjectCellGizmosProp;

        /// <summary>
        /// The draw when deselected prop.
        /// </summary>
        private SerializedProperty drawWhenDeselectedProp;

        /// <summary>
        /// The grid size prop.
        /// </summary>
        private SerializedProperty gridSizeProp;

        /// <summary>
        /// The grid start prop.
        /// </summary>
        private SerializedProperty gridStartProp;

        /// <summary>
        /// The help text.
        /// </summary>
        private string helpText;

        /// <summary>
        /// The is general open.
        /// </summary>
        private bool isGeneralOpen = true;

        /// <summary>
        /// The is gizmos open.
        /// </summary>
        private bool isGizmosOpen = true;

        /// <summary>
        /// The is grid setup open.
        /// </summary>
        private bool isGridSetupOpen = true;

        /// <summary>
        /// The is helper open.
        /// </summary>
        private bool isHelperOpen;

        /// <summary>
        /// The is information open.
        /// </summary>
        private bool isInformationOpen = true;

        /// <summary>
        /// The lod distance multiplier prop.
        /// </summary>
        private SerializedProperty lodDistanceMultiplierProp;

        /// <summary>
        /// The managed objects material prop.
        /// </summary>
        private SerializedProperty managedObjectsMaterialProp;

        /// <summary>
        /// The max cell deactivation prop.
        /// </summary>
        private SerializedProperty maxCellDeactivationProp;

        /// <summary>
        /// The pause calculations prop.
        /// </summary>
        private SerializedProperty pauseCalculationsProp;

        /// <summary>
        /// The selected tab index.
        /// </summary>
        private int selectedTabIndex;

        /// <summary>
        /// The updates per frame prop.
        /// </summary>
        private SerializedProperty updatesPerFrameProp;

        /// <summary>
        /// The use other lod system prop.
        /// </summary>
        private SerializedProperty useOtherLodSystemProp;

        /// <summary>
        /// The use scene camera in edit mode prop.
        /// </summary>
        private SerializedProperty useSceneCameraInEditModeProp;

        /// <summary>
        /// The use sources in edit mode prop.
        /// </summary>
        private SerializedProperty useSourcesInEditModeProp;

        /// <summary>
        /// The viewport distance multiplier prop.
        /// </summary>
        private SerializedProperty viewportDistanceMultiplierProp;

        /// <summary>
        /// The viewport max update distance prop.
        /// </summary>
        private SerializedProperty viewportMaxUpdateDistanceProp;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The on inspector gui.
        /// </summary>
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            var lodManagerCubic = LodManagerBase.Instance as LodManagerCubic;

            GUILayout.Space(8);

            this.isHelperOpen = EditorGUILayout.Foldout(this.isHelperOpen, "Component help");
            if (this.isHelperOpen && this.helpText != string.Empty)
            {
                EditorGUILayout.HelpBox(this.helpText, MessageType.Info);
            }

            GUILayout.Space(8);

            this.pauseCalculationsProp.boolValue = GUILayout.Toggle(
                this.pauseCalculationsProp.boolValue, new GUIContent("Pause calculations", "Pauses all calculations"));

            if (this.useOtherLodSystemProp.boolValue)
            {
                EditorGUILayout.PropertyField(
                    this.activeLodSystemProp, 
                    new GUIContent(
                        "Active lod system", 
                        "Define which lod system should be used when the game runs.\n\n"
                        + this.activeLodSystemDescription[this.activeLodSystemProp.enumValueIndex]));

                this.activateLightsOnSwitchToQuickLodProp.boolValue =
                    EditorGUILayout.Toggle(
                        new GUIContent(
                            "Activate lights on toggle", 
                            "Activates all lights containing an \"IOClight\" component when switching the active lod system to QuickLod."), 
                        this.activateLightsOnSwitchToQuickLodProp.boolValue);
            }

            GUILayout.Space(8);

            // Draw the tabs
            this.selectedTabIndex = GUILayout.Toolbar(
                this.selectedTabIndex, 
                new[]
                    {
                        new GUIContent("Global settings", "General settings"), 
                        new GUIContent("Editor settings", "Define in editor behaviour"), 
                    });

            GUILayout.Space(8);

            switch (this.selectedTabIndex)
            {
                    // General
                case 0:
                    {
                        this.isGeneralOpen = EditorGUILayout.Foldout(this.isGeneralOpen, "General settings");
                        if (this.isGeneralOpen)
                        {
                            this.lodDistanceMultiplierProp.floatValue =
                                EditorGUILayout.FloatField(
                                    new GUIContent(
                                        "Relative distance multiplier", 
                                        "The global multiplier for all relative distances."), 
                                    this.lodDistanceMultiplierProp.floatValue);
                            this.updatesPerFrameProp.intValue =
                                EditorGUILayout.IntField(
                                    new GUIContent("Updates per frame", "How many updates per frame"), 
                                    this.updatesPerFrameProp.intValue);

                            GUILayout.Space(8);
                        }

                        this.isGridSetupOpen = EditorGUILayout.Foldout(this.isGridSetupOpen, "Grid setup");
                        if (this.isGridSetupOpen)
                        {
                            this.gridSizeProp.vector3Value = EditorGUILayout.Vector3Field(
                                "Boundary size", this.gridSizeProp.vector3Value);
                            this.gridStartProp.vector3Value = EditorGUILayout.Vector3Field(
                                "Boundary start", this.gridStartProp.vector3Value);

                            this.cellSizeProp.vector3Value =
                                EditorGUILayout.Vector3Field(
                                    new GUIContent("Cell size", "The size of the grid cells"), 
                                    this.cellSizeProp.vector3Value);

                            this.cellUpdateIntervalProp.intValue =
                                EditorGUILayout.IntField(
                                    new GUIContent(
                                        "Cell update interval", 
                                        "The pause between each cell evaluation [0=each frame, 1=each second frame, etc.]"), 
                                    this.cellUpdateIntervalProp.intValue);

                            this.maxCellDeactivationProp.intValue =
                                EditorGUILayout.IntField(
                                    new GUIContent(
                                        "Max cell deactivations", 
                                        "The maximum amount of cells which can cull their content per frame.\nSet this value as high as possible, reduce it when you encounter frame drops when moving a lod source.\nMinimum 1"), 
                                    this.maxCellDeactivationProp.intValue);

                            this.clampObjectsToGridProp.boolValue =
                                GUILayout.Toggle(
                                    this.clampObjectsToGridProp.boolValue, 
                                    new GUIContent(
                                        "Clamp objects to grid", 
                                        "Also considers lod objects which aren't inside the grid."));

                            GUILayout.Space(4);

                            GUILayout.BeginHorizontal();
                            if (
                                GUILayout.Button(
                                    new GUIContent(
                                        "Recalculate grid space", 
                                        "Recalculates the grid size and start point depending on the lod objects in the scene")))
                            {
                                if (lodManagerCubic != null)
                                {
                                    lodManagerCubic.RecalculateGridSpace();
                                }
                            }

                            if (
                                GUILayout.Button(
                                    new GUIContent(
                                        "Recalculate cell size", "Recalculates the cell size depending on the grid size")))
                            {
                                if (lodManagerCubic != null)
                                {
                                    lodManagerCubic.RecalculateCellSize();
                                }
                            }

                            GUILayout.EndHorizontal();

                            GUILayout.Space(8);
                        }

                        this.isInformationOpen = EditorGUILayout.Foldout(this.isInformationOpen, "Information");
                        if (this.isInformationOpen)
                        {
                            var amountOfCells = ((LodManagerCubic)LodManagerBase.Instance).AmountOfCells;
                            EditorGUILayout.LabelField(
                                new GUIContent(
                                    "Cell amount", 
                                    "The amount of cells which can maximally be stored in the current grid configuration."), 
                                new GUIContent(amountOfCells.ToString(CultureInfo.InvariantCulture)));

                            var amountOfUsedCells = ((LodManagerCubic)LodManagerBase.Instance).AmountOfUsedCells;
                            EditorGUILayout.LabelField(
                                new GUIContent("Used cell amount", "The amount of cells which are currently used."), 
                                new GUIContent(amountOfUsedCells.ToString(CultureInfo.InvariantCulture)));
                        }

                        // Check for forbiden values
                        this.lodDistanceMultiplierProp.floatValue = Mathf.Max(
                            0, this.lodDistanceMultiplierProp.floatValue);
                        this.updatesPerFrameProp.intValue = Mathf.Max(0, this.updatesPerFrameProp.intValue);
                        this.cellUpdateIntervalProp.intValue = Mathf.Max(0, this.cellUpdateIntervalProp.intValue);

                        break;
                    }

                    // Editor
                case 1:
                    {
                        this.isGeneralOpen = EditorGUILayout.Foldout(this.isGeneralOpen, "General settings");
                        if (this.isGeneralOpen)
                        {
                            this.useSourcesInEditModeProp.boolValue =
                                EditorGUILayout.Toggle(
                                    new GUIContent("Use sources", "Use the sources in the editor"), 
                                    this.useSourcesInEditModeProp.boolValue);
                            this.useSceneCameraInEditModeProp.boolValue =
                                EditorGUILayout.Toggle(
                                    new GUIContent("Use viewport", "Use the viewport as source"), 
                                    this.useSceneCameraInEditModeProp.boolValue);
                            this.viewportDistanceMultiplierProp.floatValue =
                                EditorGUILayout.FloatField(
                                    new GUIContent(
                                        "Viewport multiplier", "Same as distance multiplier but only for the viewport"), 
                                    this.viewportDistanceMultiplierProp.floatValue);

                            GUILayout.BeginHorizontal();
                            this.viewportMaxUpdateDistanceProp.floatValue =
                                EditorGUILayout.FloatField(
                                    new GUIContent(
                                        "Viewport update distance", "The update distance of the viewport source"), 
                                    this.viewportMaxUpdateDistanceProp.floatValue);
                            if (
                                GUILayout.Button(
                                    new GUIContent(
                                        "Auto", 
                                        "Calculates the ideal value depending on the lod objects in the active scene"), 
                                    EditorStyles.miniButtonRight, 
                                    GUILayout.MaxWidth(50)))
                            {
                                this.viewportMaxUpdateDistanceProp.floatValue = LodSource.CalculateMaxDistance();
                            }

                            GUILayout.EndHorizontal();

                            GUILayout.Space(8);
                        }

                        this.isGizmosOpen = EditorGUILayout.Foldout(this.isGizmosOpen, "Gizmos");
                        if (this.isGizmosOpen)
                        {
                            this.drawCellGizmosProp.boolValue =
                                EditorGUILayout.Toggle(
                                    new GUIContent(
                                        "Draw used cells", "Highlights all cells that contains at least one lod object"), 
                                    this.drawCellGizmosProp.boolValue);
                            this.drawCellLevelGizmosProp.boolValue =
                                EditorGUILayout.Toggle(
                                    new GUIContent("Draw cell level", "Shows the update priority of all cells"), 
                                    this.drawCellLevelGizmosProp.boolValue);
                            this.drawGridGizmosProp.boolValue =
                                EditorGUILayout.Toggle(
                                    new GUIContent("Draw grid", "Draws the whole grid"), 
                                    this.drawGridGizmosProp.boolValue);
                            this.drawObjectCellGizmosProp.boolValue =
                                EditorGUILayout.Toggle(
                                    new GUIContent("Draw object cell", "Draw the cell of the selected lod object"), 
                                    this.drawObjectCellGizmosProp.boolValue);
                            this.drawManagedObjectsProp.boolValue =
                                EditorGUILayout.Toggle(
                                    new GUIContent("Draw managed objects", "Highlights all managed objects"), 
                                    this.drawManagedObjectsProp.boolValue);

                            if (this.drawManagedObjectsProp.boolValue)
                            {
                                this.drawDisabledObjectsProp.boolValue =
                                    EditorGUILayout.Toggle(
                                        new GUIContent("Disabled objects too", "Highlight disabled objects too"), 
                                        this.drawDisabledObjectsProp.boolValue);

                                this.managedObjectsMaterialProp.objectReferenceValue =
                                    EditorGUILayout.ObjectField(
                                        new GUIContent(
                                            "Managed objects material", 
                                            "Define the material used to highlight the managed objects. (Should be transparent)"), 
                                        this.managedObjectsMaterialProp.objectReferenceValue, 
                                        typeof(Material), 
                                        false);
                            }

                            this.drawWhenDeselectedProp.boolValue =
                                EditorGUILayout.Toggle(
                                    new GUIContent(
                                        "Draw when deselected", 
                                        "Draws the gizmos also when the lod manager is deselected"), 
                                    this.drawWhenDeselectedProp.boolValue);

                            this.drawInDebugProp.boolValue =
                                EditorGUILayout.Toggle(
                                    new GUIContent("Draw in debug", "Draws the gizmos in the game view too."), 
                                    this.drawInDebugProp.boolValue);
                        }

                        // Check for forbiden values
                        this.viewportDistanceMultiplierProp.floatValue = Mathf.Max(
                            0, this.viewportDistanceMultiplierProp.floatValue);

                        break;
                    }
            }

            if (this.lodDistanceMultiplierProp.floatValue < 0.1f && !this.pauseCalculationsProp.boolValue)
            {
                GUILayout.Space(8);
                EditorGUILayout.HelpBox(
                    "The distance multiplier is nearly zero. \nConsider pausing the calculations instead, that saves performance.", 
                    MessageType.Warning);
            }

            if (this.viewportDistanceMultiplierProp.floatValue < 0.1f && !this.pauseCalculationsProp.boolValue
                && this.useSceneCameraInEditModeProp.boolValue)
            {
                GUILayout.Space(8);
                EditorGUILayout.HelpBox(
                    "The viewport distance multiplier is nearly zero. \nConsider pausing the calculations or disabling the viewport camera instead, that saves performance.", 
                    MessageType.Warning);
            }

            if (this.updatesPerFrameProp.intValue == 0 && !this.pauseCalculationsProp.boolValue)
            {
                GUILayout.Space(8);
                EditorGUILayout.HelpBox(
                    "The updates per frame is set to zero.\nEach cell updates at least one object per frame.\nIf you want to pause the update, use the \"pause calculation\" instead.", 
                    MessageType.Info);
            }

            // Apply changes
            if (GUI.changed)
            {
                this.serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on disable.
        /// </summary>
        private void OnDisable()
        {
            EditorPrefs.SetBool(IsHelperOpenKey, this.isHelperOpen);
            EditorPrefs.SetBool(IsGeneralOpenKey, this.isGeneralOpen);
            EditorPrefs.SetBool(IsGridSetupOpenKey, this.isGridSetupOpen);
            EditorPrefs.SetBool(IsGizmosOpenKey, this.isGizmosOpen);
            EditorPrefs.SetBool(IsInformationOpenKey, this.isInformationOpen);
            EditorPrefs.SetInt(SelectedTabIndexKey, this.selectedTabIndex);
        }

        /// <summary>
        /// The on enable.
        /// </summary>
        private void OnEnable()
        {
            this.isHelperOpen = EditorPrefs.HasKey(IsHelperOpenKey) && EditorPrefs.GetBool(IsHelperOpenKey);
            this.isGeneralOpen = !EditorPrefs.HasKey(IsGeneralOpenKey) || EditorPrefs.GetBool(IsGeneralOpenKey);
            this.isGridSetupOpen = !EditorPrefs.HasKey(IsGridSetupOpenKey) || EditorPrefs.GetBool(IsGridSetupOpenKey);
            this.isGizmosOpen = !EditorPrefs.HasKey(IsGizmosOpenKey) || EditorPrefs.GetBool(IsGizmosOpenKey);
            this.isInformationOpen = !EditorPrefs.HasKey(IsInformationOpenKey)
                                     || EditorPrefs.GetBool(IsInformationOpenKey);
            this.selectedTabIndex = EditorPrefs.HasKey(SelectedTabIndexKey)
                                        ? EditorPrefs.GetInt(SelectedTabIndexKey)
                                        : 0;

            this.clampObjectsToGridProp = this.serializedObject.FindProperty("clampObjectsToGrid");
            this.lodDistanceMultiplierProp = this.serializedObject.FindProperty("lodDistanceMultiplier");
            this.useSourcesInEditModeProp = this.serializedObject.FindProperty("useSourcesInEditMode");
            this.updatesPerFrameProp = this.serializedObject.FindProperty("updatesPerFrame");
            this.maxCellDeactivationProp = this.serializedObject.FindProperty("maxCellDeactivations");
            this.useSceneCameraInEditModeProp = this.serializedObject.FindProperty("useSceneCameraInEditMode");
            this.viewportDistanceMultiplierProp = this.serializedObject.FindProperty("viewportDistanceMultiplier");
            this.viewportMaxUpdateDistanceProp = this.serializedObject.FindProperty("viewportMaxUpdateDistance");
            this.gridSizeProp = this.serializedObject.FindProperty("gridSize");
            this.gridStartProp = this.serializedObject.FindProperty("gridStart");
            this.cellSizeProp = this.serializedObject.FindProperty("cellSize");
            this.cellUpdateIntervalProp = this.serializedObject.FindProperty("cellUpdateInterval");
            this.pauseCalculationsProp = this.serializedObject.FindProperty("pauseCalculations");
            this.drawCellGizmosProp = this.serializedObject.FindProperty("drawCellGizmos");
            this.drawCellLevelGizmosProp = this.serializedObject.FindProperty("drawCellLevelGizmos");
            this.drawGridGizmosProp = this.serializedObject.FindProperty("drawGridGizmos");
            this.drawWhenDeselectedProp = this.serializedObject.FindProperty("drawWhenDeselected");
            this.drawObjectCellGizmosProp = this.serializedObject.FindProperty("drawObjectCellGizmos");
            this.drawManagedObjectsProp = this.serializedObject.FindProperty("drawManagedObjects");
            this.drawDisabledObjectsProp = this.serializedObject.FindProperty("drawDisabledObjects");
            this.drawInDebugProp = this.serializedObject.FindProperty("drawInDebug");
            this.managedObjectsMaterialProp = this.serializedObject.FindProperty("managedObjectsMaterial");
            this.activeLodSystemProp = this.serializedObject.FindProperty("activeLodSystem");
            this.activateLightsOnSwitchToQuickLodProp =
                this.serializedObject.FindProperty("activateLightsOnSwitchToQuickLod");
            this.useOtherLodSystemProp = this.serializedObject.FindProperty("useOtherLodSystem");

            var helpAttributes =
                this.target.GetType().GetCustomAttributes(typeof(LodHelperAttribute), true) as LodHelperAttribute[];

            if (helpAttributes != null && helpAttributes.Length > 0)
            {
                this.helpText = helpAttributes[0].HelpText;
            }
        }

        #endregion
    }
}