// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CTEditor.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The custom editor for a collision trigger
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod.Editor.GroupSwitch
{
    using System.Globalization;
    using System.Linq;

    using QuickLod.GroupSwitch;

    using UnityEditor;

    using UnityEngine;

    /// <summary>
    /// The custom editor for a collision trigger
    /// </summary>
    [CustomEditor(typeof(GsCollisionTrigger))]
    public class CTEditor : Editor
    {
        #region Constants

        /// <summary>
        /// The help text to show in the help tab
        /// </summary>
        private const string HelpText = @"This is a collision trigger.
It will trigger when a suitable collider enters a collider or child collider of this game object.

The colliders on this object need to be defined as triggers!

You can define on which layer the other colliders need to be for causing a trigger signal.
Also you can defing which tags the other colliders need to have for causing a trigger signal

This trigger doesn't work always in the editor, this is releated to the editor physic handling.";

        /// <summary>
        /// The key to store a value indicating whether the editor tab was opened or not
        /// </summary>
        private const string IsEditorOpenKey = "QLGS_CTEditor_IsEditorOpen";

        /// <summary>
        /// The key to store a value indicating whether the general tab was opened or not
        /// </summary>
        private const string IsGeneralOpenKey = "QLGS_CTEditor_IsGeneralOpen";

        /// <summary>
        /// The key to store a value indicating whether the helper tab was opened or not
        /// </summary>
        private const string IsHelperOpenKey = "QLGS_CTEditor_IsHelperOpen";

        /// <summary>
        /// The key to store a value indicating whether the information tab was opened or not
        /// </summary>
        private const string IsInfoOpenKey = "QLGS_CTEditor_IsInformationOpen";

        #endregion

        #region Fields

        /// <summary>
        /// The active trigger material prop.
        /// </summary>
        private SerializedProperty activeTriggerMaterialProp;

        /// <summary>
        /// The considered layers prop.
        /// </summary>
        private SerializedProperty consideredLayersProp;

        /// <summary>
        /// The considered tags prop.
        /// </summary>
        private SerializedProperty consideredTagsProp;

        /// <summary>
        /// The draw colliders prop.
        /// </summary>
        private SerializedProperty drawCollidersProp;

        /// <summary>
        /// The draw in debug prop.
        /// </summary>
        private SerializedProperty drawInDebugProp;

        /// <summary>
        /// The draw when deselected prop.
        /// </summary>
        private SerializedProperty drawWhenDeselectedProp;

        /// <summary>
        /// The filter by layers prop.
        /// </summary>
        private SerializedProperty filterByLayersProp;

        /// <summary>
        /// The filter by tags prop.
        /// </summary>
        private SerializedProperty filterByTagsProp;

        /// <summary>
        /// The inactive trigger material prop.
        /// </summary>
        private SerializedProperty inactiveTriggerMaterialProp;

        /// <summary>
        /// The is editor open.
        /// </summary>
        private bool isEditorOpen;

        /// <summary>
        /// The is general open.
        /// </summary>
        private bool isGeneralOpen;

        /// <summary>
        /// The is helper open.
        /// </summary>
        private bool isHelperOpen;

        /// <summary>
        /// The is info open.
        /// </summary>
        private bool isInfoOpen;

        /// <summary>
        /// The is triggered prop.
        /// </summary>
        private SerializedProperty isTriggeredProp;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The on inspector gui.
        /// </summary>
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            // Draw the help file
            this.isHelperOpen = EditorGUILayout.Foldout(this.isHelperOpen, "Component help");
            if (this.isHelperOpen)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.HelpBox(HelpText, MessageType.Info);

                GUILayout.Space(8);
                EditorGUI.indentLevel--;
            }

            this.isGeneralOpen = EditorGUILayout.Foldout(this.isGeneralOpen, "General settings");
            if (this.isGeneralOpen)
            {
                EditorGUI.indentLevel++;

                this.filterByLayersProp.boolValue =
                    EditorGUILayout.Toggle(
                        new GUIContent(
                            "Filter by layers", "Define if the colliders should be filtered with a layer mask"), 
                        this.filterByLayersProp.boolValue);

                if (this.filterByLayersProp.boolValue)
                {
                    EditorGUILayout.PropertyField(
                        this.consideredLayersProp, 
                        new GUIContent("Considered layers", "Define the layers which are considered"));

                    if (this.consideredLayersProp.intValue == 0)
                    {
                        EditorGUILayout.HelpBox(
                            "Setting the considered layers to \"Nothing\" prevents any collision detection.\nConsider enabling some layers or disable this trigger instead to prevent CPU overhead.", 
                            MessageType.Warning);
                    }

                    GUILayout.Space(6);
                }

                this.filterByTagsProp.boolValue =
                    EditorGUILayout.Toggle(
                        new GUIContent(
                            "Filter by tags", "Define if only colliders with certain tags should be considered"), 
                        this.filterByTagsProp.boolValue);

                if (this.filterByTagsProp.boolValue)
                {
                    EditorGUILayout.PropertyField(
                        this.consideredTagsProp, 
                        new GUIContent(
                            "Considered tags (" + this.consideredTagsProp.arraySize + ")", 
                            "Define the tags which are considered"), 
                        true);

                    if (this.consideredTagsProp.arraySize == 0)
                    {
                        EditorGUILayout.HelpBox(
                            "There are no considered tags, which prevents any collision detection.\nConsider adding some tags or disable this trigger instead to prevent CPU overhead.", 
                            MessageType.Warning);
                    }
                }

                GUILayout.Space(8);
                EditorGUI.indentLevel--;
            }

            this.isEditorOpen = EditorGUILayout.Foldout(this.isEditorOpen, "Editor settings");
            if (this.isEditorOpen)
            {
                EditorGUI.indentLevel++;

                this.drawCollidersProp.boolValue =
                    EditorGUILayout.Toggle(
                        new GUIContent(
                            "Draw colliders", 
                            "Draws all colliders which are considered by this trigger\nThe used material depends on the trigger state."), 
                        this.drawCollidersProp.boolValue);

                if (this.drawCollidersProp.boolValue)
                {
                    this.drawWhenDeselectedProp.boolValue =
                        EditorGUILayout.Toggle(
                            new GUIContent(
                                "Draw when deselected", "Also draws the colliders when this component is not selected."), 
                            this.drawWhenDeselectedProp.boolValue);

                    this.drawInDebugProp.boolValue =
                        EditorGUILayout.Toggle(
                            new GUIContent(
                                "Draw in debug", 
                                "Enable this option if you want to see the colliders in the game view to.\nThis won't affect the released game."), 
                            this.drawInDebugProp.boolValue);

                    EditorGUILayout.PropertyField(
                        this.activeTriggerMaterialProp, 
                        new GUIContent("Active trigger material", "The material to use when this trigger is triggered"));

                    EditorGUILayout.PropertyField(
                        this.inactiveTriggerMaterialProp, 
                        new GUIContent(
                            "Inactive trigger material", "The material to use when this trigger is not triggered"));
                }

                GUILayout.Space(8);
                EditorGUI.indentLevel--;
            }

            this.isInfoOpen = EditorGUILayout.Foldout(this.isInfoOpen, "Informations");
            if (this.isInfoOpen)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.Toggle(
                    new GUIContent("Is triggered", "Shows you if this component is triggered or not."), 
                    this.isTriggeredProp.boolValue);

                if (this.targets.Length == 1)
                {
                    var ct = this.target as GsCollisionTrigger;
                    if (ct != null)
                    {
                        var colliders =
                            ct.gameObject.GetComponentsInChildren(typeof(Collider)).OfType<Collider>().ToArray();
                        var usedAmount = 0;
                        var unusedAmount = 0;

                        foreach (var col in colliders)
                        {
                            if (col.enabled && col.isTrigger)
                            {
                                usedAmount++;
                            }
                            else
                            {
                                unusedAmount++;
                            }
                        }

                        EditorGUILayout.LabelField(
                            new GUIContent(
                                "Usable colliders", 
                                "Shows you the amount of colliders on this gameobject and it's children which are considered by this trigger"), 
                            new GUIContent(usedAmount.ToString(CultureInfo.InvariantCulture)));

                        EditorGUILayout.LabelField(
                            new GUIContent(
                                "Not usable colliders", 
                                "Shows you the amount of colliders on this gameobject and it's children which are NOT considered by this trigger\n\nHint:\nOnly enabled colliders with IsTrigger set to true are considered!"), 
                            new GUIContent(unusedAmount.ToString(CultureInfo.InvariantCulture)));
                    }
                }

                GUILayout.Space(8);
                EditorGUI.indentLevel--;
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
            EditorPrefs.SetBool(IsInfoOpenKey, this.isInfoOpen);
        }

        /// <summary>
        /// The on enable.
        /// </summary>
        private void OnEnable()
        {
            this.isHelperOpen = EditorPrefs.HasKey(IsHelperOpenKey) && EditorPrefs.GetBool(IsHelperOpenKey);
            this.isGeneralOpen = !EditorPrefs.HasKey(IsGeneralOpenKey) || EditorPrefs.GetBool(IsGeneralOpenKey);
            this.isEditorOpen = !EditorPrefs.HasKey(IsEditorOpenKey) || EditorPrefs.GetBool(IsEditorOpenKey);
            this.isInfoOpen = EditorPrefs.HasKey(IsInfoOpenKey) && EditorPrefs.GetBool(IsInfoOpenKey);

            this.consideredLayersProp = this.serializedObject.FindProperty("consideredLayers");
            this.consideredTagsProp = this.serializedObject.FindProperty("consideredTags");
            this.filterByLayersProp = this.serializedObject.FindProperty("filterByLayers");
            this.filterByTagsProp = this.serializedObject.FindProperty("filterByTags");
            this.isTriggeredProp = this.serializedObject.FindProperty("isTriggered");
            this.drawCollidersProp = this.serializedObject.FindProperty("drawColliders");
            this.drawWhenDeselectedProp = this.serializedObject.FindProperty("drawWhenDeselected");
            this.drawInDebugProp = this.serializedObject.FindProperty("drawInDebug");
            this.activeTriggerMaterialProp = this.serializedObject.FindProperty("activeTriggerMaterial");
            this.inactiveTriggerMaterialProp = this.serializedObject.FindProperty("inactiveTriggerMaterial");
        }

        #endregion
    }
}