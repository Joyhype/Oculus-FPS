// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GSEditor.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The gs editor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using QuickLod.Editor.GroupSwitch;
    using QuickLod.GroupSwitch;

    using UnityEditor;

    using UnityEngine;

    using Editor = UnityEditor.Editor;
    using Object = UnityEngine.Object;

    /// <summary>
    /// The gs editor.
    /// </summary>
    [CustomEditor(typeof(GsGroupSwitch))]
    public class GSEditor : Editor
    {
        #region Constants

        /// <summary>
        /// The help text to show in the help tab
        /// </summary>
        private const string HelpText = @"This is a group switch.
It activates and deactivates all managed objects depending on the trigger input.
You need to define which objects it should manage and which triggers it should use.

Also you can set a deactivation delay to define how long the group switch should wait with deactivating the managed objects after it's no longer triggered.

If you want to force the groupswitch to use a custom trigger state, you can use the ""Force trigger state"" property.

To quickly add managed objects, right click them in the hierarchy and select ""Quick Lod/Add to group switch"".";

        /// <summary>
        /// The key to store a value indicating whether the editor tab was opened or not
        /// </summary>
        private const string IsEditorOpenKey = "QLGS_GSEditor_IsEditorOpen";

        /// <summary>
        /// The key to store a value indicating whether the general tab was opened or not
        /// </summary>
        private const string IsGeneralOpenKey = "QLGS_GSEditor_IsGeneralOpen";

        /// <summary>
        /// The key to store a value indicating whether the helper tab was opened or not
        /// </summary>
        private const string IsHelperOpenKey = "QLGS_GSEditor_IsHelperOpen";

        /// <summary>
        /// The key to store a value indicating whether the information tab was opened or not
        /// </summary>
        private const string IsInfoOpenKey = "QLGS_GSEditor_IsInformationOpen";

        /// <summary>
        /// The key to store a value indicating whether the trigger tab was opened or not
        /// </summary>
        private const string IsTriggersOpenKey = "QLGS_GSEditor_IsTriggersOpen";

        #endregion

        #region Fields

        /// <summary>
        /// The deactivation delay prop.
        /// </summary>
        private SerializedProperty deactivationDelayProp;

        /// <summary>
        /// The draw child lines prop.
        /// </summary>
        private SerializedProperty drawChildLinesProp;

        /// <summary>
        /// The draw lines prop.
        /// </summary>
        private SerializedProperty drawLinesProp;

        /// <summary>
        /// The draw when deselected prop.
        /// </summary>
        private SerializedProperty drawWhenDeselectedProp;

        /// <summary>
        /// The force trigger state prop.
        /// </summary>
        private SerializedProperty forceTriggerStateProp;

        /// <summary>
        /// The forced state prop.
        /// </summary>
        private SerializedProperty forcedStateProp;

        /// <summary>
        /// The gs trigger selector.
        /// </summary>
        private GsTriggerSelector gsTriggerSelector;

        /// <summary>
        /// The highlight managed objects prop.
        /// </summary>
        private SerializedProperty highlightManagedObjectsProp;

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

        /// <summary>
        /// The is triggers open.
        /// </summary>
        private bool isTriggersOpen;

        /// <summary>
        /// The object line color prop.
        /// </summary>
        private SerializedProperty objectLineColorProp;

        /// <summary>
        /// The managed objects prop.
        /// </summary>
        private SerializedProperty managedObjectsProp;

        /// <summary>
        /// The selection material prop.
        /// </summary>
        private SerializedProperty selectionMaterialProp;

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

                this.deactivationDelayProp.floatValue =
                    EditorGUILayout.FloatField(
                        new GUIContent(
                            "Deactivation delay", 
                            "The time to wait before deactivating all objects after that no trigger is triggering anymore."), 
                        this.deactivationDelayProp.floatValue);

                this.forceTriggerStateProp.boolValue =
                    EditorGUILayout.Toggle(
                        new GUIContent(
                            "Force trigger state", 
                            "Set this field if you want to force a certain trigger state.\nThis overrides all trigger input."), 
                        this.forceTriggerStateProp.boolValue);

                if (this.forceTriggerStateProp.boolValue)
                {
                    this.forcedStateProp.boolValue =
                        EditorGUILayout.Toggle(
                            new GUIContent("Forced state", "Set the trigger state for this group switch."), 
                            this.forcedStateProp.boolValue);
                }

                this.isTriggersOpen = EditorGUILayout.Foldout(this.isTriggersOpen, "Triggers");
                if (this.isTriggersOpen)
                {
                    EditorGUI.indentLevel++;
                    this.DrawTriggerList();
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(
                    this.managedObjectsProp, 
                    new GUIContent("Managed objects", "Define here which objects are managed by the group switch."), 
                    true);

                EditorGUI.indentLevel--;

                GUILayout.Space(8);
            }

            this.isEditorOpen = EditorGUILayout.Foldout(this.isEditorOpen, "Editor settings");
            if (this.isEditorOpen)
            {
                EditorGUI.indentLevel++;

                this.drawLinesProp.boolValue =
                    EditorGUILayout.Toggle(
                        new GUIContent("Draw lines", "Draws lines from the group switch to all managed objects"), 
                        this.drawLinesProp.boolValue);

                if (this.drawLinesProp.boolValue)
                {
                    this.drawChildLinesProp.boolValue =
                        EditorGUILayout.Toggle(
                            new GUIContent(
                                "Draw child lines", 
                                "Also draw lines from all managed objects to their children and subchildren"), 
                            this.drawChildLinesProp.boolValue);

                    this.objectLineColorProp.colorValue =
                        EditorGUILayout.ColorField(
                            new GUIContent("Line color", "Define the color with which the lines should be drawn"), 
                            this.objectLineColorProp.colorValue);

                    GUILayout.Space(6);
                }

                this.highlightManagedObjectsProp.boolValue =
                    EditorGUILayout.Toggle(
                        new GUIContent(
                            "Highlight managed objects", "Highlights all objects which are managed by this group switch"), 
                        this.highlightManagedObjectsProp.boolValue);

                if (this.highlightManagedObjectsProp.boolValue)
                {
                    EditorGUILayout.PropertyField(
                        this.selectionMaterialProp, 
                        new GUIContent("Highlight material", "Define the material to use to highlight the objects"));

                    GUILayout.Space(6);
                }

                this.drawWhenDeselectedProp.boolValue =
                    EditorGUILayout.Toggle(
                        new GUIContent(
                            "Draw when deselected", "Also draw the gizmos when this group switch is not selected"), 
                        this.drawWhenDeselectedProp.boolValue);

                EditorGUI.indentLevel--;

                GUILayout.Space(8);
            }

            this.isInfoOpen = EditorGUILayout.Foldout(this.isInfoOpen, "Informations");
            if (this.isInfoOpen)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.Toggle(
                    new GUIContent("Is triggered", "Shows you whether this group switch is triggered"), 
                    this.isTriggeredProp.boolValue);

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
        /// The draw trigger list.
        /// </summary>
        private void DrawTriggerList()
        {
            var groupSwitch = this.target as GsGroupSwitch;

            if (groupSwitch == null)
            {
                return;
            }

            var availableWidth = Screen.width;
            var textWidth = (availableWidth - 217) / 2f;

            for (int index = 0; index < groupSwitch.TriggerObjects.Count; index++)
            {
                var trigger = groupSwitch.TriggerObjects[index];
                if (trigger == null)
                {
                    continue;
                }

                var baseTrigger = trigger as IGsTrigger;

                EditorGUILayout.BeginHorizontal();

                GUILayout.Space(14);

                EditorGUILayout.Toggle(baseTrigger.IsTriggered, GUILayout.Width(10));
                EditorGUILayout.LabelField(new GUIContent(trigger.name), GUILayout.Width(textWidth));
                EditorGUILayout.LabelField(new GUIContent(":"), GUILayout.Width(8));
                EditorGUILayout.LabelField(new GUIContent(baseTrigger.Name), GUILayout.Width(textWidth));

                if (GUILayout.Button("Select", EditorStyles.miniButtonLeft, GUILayout.Width(75)))
                {
                    if (!Selection.Contains(trigger))
                    {
                        Selection.objects = new Object[] { trigger.gameObject };
                    }
                }

                if (GUILayout.Button("Remove", EditorStyles.miniButtonRight, GUILayout.Width(75)))
                {
                    groupSwitch.Triggers.Remove(trigger as IGsTrigger);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (
                GUILayout.Button(
                    new GUIContent("Select triggers", "Select the triggers you want to use for this group switch.")))
            {
                var triggers = groupSwitch.Triggers;
                this.gsTriggerSelector = GsTriggerSelector.Create(ref triggers);
                this.gsTriggerSelector.SelectionChanged += this.GsTriggerSelectorOnSelectionChanged;
                this.gsTriggerSelector.WindowClosed += this.GsTriggerSelectorOnWindowClosed;
            }
        }

        /// <summary>
        /// The gs trigger selector on selection changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        private void GsTriggerSelectorOnSelectionChanged(object sender, EventArgs eventArgs)
        {
            var groupSwitch = this.target as GsGroupSwitch;
            if (groupSwitch == null)
            {
                return;
            }

            groupSwitch.Triggers = this.gsTriggerSelector.ActiveList;
            this.Repaint();
        }

        /// <summary>
        /// The gs trigger selector on window closed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        private void GsTriggerSelectorOnWindowClosed(object sender, EventArgs eventArgs)
        {
            this.gsTriggerSelector.SelectionChanged -= this.GsTriggerSelectorOnSelectionChanged;
            this.gsTriggerSelector.WindowClosed -= this.GsTriggerSelectorOnWindowClosed;
        }

        /// <summary>
        /// The on disable.
        /// </summary>
        private void OnDisable()
        {
            EditorPrefs.SetBool(IsHelperOpenKey, this.isHelperOpen);
            EditorPrefs.SetBool(IsGeneralOpenKey, this.isGeneralOpen);
            EditorPrefs.SetBool(IsEditorOpenKey, this.isEditorOpen);
            EditorPrefs.SetBool(IsTriggersOpenKey, this.isTriggersOpen);
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
            this.isTriggersOpen = EditorPrefs.HasKey(IsTriggersOpenKey) && EditorPrefs.GetBool(IsTriggersOpenKey);
            this.isInfoOpen = EditorPrefs.HasKey(IsInfoOpenKey) && EditorPrefs.GetBool(IsInfoOpenKey);

            this.deactivationDelayProp = this.serializedObject.FindProperty("deactivationDelay");
            this.forceTriggerStateProp = this.serializedObject.FindProperty("forceTriggerState");
            this.forcedStateProp = this.serializedObject.FindProperty("forcedState");
            this.managedObjectsProp = this.serializedObject.FindProperty("managedObjects");
            this.isTriggeredProp = this.serializedObject.FindProperty("isTriggered");
            this.objectLineColorProp = this.serializedObject.FindProperty("objectLineColor");
            this.selectionMaterialProp = this.serializedObject.FindProperty("selectionMaterial");
            this.drawLinesProp = this.serializedObject.FindProperty("drawLines");
            this.drawChildLinesProp = this.serializedObject.FindProperty("drawChildLines");
            this.highlightManagedObjectsProp = this.serializedObject.FindProperty("highlightManagedObjects");
            this.drawWhenDeselectedProp = this.serializedObject.FindProperty("drawWhenDeselected");
        }

        #endregion
    }
}