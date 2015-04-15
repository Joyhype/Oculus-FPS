// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LOBEditor.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The lob editor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod.Editor
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    using QuickLod.Attributes;

    using UnityEditor;

    using UnityEngine;

    /// <summary>
    /// The lob editor.
    /// </summary>
    [CustomEditor(typeof(LodObjectBase), true)]
    [CanEditMultipleObjects]
    public class LOBEditor : Editor
    {
        #region Constants

        /// <summary>
        /// The key to store a value indicating whether the editor tab was opened or not
        /// </summary>
        private const string IsEditorOpenKey = "QL_LOBEditor_IsEditorOpen";

        /// <summary>
        /// The key to store a value indicating whether the general tab was opened or not
        /// </summary>
        private const string IsGeneralOpenKey = "QL_LOBEditor_IsGeneralOpen";

        /// <summary>
        /// The key to store a value indicating whether the helper tab was opened or not
        /// </summary>
        private const string IsHelperOpenKey = "QL_LOBEditor_IsHelperOpen";

        /// <summary>
        /// The key to store a value indicating whether the information tab was opened or not
        /// </summary>
        private const string IsInformationOpenKey = "QL_LOBEditor_IsInformationOpen";

        /// <summary>
        /// The key to store a value indicating whether the lods tab was opened or not
        /// </summary>
        private const string IsLodsOpenKey = "QL_LOBEditor_IsLodsOpen";

        #endregion

        #region Fields

        /// <summary>
        /// The absolute distance prop.
        /// </summary>
        private SerializedProperty absoluteDistanceProp;

        /// <summary>
        /// The additional fields.
        /// </summary>
        private Dictionary<SerializedProperty, LodObjectAddFieldAttribute> additionalFields;

        /// <summary>
        /// The can calculate distances.
        /// </summary>
        private bool canCalculateDistances;

        /// <summary>
        /// The can generate lod levels.
        /// </summary>
        private bool canGenerateLodLevels;

        /// <summary>
        /// The current lod level prop.
        /// </summary>
        private SerializedProperty currentLodLevelProp;

        /// <summary>
        /// The default color.
        /// </summary>
        private Color defaultColor;

        /// <summary>
        /// The distance prop.
        /// </summary>
        private SerializedProperty distanceProp;

        /// <summary>
        /// The exclude from manager prop.
        /// </summary>
        private SerializedProperty excludeFromManagerProp;

        /// <summary>
        /// The help text.
        /// </summary>
        private string helpText;

        /// <summary>
        /// The is editor open.
        /// </summary>
        private bool isEditorOpen;

        /// <summary>
        /// The is editor visible.
        /// </summary>
        private bool isEditorVisible;

        /// <summary>
        /// The is general open.
        /// </summary>
        private bool isGeneralOpen;

        /// <summary>
        /// The is helper open.
        /// </summary>
        private bool isHelperOpen;

        /// <summary>
        /// The is information open.
        /// </summary>
        private bool isInformationOpen;

        /// <summary>
        /// The is lods open.
        /// </summary>
        private bool isLodsOpen;

        /// <summary>
        /// The is lods visible.
        /// </summary>
        private bool isLodsVisible;

        /// <summary>
        /// The is overwrite visible.
        /// </summary>
        private bool isOverwriteVisible;

        /// <summary>
        /// The is static prop.
        /// </summary>
        private SerializedProperty isStaticProp;

        /// <summary>
        /// The lods prop.
        /// </summary>
        private SerializedProperty lodsProp;

        /// <summary>
        /// The needs update.
        /// </summary>
        private bool needsUpdate;

        /// <summary>
        /// The overwrite global settings prop.
        /// </summary>
        private SerializedProperty overwriteGlobalSettingsProp;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The on inspector gui.
        /// </summary>
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            this.needsUpdate = false;
            this.defaultColor = GUI.color;

            GUILayout.Space(8);

            this.isHelperOpen = EditorGUILayout.Foldout(this.isHelperOpen, "Component help");
            if (this.isHelperOpen && this.helpText != string.Empty)
            {
                EditorGUILayout.HelpBox(this.helpText, MessageType.Info);
                GUILayout.Space(8);
            }

            this.isGeneralOpen = EditorGUILayout.Foldout(this.isGeneralOpen, "General settings");
            if (this.isGeneralOpen)
            {
                this.isStaticProp.boolValue = GUILayout.Toggle(
                    this.isStaticProp.boolValue, 
                    new GUIContent("Is static", "Activate this option when the object doesn't move in the game"));

                if (LodManagerBase.Instance == null || !LodManagerBase.Instance.enabled)
                {
                    GUI.enabled = false;
                }

                this.excludeFromManagerProp.boolValue = GUILayout.Toggle(
                    this.excludeFromManagerProp.boolValue, 
                    new GUIContent(
                        "Exclude from manager", "Defines if the object should be ignored by the global manager"));

                GUILayout.Space(4);
                GUI.enabled = true;

                var lobs = this.targets.OfType<LodObjectBase>().ToArray();
                if (this.isLodsVisible || lobs.Any())
                {
                    var currentLodLevelHint = "The currently active lod level.\n(A value of -1 hides all lod levels)";
                    if (!this.excludeFromManagerProp.boolValue && LodManagerBase.Instance != null
                        && LodManagerBase.Instance.enabled)
                    {
                        GUI.enabled = false;
                        currentLodLevelHint =
                            "The currently active lod level.\nYou need to activate \"Exclude from manager\" to modify this variable.";
                    }

                    var lodLevels = lobs.Select(lob => lob.CurrentLodLevel).Distinct().ToArray();
                    var maxLodLevel = lobs.Max(lob => lob.GetLodAmount()) - 1;
                    var hasMultipleAmounts = lodLevels.Count() > 1;

                    if (hasMultipleAmounts)
                    {
                        var newValue =
                            EditorGUILayout.IntSlider(
                                new GUIContent("Current lod level", currentLodLevelHint), -1, -1, maxLodLevel);
                        if (newValue != -1)
                        {
                            this.currentLodLevelProp.intValue = newValue;
                        }
                    }
                    else
                    {
                        this.currentLodLevelProp.intValue =
                            EditorGUILayout.IntSlider(
                                new GUIContent("Current lod level", currentLodLevelHint), 
                                this.currentLodLevelProp.intValue, 
                                -1, 
                                maxLodLevel);
                    }
                }

                GUILayout.Space(4);
                GUI.enabled = true;

                foreach (var field in
                    this.additionalFields.Where(
                        field => field.Value.FieldCategory == LodObjectAddFieldAttribute.Category.General))
                {
                    EditorGUILayout.PropertyField(field.Key, new GUIContent(field.Value.Name, field.Value.Tooltip));
                }

                if (this.isOverwriteVisible && this.lodsProp != null)
                {
                    this.overwriteGlobalSettingsProp.boolValue =
                        GUILayout.Toggle(
                            this.overwriteGlobalSettingsProp.boolValue, 
                            new GUIContent(
                                "Use local settings", 
                                "Activate this option if the lod object should use the local settings instead of the global ones"));

                    if (this.overwriteGlobalSettingsProp.boolValue)
                    {
                        foreach (var field in
                            this.additionalFields.Where(
                                field =>
                                field.Value.FieldCategory == LodObjectAddFieldAttribute.Category.OverwriteGlobal))
                        {
                            EditorGUILayout.PropertyField(
                                field.Key, new GUIContent(field.Value.Name, field.Value.Tooltip));
                        }
                    }
                }

                GUILayout.Space(8);
            }

            this.isLodsOpen = (this.isLodsVisible || this.lodsProp != null)
                              && EditorGUILayout.Foldout(this.isLodsOpen, "Lods");
            if (this.isLodsOpen)
            {
                if (this.lodsProp != null)
                {
                    this.DrawLods();

                    GUILayout.Space(4);
                    GUI.color = Color.green;

                    if (GUILayout.Button(
                        new GUIContent("New lod level", "Adds a new lod level at the end of this list")))
                    {
                        this.lodsProp.InsertArrayElementAtIndex(Mathf.Max(this.lodsProp.arraySize - 1, 0));
                    }

                    GUI.color = this.defaultColor;
                }

                GUILayout.BeginHorizontal();

                if (this.canGenerateLodLevels)
                {
                    if (
                        GUILayout.Button(
                            new GUIContent(
                                "Generate all", "Generates a new list of lod levels and recalculates the distances.")))
                    {
#if UNITY_4_1 || UNITY_4_2
                        Undo.RegisterUndo(this.targets, "Generate all");
#else
                        Undo.RecordObjects(this.targets, "Generate all");
#endif
                        this.GenerateLodLevels();
                    }
                }

                if (this.canCalculateDistances)
                {
                    if (GUILayout.Button(new GUIContent("Optimize distances", "Recalculates the distances")))
                    {
#if UNITY_4_1 || UNITY_4_2
                        Undo.RegisterUndo(this.targets, "Optimize distances");
#else
                        Undo.RecordObjects(this.targets, "Optimize distances");
#endif
                        this.GenerateLodDistances();
                    }
                }

                GUILayout.EndHorizontal();

                GUILayout.Space(4);
                foreach (var field in
                    this.additionalFields.Where(
                        field => field.Value.FieldCategory == LodObjectAddFieldAttribute.Category.Lods))
                {
                    EditorGUILayout.PropertyField(field.Key, new GUIContent(field.Value.Name, field.Value.Tooltip));
                }

                GUILayout.Space(8);
            }

            if (this.isEditorVisible)
            {
                this.isEditorOpen = EditorGUILayout.Foldout(this.isEditorOpen, "Editor settings");

                if (this.isEditorOpen)
                {
                    foreach (var field in
                        this.additionalFields.Where(
                            field => field.Value.FieldCategory == LodObjectAddFieldAttribute.Category.Editor))
                    {
                        EditorGUILayout.PropertyField(field.Key, new GUIContent(field.Value.Name, field.Value.Tooltip));
                    }

                    GUILayout.Space(8);
                }
            }

            this.isInformationOpen = EditorGUILayout.Foldout(this.isInformationOpen, "Informations");

            if (this.excludeFromManagerProp.boolValue || LodManagerBase.Instance == null
                || !LodManagerBase.Instance.enabled)
            {
                GUI.enabled = false;
            }

            if (this.isInformationOpen)
            {
                var relDistText = !this.excludeFromManagerProp.boolValue && LodManagerBase.Instance != null
                                  && LodManagerBase.Instance.enabled
                                      ? this.distanceProp.floatValue.ToString("0.00", CultureInfo.InvariantCulture)
                                      : "Not managed";
                EditorGUILayout.LabelField(
                    new GUIContent(
                        "Relative distance", "The shortes distance to any lod source using the distance multiplier"), 
                    new GUIContent(relDistText));

                var absDistText = !this.excludeFromManagerProp.boolValue && LodManagerBase.Instance != null
                                  && LodManagerBase.Instance.enabled
                                      ? this.absoluteDistanceProp.floatValue.ToString(
                                          "0.00", CultureInfo.InvariantCulture)
                                      : "Not managed";
                EditorGUILayout.LabelField(
                    new GUIContent("Absolute distance", "The shortes absolute distance to any lod source"), 
                    new GUIContent(absDistText));

                foreach (var field in
                    this.additionalFields.Where(
                        field => field.Value.FieldCategory == LodObjectAddFieldAttribute.Category.Information))
                {
                    EditorGUILayout.PropertyField(field.Key, new GUIContent(field.Value.Name, field.Value.Tooltip));
                }

                GUILayout.Space(8);
            }

            GUI.enabled = true;

            if (GUI.changed)
            {
                this.serializedObject.ApplyModifiedProperties();
            }

            if (this.needsUpdate)
            {
                foreach (var lodObject in this.targets.OfType<LodObjectBase>())
                {
                    lodObject.ForceUpdateAllLods();
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The draw lods.
        /// </summary>
        private void DrawLods()
        {
            var index = 0;

            while (index < this.lodsProp.arraySize)
            {
                var lodLevel = this.lodsProp.GetArrayElementAtIndex(index);

                GUILayout.BeginHorizontal();

                GUILayout.Label(
                    index.ToString(CultureInfo.InvariantCulture) + ":", 
                    index == this.currentLodLevelProp.intValue ? EditorStyles.label : EditorStyles.whiteLabel);
                EditorGUILayout.PropertyField(
                    lodLevel.FindPropertyRelative("lod"), new GUIContent(string.Empty), GUILayout.MinWidth(60));

                GUILayout.Label(new GUIContent("D:", "Maximum distance"), GUILayout.Width(18));

                var distObj = lodLevel.FindPropertyRelative("distance");
                var text = GUILayout.TextField(
                    distObj.floatValue.ToString(CultureInfo.InvariantCulture), 
                    9, 
                    EditorStyles.numberField, 
                    GUILayout.MinWidth(40));
                float value;
                if (!float.TryParse(text, out value))
                {
                    if (text == string.Empty)
                    {
                        value = 0;
                    }
                }

                distObj.floatValue = value;

                var icoUp = Resources.Load("LodLevelListUp") as Texture;
                var icoDown = Resources.Load("LodLevelListDown") as Texture;

                GUI.enabled = index > 0;

                // Moves the lod level up
                if (GUILayout.Button(
                    new GUIContent(icoUp, "Switch the lod level object with the previous one"), 
                    EditorStyles.miniButtonLeft, 
                    GUILayout.Width(22)))
                {
                    if (index > 0)
                    {
                        var prevLevel = this.lodsProp.GetArrayElementAtIndex(index - 1);
                        var prevObject = prevLevel.FindPropertyRelative("lod").objectReferenceValue;
                        prevLevel.FindPropertyRelative("lod").objectReferenceValue =
                            lodLevel.FindPropertyRelative("lod").objectReferenceValue;
                        lodLevel.FindPropertyRelative("lod").objectReferenceValue = prevObject;

                        this.needsUpdate = true;
                    }
                }

                GUI.enabled = index < this.lodsProp.arraySize - 1;

                // Moves the lod level down
                if (GUILayout.Button(
                    new GUIContent(icoDown, "Switch the lod level object with the next one"), 
                    EditorStyles.miniButtonRight, 
                    GUILayout.Width(22)))
                {
                    if (index < this.lodsProp.arraySize - 2)
                    {
                        var nextLevel = this.lodsProp.GetArrayElementAtIndex(index + 1);
                        var nextObject = nextLevel.FindPropertyRelative("lod").objectReferenceValue;
                        nextLevel.FindPropertyRelative("lod").objectReferenceValue =
                            lodLevel.FindPropertyRelative("lod").objectReferenceValue;
                        lodLevel.FindPropertyRelative("lod").objectReferenceValue = nextObject;

                        this.needsUpdate = true;
                    }
                }

                GUI.enabled = true;

                GUI.color = Color.green;

                // Inserts a lod level
                if (GUILayout.Button(
                    new GUIContent("+", "Insert a new lod level before this one"), 
                    EditorStyles.miniButtonLeft, 
                    GUILayout.Width(22)))
                {
                    this.lodsProp.InsertArrayElementAtIndex(index);
                    index++;
                }

                GUI.color = Color.red;

                // Removes the lod level
                if (GUILayout.Button(
                    new GUIContent("X", "Remove this lod level"), EditorStyles.miniButtonRight, GUILayout.Width(22)))
                {
                    this.lodsProp.DeleteArrayElementAtIndex(index);
                }

                GUI.color = this.defaultColor;

                GUILayout.EndHorizontal();

                index++;
            }
        }

        /// <summary>
        /// The generate lod distances.
        /// </summary>
        private void GenerateLodDistances()
        {
            foreach (var t in this.targets.OfType<LodObjectBase>())
            {
                t.RecalculateDistances();
            }
        }

        /// <summary>
        /// The generate lod levels.
        /// </summary>
        private void GenerateLodLevels()
        {
            foreach (var t in this.targets.OfType<LodObjectBase>())
            {
                t.AutoAssignLodLevels();
            }
        }

        /// <summary>
        /// The on disable.
        /// </summary>
        private void OnDisable()
        {
            EditorPrefs.SetBool(IsHelperOpenKey, this.isHelperOpen);
            EditorPrefs.SetBool(IsGeneralOpenKey, this.isGeneralOpen);
            EditorPrefs.SetBool(IsLodsOpenKey, this.isLodsOpen);
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
            this.isLodsOpen = !EditorPrefs.HasKey(IsLodsOpenKey) || EditorPrefs.GetBool(IsLodsOpenKey);
            this.isEditorOpen = EditorPrefs.HasKey(IsEditorOpenKey) && EditorPrefs.GetBool(IsEditorOpenKey);
            this.isInformationOpen = EditorPrefs.HasKey(IsInformationOpenKey)
                                     && EditorPrefs.GetBool(IsInformationOpenKey);

            this.isStaticProp = this.serializedObject.FindProperty("isStatic");
            this.excludeFromManagerProp = this.serializedObject.FindProperty("excludeFromManager");
            this.overwriteGlobalSettingsProp = this.serializedObject.FindProperty("overwriteGlobalSettings");
            this.lodsProp = this.serializedObject.FindProperty("lods");
            this.distanceProp = this.serializedObject.FindProperty("relativeDistance");
            this.absoluteDistanceProp = this.serializedObject.FindProperty("absoluteDistance");
            this.currentLodLevelProp = this.serializedObject.FindProperty("currentLodLevel");
            this.additionalFields = new Dictionary<SerializedProperty, LodObjectAddFieldAttribute>();

            this.canCalculateDistances = true;
            this.canGenerateLodLevels = true;
            foreach (var t in this.targets.OfType<LodObjectBase>())
            {
                this.canCalculateDistances = this.canCalculateDistances && t.CanCalculateDistances;
                this.canGenerateLodLevels = this.canGenerateLodLevels && t.CanGenerateLodLevels;
            }

            var addFields =
                this.target.GetType()
                    .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(field => field.IsDefined(typeof(LodObjectAddFieldAttribute), true));
            this.isOverwriteVisible = false;
            this.isEditorVisible = false;
            this.isLodsVisible = false;

            foreach (var af in addFields)
            {
                var attributes =
                    af.GetCustomAttributes(typeof(LodObjectAddFieldAttribute), true) as LodObjectAddFieldAttribute[];
                if (attributes != null && attributes.Length > 0)
                {
                    this.additionalFields.Add(this.serializedObject.FindProperty(af.Name), attributes[0]);

                    if (attributes[0].FieldCategory == LodObjectAddFieldAttribute.Category.OverwriteGlobal)
                    {
                        this.isOverwriteVisible = true;
                    }

                    if (attributes[0].FieldCategory == LodObjectAddFieldAttribute.Category.Editor)
                    {
                        this.isEditorVisible = true;
                    }

                    if (attributes[0].FieldCategory == LodObjectAddFieldAttribute.Category.Lods)
                    {
                        this.isLodsVisible = true;
                    }
                }
            }

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