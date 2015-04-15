// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroupSwitchSelector.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The group switch selector.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System;
using System.Linq;

using QuickLod.GroupSwitch;

using UnityEditor;

using UnityEngine;

/// <summary>
/// The group switch selector.
/// </summary>
public class GroupSwitchSelector : EditorWindow
{
    #region Fields

    /// <summary>
    /// The scrol position.
    /// </summary>
    private Vector2 scrolPosition;

    /// <summary>
    /// The search string.
    /// </summary>
    private string searchString;

    /// <summary>
    /// The selection index.
    /// </summary>
    private int selectionIndex;

    #endregion

    #region Public Events

    /// <summary>
    /// The selection applied.
    /// </summary>
    public event EventHandler<EventArgs> SelectionApplied;

    /// <summary>
    /// The window closed.
    /// </summary>
    public event EventHandler<EventArgs> WindowClosed;

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the selected group switch.
    /// </summary>
    public GsGroupSwitch SelectedGroupSwitch { get; private set; }

    #endregion

    #region Public Methods and Operators

    /// <summary>
    /// The create.
    /// </summary>
    /// <returns>
    /// The <see cref="GroupSwitchSelector"/>.
    /// </returns>
    public static GroupSwitchSelector Create()
    {
        var newWindow = (GroupSwitchSelector)GetWindow(typeof(GroupSwitchSelector), true, "Select group switch");
        var rect = new Rect(100, 100, 400, 500);
        newWindow.ShowAsDropDown(rect, new Vector2(400, 500));

        newWindow.scrolPosition = new Vector2();
        newWindow.selectionIndex = -1;
        newWindow.searchString = string.Empty;

        return newWindow;
    }

    #endregion

    #region Methods

    /// <summary>
    /// The on selection applied.
    /// </summary>
    protected virtual void OnSelectionApplied()
    {
        var handler = this.SelectionApplied;
        if (handler != null)
        {
            handler(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// The on window closed.
    /// </summary>
    protected virtual void OnWindowClosed()
    {
        var handler = this.WindowClosed;
        if (handler != null)
        {
            handler(this, EventArgs.Empty);
        }
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
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        this.searchString = EditorGUILayout.TextField(this.searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
        if (GUILayout.Button(string.Empty, GUI.skin.FindStyle("ToolbarSeachCancelButton")))
        {
            // Remove focus if cleared
            this.searchString = string.Empty;
            GUI.FocusControl(null);
        }

        GUILayout.EndHorizontal();

        var groupSwitches = FindObjectsOfType(typeof(GsGroupSwitch)) as GsGroupSwitch[];
        if (groupSwitches == null)
        {
            return;
        }

        var data =
            groupSwitches.Where(gs => gs.name.ToLowerInvariant().Contains(this.searchString))
                         .Select(gs => gs.name)
                         .ToArray();

        var maxScrollHeight = Screen.height - 35;
        this.scrolPosition = EditorGUILayout.BeginScrollView(
            this.scrolPosition, false, false, GUILayout.Height(maxScrollHeight));
        this.selectionIndex = GUILayout.SelectionGrid(this.selectionIndex, data, 1);
        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
        GUILayout.Label(this.selectionIndex == -1 ? "Select group switch" : data[this.selectionIndex]);
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Ok", EditorStyles.toolbarButton, GUILayout.Width(75)))
        {
            if (this.selectionIndex >= 0)
            {
                this.SelectedGroupSwitch = groupSwitches[this.selectionIndex];
            }

            this.OnSelectionApplied();
            this.Close();
        }

        if (GUILayout.Button("Cancel", EditorStyles.toolbarButton, GUILayout.Width(75)))
        {
            this.Close();
        }

        EditorGUILayout.EndHorizontal();
    }

    #endregion
}