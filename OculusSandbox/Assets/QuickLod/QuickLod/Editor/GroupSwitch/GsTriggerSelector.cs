// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GsTriggerSelector.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The interface selector.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod.Editor.GroupSwitch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using QuickLod.GroupSwitch;

    using UnityEditor;

    using UnityEngine;

    /// <summary>
    /// The interface selector.
    /// </summary>
    public sealed class GsTriggerSelector : EditorWindow
    {
        #region Fields

        /// <summary>
        /// The active list.
        /// </summary>
        public List<IGsTrigger> ActiveList;

        /// <summary>
        /// The all scene objects.
        /// </summary>
        private List<ObjContainer> allSceneObjects;

        /// <summary>
        /// The search string.
        /// </summary>
        private string searchString;

        #endregion

        #region Public Events

        /// <summary>
        /// The selection changed.
        /// </summary>
        public event EventHandler<EventArgs> SelectionChanged;

        /// <summary>
        /// The window closed.
        /// </summary>
        public event EventHandler<EventArgs> WindowClosed;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="selectedItems">
        /// The selected items.
        /// </param>
        /// <returns>
        /// The <see cref="GsTriggerSelector"/>.
        /// </returns>
        public static GsTriggerSelector Create(ref List<IGsTrigger> selectedItems)
        {
            var newWindow = (GsTriggerSelector)GetWindow(typeof(GsTriggerSelector), true, "Select");
            newWindow.allSceneObjects = new List<ObjContainer>();
            newWindow.ActiveList = selectedItems;
            newWindow.searchString = string.Empty;

            var allImplementedTypes = FindInterfaceImplementations<IGsTrigger>();
            var foundBehaviours = new List<MonoBehaviour>();

            // Get all objects for all supported types
            foreach (var impType in allImplementedTypes)
            {
                foundBehaviours.AddRange(FindObjectsOfType(impType).OfType<MonoBehaviour>());
            }

            foreach (var fb in foundBehaviours)
            {
                var foundT = (IGsTrigger)fb;
                var type = fb.GetType();
                newWindow.allSceneObjects.Add(
                    new ObjContainer
                        {
                            IsSelected = selectedItems.Any(si => si.Equals(fb)), 
                            Name = fb.name, 
                            type = type, 
                            Object = foundT
                        });
            }

            var rect = new Rect(100, 100, 400, 500);
            newWindow.ShowAsDropDown(rect, new Vector2(400, 500));

            return newWindow;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The find interface implementations.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        private static IEnumerable<Type> FindInterfaceImplementations<T>()
        {
            return
                Assembly.GetAssembly(typeof(T))
                        .GetTypes()
                        .Where(type => type.IsClass && !type.IsAbstract && typeof(T).IsAssignableFrom(type))
                        .ToArray();
        }

        /// <summary>
        /// The on destroy.
        /// </summary>
        private void OnDestroy()
        {
            this.OnWindowClosed();
        }

        /// <summary>
        /// The on gui.
        /// </summary>
        private void OnGUI()
        {
            GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
            this.searchString =
                EditorGUILayout.TextField(this.searchString, GUI.skin.FindStyle("ToolbarSeachTextField"))
                               .ToLowerInvariant();
            if (GUILayout.Button(string.Empty, GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                // Remove focus if cleared
                this.searchString = string.Empty;
                GUI.FocusControl(null);
            }

            GUILayout.EndHorizontal();

            var columnWidth = (this.position.width - 30f) / 2f;
            var objects =
                this.allSceneObjects.Where(
                    obj =>
                    obj.Name.ToLowerInvariant().Contains(this.searchString)
                    || obj.Object.Name.ToLowerInvariant().Contains(this.searchString))
                    .OrderBy(obj => obj.Name)
                    .ToArray();

            // Draw list 
            foreach (var sceneObject in objects)
            {
                EditorGUILayout.BeginHorizontal();

                var prevIsSelected = sceneObject.IsSelected;
                sceneObject.IsSelected = EditorGUILayout.Toggle(sceneObject.IsSelected, GUILayout.Width(20));
                EditorGUILayout.LabelField(sceneObject.Name, GUILayout.Width(columnWidth));
                EditorGUILayout.LabelField(":", GUILayout.Width(10));
                EditorGUILayout.LabelField(sceneObject.Object.Name, GUILayout.Width(columnWidth));

                EditorGUILayout.EndHorizontal();

                if (prevIsSelected != sceneObject.IsSelected)
                {
                    if (prevIsSelected == false)
                    {
                        this.ActiveList.Add(sceneObject.Object);
                    }
                    else
                    {
                        this.ActiveList.Remove(sceneObject.Object);
                    }

                    this.OnSelectionChanged();
                }
            }
        }

        /// <summary>
        /// The on inspector update.
        /// </summary>
        private void OnInspectorUpdate()
        {
            this.Repaint();
        }

        /// <summary>
        /// The on selection changed.
        /// </summary>
        private void OnSelectionChanged()
        {
            var handler = this.SelectionChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The on window closed.
        /// </summary>
        private void OnWindowClosed()
        {
            var handler = this.WindowClosed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion
    }

    /// <summary>
    /// The obj container.
    /// </summary>
    internal class ObjContainer
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether is selected.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the object.
        /// </summary>
        public IGsTrigger Object { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public Type type { get; set; }

        #endregion
    }
}