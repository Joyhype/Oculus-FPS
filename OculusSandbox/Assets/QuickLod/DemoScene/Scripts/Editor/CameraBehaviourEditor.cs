// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CameraBehaviourEditor.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The camera behaviour editor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QLDemo.Editor
{
    using UnityEditor;

    using UnityEngine;

    /// <summary>
    /// The camera behaviour editor.
    /// </summary>
    [CustomEditor(typeof(CameraBehaviour))]
    [CanEditMultipleObjects]
    public class CameraBehaviourEditor : Editor
    {
        #region Fields

        /// <summary>
        ///     The absoulte horizontal distance prop.
        /// </summary>
        private SerializedProperty AbsoulteHorizontalDistanceProp;

        /// <summary>
        ///     The absoulte vertical distance prop.
        /// </summary>
        private SerializedProperty AbsoulteVerticalDistanceProp;

        /// <summary>
        ///     The allow camera collision prop.
        /// </summary>
        private SerializedProperty AllowCameraCollisionProp;

        /// <summary>
        ///     The allow x rotation prop.
        /// </summary>
        private SerializedProperty AllowXRotationProp;

        /// <summary>
        ///     The allow y rotation prop.
        /// </summary>
        private SerializedProperty AllowYRotationProp;

        /// <summary>
        ///     The allow zoom prop.
        /// </summary>
        private SerializedProperty AllowZoomProp;

        /// <summary>
        ///     The camera damping prop.
        /// </summary>
        private SerializedProperty CameraDampingProp;

        /// <summary>
        ///     The collision precission prop.
        /// </summary>
        private SerializedProperty CollisionPrecissionProp;

        /// <summary>
        ///     The collision radius prop.
        /// </summary>
        private SerializedProperty CollisionRadiusProp;

        /// <summary>
        ///     The current x rotation prop.
        /// </summary>
        private SerializedProperty CurrentXRotationProp;

        /// <summary>
        ///     The current y rotation prop.
        /// </summary>
        private SerializedProperty CurrentYRotationProp;

        /// <summary>
        ///     The current zoom prop.
        /// </summary>
        private SerializedProperty CurrentZoomProp;

        /// <summary>
        ///     The ignore layer prop.
        /// </summary>
        private SerializedProperty IgnoreLayerProp;

        /// <summary>
        ///     The lock cursor to center screen prop.
        /// </summary>
        private SerializedProperty LockCursorToCenterScreenProp;

        /// <summary>
        ///     The main camera prop.
        /// </summary>
        private SerializedProperty MainCameraProp;

        /// <summary>
        ///     The max horizontal distance prop.
        /// </summary>
        private SerializedProperty MaxHorizontalDistanceProp;

        /// <summary>
        ///     The max vertical distance prop.
        /// </summary>
        private SerializedProperty MaxVerticalDistanceProp;

        /// <summary>
        ///     The max x rotation prop.
        /// </summary>
        private SerializedProperty MaxXRotationProp;

        /// <summary>
        ///     The max y rotation prop.
        /// </summary>
        private SerializedProperty MaxYRotationProp;

        /// <summary>
        ///     The min horizontal distance prop.
        /// </summary>
        private SerializedProperty MinHorizontalDistanceProp;

        /// <summary>
        ///     The min vertical distance prop.
        /// </summary>
        private SerializedProperty MinVerticalDistanceProp;

        /// <summary>
        ///     The min x rotation prop.
        /// </summary>
        private SerializedProperty MinXRotationProp;

        /// <summary>
        ///     The min y rotation prop.
        /// </summary>
        private SerializedProperty MinYRotationProp;

        // X Rotation

        /// <summary>
        ///     The restrict x rotation prop.
        /// </summary>
        private SerializedProperty RestrictXRotationProp;

        /// <summary>
        ///     The restrict y rotation prop.
        /// </summary>
        private SerializedProperty RestrictYRotationProp;

        /// <summary>
        ///     The rotation speed prop.
        /// </summary>
        private SerializedProperty RotationSpeedProp;

        /// <summary>
        ///     The show collision calculation prop.
        /// </summary>
        private SerializedProperty ShowCollisionCalculationProp;

        /// <summary>
        ///     The show cursor prop.
        /// </summary>
        private SerializedProperty ShowCursorProp;

        /// <summary>
        ///     The triggered x rotation input axis names prop.
        /// </summary>
        private SerializedProperty TriggeredXRotationInputAxisNamesProp;

        /// <summary>
        ///     The triggered y rotation input axis names prop.
        /// </summary>
        private SerializedProperty TriggeredYRotationInputAxisNamesProp;

        /// <summary>
        ///     The triggered zoom input axis names prop.
        /// </summary>
        private SerializedProperty TriggeredZoomInputAxisNamesProp;

        /// <summary>
        ///     The x rotation input axis names prop.
        /// </summary>
        private SerializedProperty XRotationInputAxisNamesProp;

        /// <summary>
        ///     The x trigger key code prop.
        /// </summary>
        private SerializedProperty XTriggerKeyCodeProp;

        /// <summary>
        ///     The y rotation input axis names prop.
        /// </summary>
        private SerializedProperty YRotationInputAxisNamesProp;

        /// <summary>
        ///     The y trigger key code prop.
        /// </summary>
        private SerializedProperty YTriggerKeyCodeProp;

        /// <summary>
        ///     The zoom input axis names prop.
        /// </summary>
        private SerializedProperty ZoomInputAxisNamesProp;

        /// <summary>
        ///     The zoom speed prop.
        /// </summary>
        private SerializedProperty ZoomSpeedProp;

        /// <summary>
        ///     The zoom trigger key code prop.
        /// </summary>
        private SerializedProperty ZoomTriggerKeyCodeProp;

        /// <summary>
        ///     The previous tab prop.
        /// </summary>
        private SerializedProperty previousTabProp;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The on inspector gui.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            this.serializedObject.Update();

            // General settings
            EditorGUILayout.PropertyField(
                this.MainCameraProp, new GUIContent("Main Camera", "The camera that should be used."));

            GUILayout.Space(8.0f);

            this.ZoomSpeedProp.floatValue =
                EditorGUILayout.FloatField(
                    new GUIContent("Zoom Speed", "Define how fast you can zoom."), this.ZoomSpeedProp.floatValue);
            this.RotationSpeedProp.floatValue =
                EditorGUILayout.FloatField(
                    new GUIContent("Rotation Speed", "Define how fast you can rotate."), 
                    this.RotationSpeedProp.floatValue);
            this.CameraDampingProp.floatValue =
                EditorGUILayout.FloatField(
                    new GUIContent(
                        "Camera Damping", "Define how much damping the camera gets. Large values = less damping."), 
                    this.CameraDampingProp.floatValue);

            GUILayout.Space(8.0f);

            this.LockCursorToCenterScreenProp.boolValue =
                EditorGUILayout.Toggle(
                    new GUIContent("Lock cursor", "Lock the cursor to the center of the screen"), 
                    this.LockCursorToCenterScreenProp.boolValue);

            if (!this.LockCursorToCenterScreenProp.boolValue)
            {
                this.ShowCursorProp.boolValue =
                    EditorGUILayout.Toggle(
                        new GUIContent("Show cursor", "Should the cursor be visible in the game?"), 
                        this.ShowCursorProp.boolValue);
            }

            // Tabs
            GUILayout.Space(24.0f);
            this.previousTabProp.intValue = GUILayout.Toolbar(
                this.previousTabProp.intValue, 
                new[]
                    {
                        new GUIContent("Zoom", "Edit the camera zoom settings"), 
                        new GUIContent("X Rotation", "Edit the settings for the x rotation of the camera"), 
                        new GUIContent("Y Rotation", "Edit the settings for the y rotation of the camera"), 
                        new GUIContent("Collision", "Edit the camera collision settings")
                    });
            GUILayout.Space(8.0f);

            // Zoom
            switch (this.previousTabProp.intValue)
            {
                    // Zoom
                case 0:
                    {
                        this.AllowZoomProp.boolValue =
                            EditorGUILayout.Toggle(
                                new GUIContent("Allow Zoom", "Can the player zoom the camera?"), 
                                this.AllowZoomProp.boolValue);
                        GUILayout.Space(8);

                        if (this.AllowZoomProp.boolValue)
                        {
                            EditorGUILayout.Slider(
                                this.CurrentZoomProp, 
                                0.0f, 
                                1.0f, 
                                new GUIContent(
                                    "Current Zoom", "The current zoomlevel between max distance and min distance"));

                            GUILayout.Space(8);
                            EditorGUILayout.PropertyField(
                                this.ZoomInputAxisNamesProp, 
                                new GUIContent(
                                    "Input Axis", "Insert the name of the input axis that should be used for zoom"), 
                                true);
                            EditorGUILayout.PropertyField(
                                this.TriggeredZoomInputAxisNamesProp, 
                                new GUIContent(
                                    "Triggered Input Axis", 
                                    "Insert the name of the input axis that should be used for zoom only when the trigger key is pressed"), 
                                true);
                            EditorGUILayout.PropertyField(
                                this.ZoomTriggerKeyCodeProp, 
                                new GUIContent(
                                    "Trigger Keycode", 
                                    "Select which key need to be pressed, so that the \"Triggered Input Axis\" get used. Leave empty if not used."));

                            GUILayout.Space(8);
                            EditorGUILayout.LabelField(
                                new GUIContent(
                                    "Vertical relativeDistance", "Defines the vertical distances used for zoom."));
                            this.MinVerticalDistanceProp.floatValue =
                                Mathf.Max(
                                    EditorGUILayout.FloatField(
                                        new GUIContent("Min", "Defines the minimum vertical distance of the camera"), 
                                        this.MinVerticalDistanceProp.floatValue), 
                                    0.0f);
                            this.MaxVerticalDistanceProp.floatValue = Mathf.Max(
                                this.MaxVerticalDistanceProp.floatValue, this.MinVerticalDistanceProp.floatValue + 0.1f);
                            this.MaxVerticalDistanceProp.floatValue =
                                Mathf.Max(
                                    EditorGUILayout.FloatField(
                                        new GUIContent("Max", "Defines the maximum vertical distance of the camera"), 
                                        this.MaxVerticalDistanceProp.floatValue), 
                                    0.0f);
                            this.MinVerticalDistanceProp.floatValue =
                                Mathf.Min(
                                    this.MaxVerticalDistanceProp.floatValue - 0.1f, 
                                    this.MinVerticalDistanceProp.floatValue);

                            GUILayout.Space(8);
                            EditorGUILayout.LabelField(
                                new GUIContent(
                                    "Horizontal relativeDistance", "Defines the horizontal distances used for zoom."));
                            this.MinHorizontalDistanceProp.floatValue =
                                Mathf.Max(
                                    EditorGUILayout.FloatField(
                                        new GUIContent("Min", "Defines the minimum horizontal distance of the camera"), 
                                        this.MinHorizontalDistanceProp.floatValue), 
                                    0.0f);
                            this.MaxHorizontalDistanceProp.floatValue =
                                Mathf.Max(
                                    this.MaxHorizontalDistanceProp.floatValue, 
                                    this.MinHorizontalDistanceProp.floatValue + 0.1f);
                            this.MaxHorizontalDistanceProp.floatValue =
                                Mathf.Max(
                                    EditorGUILayout.FloatField(
                                        new GUIContent("Max", "Defines the maximum horizontal distance of the camera"), 
                                        this.MaxHorizontalDistanceProp.floatValue), 
                                    0.0f);
                            this.MinHorizontalDistanceProp.floatValue =
                                Mathf.Min(
                                    this.MaxHorizontalDistanceProp.floatValue - 0.1f, 
                                    this.MinHorizontalDistanceProp.floatValue);
                        }
                        else
                        {
                            this.AbsoulteVerticalDistanceProp.floatValue =
                                Mathf.Max(
                                    EditorGUILayout.FloatField(
                                        new GUIContent(
                                            "Vertical relativeDistance", 
                                            "Set the absolute vertical distance of the camera"), 
                                        this.AbsoulteVerticalDistanceProp.floatValue), 
                                    0.0f);
                            this.AbsoulteHorizontalDistanceProp.floatValue =
                                Mathf.Max(
                                    EditorGUILayout.FloatField(
                                        new GUIContent(
                                            "Horizontal relativeDistance", 
                                            "Set the absolute horizontal distance of the camera"), 
                                        this.AbsoulteHorizontalDistanceProp.floatValue), 
                                    0.0f);
                        }

                        break;
                    }

                    // X Rotation
                case 1:
                    {
                        this.AllowXRotationProp.boolValue =
                            EditorGUILayout.Toggle(
                                new GUIContent(
                                    "Allow X Rotation", "Can the player rotate the camera around the x axis?"), 
                                this.AllowXRotationProp.boolValue);
                        GUILayout.Space(8);

                        EditorGUILayout.Slider(
                            this.CurrentXRotationProp, 
                            0.0f, 
                            360.0f, 
                            new GUIContent("Current X Rotation", "Define the actual x rotation of the camera"));
                        GUILayout.Space(8);

                        if (this.AllowXRotationProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(
                                this.XRotationInputAxisNamesProp, 
                                new GUIContent(
                                    "Input Axis", "Insert the name of the input axis that should be used for x rotation"), 
                                true);
                            EditorGUILayout.PropertyField(
                                this.TriggeredXRotationInputAxisNamesProp, 
                                new GUIContent(
                                    "Triggered Input Axis", 
                                    "Insert the name of the input axis that should be used for x rotation only when the trigger key is pressed"), 
                                true);
                            EditorGUILayout.PropertyField(
                                this.XTriggerKeyCodeProp, 
                                new GUIContent(
                                    "Trigger Keycode", 
                                    "Select which key need to be pressed, so that the \"Triggered Input Axis\" get used. Leave empty if not used."));

                            GUILayout.Space(8);
                            this.RestrictXRotationProp.boolValue =
                                EditorGUILayout.Toggle(
                                    new GUIContent(
                                        "Restrict X Rotation", 
                                        "Restrict the rotation around the x axis to the min and max values"), 
                                    this.RestrictXRotationProp.boolValue);

                            GUILayout.Space(8);
                            if (this.RestrictXRotationProp.boolValue)
                            {
                                this.MinXRotationProp.floatValue =
                                    EditorGUILayout.FloatField(
                                        new GUIContent("Min", "Min x rotation"), this.MinXRotationProp.floatValue);
                                this.MaxXRotationProp.floatValue = Mathf.Max(
                                    this.MaxXRotationProp.floatValue, this.MinXRotationProp.floatValue + 0.1f);
                                this.MaxXRotationProp.floatValue =
                                    EditorGUILayout.FloatField(
                                        new GUIContent("Max", "Max x rotation"), this.MaxXRotationProp.floatValue);
                                this.MinXRotationProp.floatValue = Mathf.Min(
                                    this.MaxXRotationProp.floatValue - 0.1f, this.MinXRotationProp.floatValue);
                            }
                        }

                        break;
                    }

                    // Y Rotation
                case 2:
                    {
                        this.AllowYRotationProp.boolValue =
                            EditorGUILayout.Toggle(
                                new GUIContent(
                                    "Allow Y Rotation", "Can the player rotate the camera around the y axis?"), 
                                this.AllowYRotationProp.boolValue);
                        GUILayout.Space(8);

                        EditorGUILayout.Slider(
                            this.CurrentYRotationProp, 
                            0.0f, 
                            360.0f, 
                            new GUIContent("Current Y Rotation", "Define the actual y rotation of the camera"));
                        GUILayout.Space(8);

                        if (this.AllowYRotationProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(
                                this.YRotationInputAxisNamesProp, 
                                new GUIContent(
                                    "Input Axis", "Insert the name of the input axis that should be used for y rotation"), 
                                true);
                            EditorGUILayout.PropertyField(
                                this.TriggeredYRotationInputAxisNamesProp, 
                                new GUIContent(
                                    "Triggered Input Axis", 
                                    "Insert the name of the input axis that should be used for y rotation only when the trigger key is pressed"), 
                                true);
                            EditorGUILayout.PropertyField(
                                this.YTriggerKeyCodeProp, 
                                new GUIContent(
                                    "Trigger Keycode", 
                                    "Select which key need to be pressed, so that the \"Triggered Input Axis\" get used. Leave empty if not used."));

                            GUILayout.Space(8);
                            this.RestrictYRotationProp.boolValue =
                                EditorGUILayout.Toggle(
                                    new GUIContent(
                                        "Restrict Y Rotation", 
                                        "Restrict the rotation around the y axis to the min and max values"), 
                                    this.RestrictYRotationProp.boolValue);

                            GUILayout.Space(8);
                            if (this.RestrictYRotationProp.boolValue)
                            {
                                this.MinYRotationProp.floatValue =
                                    EditorGUILayout.FloatField(
                                        new GUIContent("Min", "Min y rotation"), this.MinYRotationProp.floatValue);
                                this.MaxYRotationProp.floatValue = Mathf.Max(
                                    this.MaxYRotationProp.floatValue, this.MinYRotationProp.floatValue + 0.1f);
                                this.MaxYRotationProp.floatValue =
                                    EditorGUILayout.FloatField(
                                        new GUIContent("Max", "Max y rotation"), this.MaxYRotationProp.floatValue);
                                this.MinYRotationProp.floatValue = Mathf.Min(
                                    this.MaxYRotationProp.floatValue - 0.1f, this.MinYRotationProp.floatValue);
                            }
                        }

                        break;
                    }

                    // Camera Collision
                case 3:
                    {
                        this.AllowCameraCollisionProp.boolValue =
                            EditorGUILayout.Toggle(
                                new GUIContent(
                                    "Allow Camera Collision", "Should the camera avoid collision with obstacles?"), 
                                this.AllowCameraCollisionProp.boolValue);
                        GUILayout.Space(8);

                        if (this.AllowCameraCollisionProp.boolValue)
                        {
                            this.CollisionRadiusProp.floatValue =
                                Mathf.Max(
                                    EditorGUILayout.FloatField(
                                        new GUIContent("Collision Radius", "The radius for the collision detection"), 
                                        this.CollisionRadiusProp.floatValue), 
                                    0.01f);

                            GUILayout.Space(8);
                            EditorGUILayout.IntSlider(
                                this.CollisionPrecissionProp, 
                                1, 
                                10, 
                                new GUIContent(
                                    "Collision Precission", 
                                    "Amount of substeps that should be used for the collision detection."));

                            GUILayout.Space(8);
                            EditorGUILayout.PropertyField(
                                this.IgnoreLayerProp, 
                                new GUIContent(
                                    "Ignore Layer(s)", "Which layers should be ignored for the collision detection"), 
                                true);

                            GUILayout.Space(8);
                            this.ShowCollisionCalculationProp.boolValue =
                                EditorGUILayout.Toggle(
                                    new GUIContent(
                                        "Visualize", "Should the collision calculation be shown in the editor?"), 
                                    this.ShowCollisionCalculationProp.boolValue);
                        }

                        break;
                    }
            }

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            this.serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The on enable.
        /// </summary>
        private void OnEnable()
        {
            // Initialize overview

            // General settings
            this.MainCameraProp = this.serializedObject.FindProperty("MainCamera");

            this.ZoomSpeedProp = this.serializedObject.FindProperty("ZoomSpeed");
            this.RotationSpeedProp = this.serializedObject.FindProperty("RotationSpeed");
            this.CameraDampingProp = this.serializedObject.FindProperty("CameraDamping");

            this.LockCursorToCenterScreenProp = this.serializedObject.FindProperty("LockCursorToCenterScreen");
            this.ShowCursorProp = this.serializedObject.FindProperty("ShowCursor");

            // Zoom
            this.AllowZoomProp = this.serializedObject.FindProperty("AllowZoom");
            this.ZoomInputAxisNamesProp = this.serializedObject.FindProperty("ZoomInputAxisNames");
            this.TriggeredZoomInputAxisNamesProp = this.serializedObject.FindProperty("TriggeredZoomInputAxisNames");
            this.ZoomTriggerKeyCodeProp = this.serializedObject.FindProperty("ZoomTriggerKeyCode");
            this.AbsoulteVerticalDistanceProp = this.serializedObject.FindProperty("AbsoluteVerticalDistance");
            this.AbsoulteHorizontalDistanceProp = this.serializedObject.FindProperty("AbsoluteHorizontalDistance");
            this.CurrentZoomProp = this.serializedObject.FindProperty("CurrentZoom");
            this.MaxVerticalDistanceProp = this.serializedObject.FindProperty("MaxVerticalDistance");
            this.MinVerticalDistanceProp = this.serializedObject.FindProperty("MinVerticalDistance");
            this.MaxHorizontalDistanceProp = this.serializedObject.FindProperty("MaxHorizontalDistance");
            this.MinHorizontalDistanceProp = this.serializedObject.FindProperty("MinHorizontalDistance");

            // X Rotation
            this.AllowXRotationProp = this.serializedObject.FindProperty("AllowXRotation");
            this.RestrictXRotationProp = this.serializedObject.FindProperty("RestrictXRotation");
            this.XRotationInputAxisNamesProp = this.serializedObject.FindProperty("XRotationInputAxisNames");
            this.TriggeredXRotationInputAxisNamesProp =
                this.serializedObject.FindProperty("TriggeredXRotationInputAxisNames");
            this.XTriggerKeyCodeProp = this.serializedObject.FindProperty("XTriggerKeyCode");
            this.CurrentXRotationProp = this.serializedObject.FindProperty("CurrentXRotation");
            this.MaxXRotationProp = this.serializedObject.FindProperty("MaxXRotation");
            this.MinXRotationProp = this.serializedObject.FindProperty("MinXRotation");

            // Y Rotation
            this.AllowYRotationProp = this.serializedObject.FindProperty("AllowYRotation");
            this.RestrictYRotationProp = this.serializedObject.FindProperty("RestrictYRotation");
            this.YRotationInputAxisNamesProp = this.serializedObject.FindProperty("YRotationInputAxisNames");
            this.TriggeredYRotationInputAxisNamesProp =
                this.serializedObject.FindProperty("TriggeredYRotationInputAxisNames");
            this.YTriggerKeyCodeProp = this.serializedObject.FindProperty("YTriggerKeyCode");
            this.CurrentYRotationProp = this.serializedObject.FindProperty("CurrentYRotation");
            this.MaxYRotationProp = this.serializedObject.FindProperty("MaxYRotation");
            this.MinYRotationProp = this.serializedObject.FindProperty("MinYRotation");

            // Camera Collision
            this.AllowCameraCollisionProp = this.serializedObject.FindProperty("AllowCameraCollision");
            this.CollisionRadiusProp = this.serializedObject.FindProperty("CollisionRadius");
            this.CollisionPrecissionProp = this.serializedObject.FindProperty("CollisionPrecission");
            this.ShowCollisionCalculationProp = this.serializedObject.FindProperty("ShowCollisionCalculation");
            this.IgnoreLayerProp = this.serializedObject.FindProperty("IgnoreLayer");

            this.previousTabProp = this.serializedObject.FindProperty("previousTabSelection");
        }

        #endregion
    }
}