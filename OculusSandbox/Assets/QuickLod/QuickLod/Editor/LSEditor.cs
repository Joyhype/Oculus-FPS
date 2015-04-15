// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LSEditor.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The ls editor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod.Editor
{
    using System.Globalization;

    using QuickLod.Attributes;

    using UnityEditor;

    using UnityEngine;

    /// <summary>
    /// The ls editor.
    /// </summary>
    [CustomEditor(typeof(LodSource))]
    [CanEditMultipleObjects]
    public class LSEditor : Editor
    {
        #region Constants

        /// <summary>
        /// The key to store a value indicating whether the editor tab was opened or not
        /// </summary>
        private const string IsEditorOpenKey = "QL_LSEditor_IsEditorOpen";

        /// <summary>
        /// The key to store a value indicating whether the general tab was opened or not
        /// </summary>
        private const string IsGeneralOpenKey = "QL_LSEditor_IsGeneralOpen";

        /// <summary>
        /// The key to store a value indicating whether the helper tab was opened or not
        /// </summary>
        private const string IsHelperOpenKey = "QL_LSEditor_IsHelperOpen";

        /// <summary>
        /// The key to store a value indicating whether the information tab was opened or not
        /// </summary>
        private const string IsInformationOpenKey = "QL_LSEditor_IsInformationOpen";

        #endregion

        #region Fields

        /// <summary>
        /// The angle margin prop.
        /// </summary>
        private SerializedProperty angleMarginProp;

        /// <summary>
        /// The border width prop.
        /// </summary>
        private SerializedProperty borderWidthProp;

        /// <summary>
        /// The default camer fo v prop.
        /// </summary>
        private SerializedProperty defaultCamerFoVProp;

        /// <summary>
        /// The distance material prop.
        /// </summary>
        private SerializedProperty distanceMaterialProp;

        /// <summary>
        /// The draw gizmos prop.
        /// </summary>
        private SerializedProperty drawGizmosProp;

        /// <summary>
        /// The draw in debug prop.
        /// </summary>
        private SerializedProperty drawInDebugProp;

        /// <summary>
        /// The draw when deselected prop.
        /// </summary>
        private SerializedProperty drawWhenDeselectedProp;

        /// <summary>
        /// The falloff descriptions.
        /// </summary>
        private string[] falloffDescriptions = new[]
                                                   {
                                                       "HARD: Angle based view mode with hard falloff.\n+ Fast\n+ Best chunk optimization\n- Not precise\n- Causes pop in"
                                                       , 
                                                       "HARD WITH BORDER: Like hard mode, but with extended borders.\n+ Good chunk optimization\n- Slow\n- Can cause pop in"
                                                       , 
                                                       "SMOOTH: [Recommended] Angle based view mode with smoothed out falloff.\n+ No pop in\n+- Medium fast\n+- Medium chunk optimization"
                                                   };

        /// <summary>
        /// The falloff prop.
        /// </summary>
        private SerializedProperty falloffProp;

        /// <summary>
        /// The help text.
        /// </summary>
        private string helpText;

        /// <summary>
        /// The ignore hidden prop.
        /// </summary>
        private SerializedProperty ignoreHiddenProp;

        /// <summary>
        /// The is editor open.
        /// </summary>
        private bool isEditorOpen = true;

        /// <summary>
        /// The is general open.
        /// </summary>
        private bool isGeneralOpen = true;

        /// <summary>
        /// The is helper open.
        /// </summary>
        private bool isHelperOpen;

        /// <summary>
        /// The is information open.
        /// </summary>
        private bool isInformationOpen = true;

        /// <summary>
        /// The local distance multiplier prop.
        /// </summary>
        private SerializedProperty localDistanceMultiplierProp;

        /// <summary>
        /// The max update distance prop.
        /// </summary>
        private SerializedProperty maxUpdateDistanceProp;

        /// <summary>
        /// The observe camera prop.
        /// </summary>
        private SerializedProperty observeCameraProp;

        /// <summary>
        /// The observed camera component prop.
        /// </summary>
        private SerializedProperty observedCameraComponentProp;

        /// <summary>
        /// The sphere mesh prop.
        /// </summary>
        private SerializedProperty sphereMeshProp;

        /// <summary>
        /// The update angle margin from camera prop.
        /// </summary>
        private SerializedProperty updateAngleMarginFromCameraProp;

        /// <summary>
        /// The update local multiplier with fo v prop.
        /// </summary>
        private SerializedProperty updateLocalMultiplierWithFoVProp;

        /// <summary>
        /// The use global distance multiplier prop.
        /// </summary>
        private SerializedProperty useGlobalDistanceMultiplierProp;

        /// <summary>
        /// The use view angle prop.
        /// </summary>
        private SerializedProperty useViewAngleProp;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The on inspector gui.
        /// </summary>
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            GUILayout.Space(8);

            this.isHelperOpen = EditorGUILayout.Foldout(this.isHelperOpen, "Component help");
            if (this.isHelperOpen && this.helpText != string.Empty)
            {
                EditorGUILayout.HelpBox(this.helpText, MessageType.Info);
                GUILayout.Space(8);
            }

            this.isGeneralOpen = EditorGUILayout.Foldout(this.isGeneralOpen, new GUIContent("General settings"));
            if (this.isGeneralOpen)
            {
                this.observeCameraProp.boolValue = GUILayout.Toggle(
                    this.observeCameraProp.boolValue, 
                    new GUIContent(
                        "Observe camera", 
                        "Should the lod source observe an available camera component?\n(The camera component must be on the same game object)"));

                this.useGlobalDistanceMultiplierProp.boolValue =
                    GUILayout.Toggle(
                        this.useGlobalDistanceMultiplierProp.boolValue, 
                        new GUIContent(
                            "Use global lod multiplier", 
                            "Defines whether the global lod distance multiplier should be used or not."));

                GUILayout.Space(4);

                // General values
                GUILayout.BeginHorizontal();

                this.maxUpdateDistanceProp.floatValue =
                    EditorGUILayout.FloatField(
                        new GUIContent(
                            "Max update distance", 
                            "Should be set to the greatest lod level distance used in any lod object"), 
                        this.maxUpdateDistanceProp.floatValue);

                if (
                    GUILayout.Button(
                        new GUIContent(
                            "Auto", "Calculates the ideal value depending on the lod objects in the active scene"), 
                        EditorStyles.miniButtonRight, 
                        GUILayout.MaxWidth(50)))
                {
                    this.maxUpdateDistanceProp.floatValue = LodSource.CalculateMaxDistance();
                }

                GUILayout.EndHorizontal();

                if (this.observeCameraProp.boolValue && this.observedCameraComponentProp.objectReferenceValue != null
                    && this.updateLocalMultiplierWithFoVProp.boolValue)
                {
                    GUI.enabled = false;
                }

                this.localDistanceMultiplierProp.floatValue =
                    EditorGUILayout.FloatField(
                        new GUIContent("Local distance mult.", "Define a distance multiplier for this lod source only."), 
                        this.localDistanceMultiplierProp.floatValue);

                GUI.enabled = true;

                if (this.observeCameraProp.boolValue && this.observedCameraComponentProp.objectReferenceValue != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    EditorGUILayout.BeginVertical();

                    this.updateLocalMultiplierWithFoVProp.boolValue =
                        GUILayout.Toggle(
                            this.updateLocalMultiplierWithFoVProp.boolValue, 
                            new GUIContent(
                                "Update local dist. mult. depending on FoV", 
                                "Should the local distance multiplier be adjusted depending on the field of view?"));

                    if (this.updateLocalMultiplierWithFoVProp.boolValue)
                    {
                        this.defaultCamerFoVProp.floatValue =
                            EditorGUILayout.FloatField(
                                new GUIContent("Default FoV", "Define the default FoV at which the zoom factor is 1x."), 
                                this.defaultCamerFoVProp.floatValue);
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space(4);

                // View angle values
                this.useViewAngleProp.boolValue = GUILayout.Toggle(
                    this.useViewAngleProp.boolValue, 
                    new GUIContent(
                        "Use view angle", 
                        "Defines whether the view angle should be taken into account to prioritize chunks in front of the camera"));

                if (this.useViewAngleProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    EditorGUILayout.BeginVertical();

                    this.ignoreHiddenProp.boolValue = GUILayout.Toggle(
                        this.ignoreHiddenProp.boolValue, 
                        new GUIContent(
                            "Ignore hidden stuff", 
                            "Defines whether chunks and lod objects outside the view angle (including falloff) should be ignored."));

                    EditorGUILayout.PropertyField(
                        this.falloffProp, 
                        new GUIContent(
                            "Falloff mode", 
                            "Defines how the chunk selection should be affected by the view angle.\n\n"
                            + this.falloffDescriptions[this.falloffProp.enumValueIndex]), 
                        false);

                    if (this.falloffProp.enumValueIndex == 1)
                    {
                        GUILayout.BeginHorizontal();

                        this.borderWidthProp.floatValue =
                            EditorGUILayout.FloatField(
                                new GUIContent("Border width", "Define how far the border should be extended."), 
                                this.borderWidthProp.floatValue);

                        if (
                            GUILayout.Button(
                                new GUIContent(
                                    "Auto", 
                                    "Calculates the ideal value depending on the lod objects in the active scene"), 
                                EditorStyles.miniButtonRight, 
                                GUILayout.MaxWidth(50)))
                        {
                            this.borderWidthProp.floatValue = LodSource.CalculateOptimalBorderWidth();
                        }

                        GUILayout.EndHorizontal();
                    }

                    if (this.observeCameraProp.boolValue
                        && this.observedCameraComponentProp.objectReferenceValue != null
                        && this.updateAngleMarginFromCameraProp.boolValue)
                    {
                        GUI.enabled = false;
                    }

                    this.angleMarginProp.floatValue =
                        EditorGUILayout.FloatField(
                            new GUIContent(
                                "Angle margin", "The margin to use before the view angle is taken into account"), 
                            this.angleMarginProp.floatValue);

                    GUI.enabled = true;

                    if (this.observeCameraProp.boolValue
                        && this.observedCameraComponentProp.objectReferenceValue != null)
                    {
                        this.updateAngleMarginFromCameraProp.boolValue =
                            GUILayout.Toggle(
                                this.updateAngleMarginFromCameraProp.boolValue, 
                                new GUIContent(
                                    "Update angle margin from camera", 
                                    "Should the camera margin be taken from the camera component?"));
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space(8);

                if (this.observeCameraProp.boolValue)
                {
                    if (this.observedCameraComponentProp.objectReferenceValue == null)
                    {
                        EditorGUILayout.HelpBox(
                            "No camera component found.\nThe camera related functions aren't available.", 
                            MessageType.Warning);
                    }
                    else if (!this.updateAngleMarginFromCameraProp.boolValue
                             && !this.updateLocalMultiplierWithFoVProp.boolValue)
                    {
                        EditorGUILayout.HelpBox(@"You have set Observe Camera to true, but don't use any functions related to this setting.
Consider deactivating this function to prevent unnecessary calculations.", 
                            MessageType.Warning);
                    }
                }

                if (this.falloffProp.enumValueIndex == 1 && this.borderWidthProp.floatValue < 0.1f)
                {
                    EditorGUILayout.HelpBox(@"The border width is nearly zero.
Consider setting the falloff mode to Hard to prevent unnecessary calculations.", MessageType.Warning);
                }
            }

            this.isEditorOpen = EditorGUILayout.Foldout(this.isEditorOpen, new GUIContent("Editor settings"));
            if (this.isEditorOpen)
            {
                this.drawGizmosProp.boolValue = GUILayout.Toggle(
                    this.drawGizmosProp.boolValue, 
                    new GUIContent(
                        "Draw gizmos", "Shows the update distances which are used for the different chunk levels."));

                if (this.drawGizmosProp.boolValue)
                {
                    this.drawWhenDeselectedProp.boolValue = GUILayout.Toggle(
                        this.drawWhenDeselectedProp.boolValue, 
                        new GUIContent("Draw when deselected", "Also draws the gizmos when deselected."));

                    this.drawInDebugProp.boolValue = GUILayout.Toggle(
                        this.drawInDebugProp.boolValue, 
                        new GUIContent("Draw in debug", "Draws the gizmos in the game view too."));

                    this.sphereMeshProp.objectReferenceValue =
                        EditorGUILayout.ObjectField(
                            new GUIContent(
                                "Sphere mesh", 
                                "The mesh to use to display the distances.\nThe mesh should have a radius of 1.\nLeave empty when you want the distances to be shown with default gizmos graphics."), 
                            this.sphereMeshProp.objectReferenceValue, 
                            typeof(Mesh), 
                            false);

                    if (this.sphereMeshProp.objectReferenceValue != null)
                    {
                        this.distanceMaterialProp.objectReferenceValue =
                            EditorGUILayout.ObjectField(
                                new GUIContent(
                                    "Distance material", 
                                    "Used as base material for the distances.\nLeave empty to display all distances with the default gizmos graphic."), 
                                this.distanceMaterialProp.objectReferenceValue, 
                                typeof(Material), 
                                false);
                    }
                }

                GUILayout.Space(8);
            }

            this.isInformationOpen = EditorGUILayout.Foldout(this.isInformationOpen, new GUIContent("Informations"));
            if (this.isInformationOpen)
            {
                if (LodManagerBase.Instance != null)
                {
                    EditorGUILayout.LabelField(
                        new GUIContent(
                            "Global multiplier", "The distance multiplier defined in the current lod manager"), 
                        new GUIContent(
                            LodManagerBase.Instance.LodDistanceMultiplier.ToString(CultureInfo.InvariantCulture)));

                    var resultingMultiplier = LodManagerBase.Instance.LodDistanceMultiplier
                                              * this.localDistanceMultiplierProp.floatValue;

                    EditorGUILayout.LabelField(
                        new GUIContent(
                            "Resulting multiplier", 
                            "The distance multiplier that results from the local and global distance multiplier."), 
                        new GUIContent(resultingMultiplier.ToString(CultureInfo.InvariantCulture)));
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Add a lod manager to the scene to get more informations.", MessageType.None);
                }
            }

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
            EditorPrefs.SetBool(IsEditorOpenKey, this.isEditorOpen);
            EditorPrefs.SetBool(IsInformationOpenKey, this.isInformationOpen);
        }

        /// <summary>
        /// The on enable.
        /// </summary>
        private void OnEnable()
        {
            this.isHelperOpen = EditorPrefs.HasKey(IsHelperOpenKey) && EditorPrefs.GetBool(IsHelperOpenKey);
            this.isGeneralOpen = !EditorPrefs.HasKey(IsGeneralOpenKey) || EditorPrefs.GetBool(IsGeneralOpenKey);
            this.isEditorOpen = !EditorPrefs.HasKey(IsEditorOpenKey) || EditorPrefs.GetBool(IsEditorOpenKey);
            this.isInformationOpen = !EditorPrefs.HasKey(IsInformationOpenKey)
                                     || EditorPrefs.GetBool(IsInformationOpenKey);

            this.maxUpdateDistanceProp = this.serializedObject.FindProperty("maxUpdateDistance");
            this.localDistanceMultiplierProp = this.serializedObject.FindProperty("localDistanceMultiplier");
            this.useGlobalDistanceMultiplierProp = this.serializedObject.FindProperty("useGlobalDistanceMultiplier");
            this.observeCameraProp = this.serializedObject.FindProperty("observeCamera");
            this.observedCameraComponentProp = this.serializedObject.FindProperty("observedCameraComponent");
            this.updateAngleMarginFromCameraProp = this.serializedObject.FindProperty("updateAngleMarginFromCamera");
            this.updateLocalMultiplierWithFoVProp = this.serializedObject.FindProperty("updateLocalMultiplierWithFoV");
            this.defaultCamerFoVProp = this.serializedObject.FindProperty("defaultCamerFoV");
            this.useViewAngleProp = this.serializedObject.FindProperty("useViewAngle");
            this.falloffProp = this.serializedObject.FindProperty("falloff");
            this.ignoreHiddenProp = this.serializedObject.FindProperty("ignoreHidden");
            this.angleMarginProp = this.serializedObject.FindProperty("angleMargin");
            this.borderWidthProp = this.serializedObject.FindProperty("borderWidth");

            this.drawGizmosProp = this.serializedObject.FindProperty("drawGizmos");
            this.drawWhenDeselectedProp = this.serializedObject.FindProperty("drawWhenDeselected");
            this.drawInDebugProp = this.serializedObject.FindProperty("drawInDebug");
            this.sphereMeshProp = this.serializedObject.FindProperty("sphereMesh");
            this.distanceMaterialProp = this.serializedObject.FindProperty("distanceMaterial");

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