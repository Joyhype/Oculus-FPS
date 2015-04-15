// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GSHierarchyHelper.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The gs hierarchy helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System;
using System.Collections;
using System.Linq;

using QuickLod.GroupSwitch;

using UnityEditor;

using UnityEngine;

/// <summary>
/// The gs hierarchy helper.
/// </summary>
[AddComponentMenu("")]
public class GSHierarchyHelper : Editor
{
    #region Static Fields

    /// <summary>
    /// The group switch selector.
    /// </summary>
    private static GroupSwitchSelector groupSwitchSelector;

    /// <summary>
    /// The selected objects.
    /// </summary>
    private static GameObject[] selectedObjects;

    #endregion

    #region Methods

    /// <summary>
    /// The add to group switch.
    /// </summary>
    [MenuItem("GameObject/Quick Lod/Add to group switch", false, 20)]
    private static void AddToGroupSwitch()
    {
        // Can't select new objects when already applied
        if (selectedObjects != null)
        {
            return;
        }

        selectedObjects = Selection.gameObjects;

        groupSwitchSelector = GroupSwitchSelector.Create();
        groupSwitchSelector.SelectionApplied += GsSelectorOnSelectionApplied;
        groupSwitchSelector.WindowClosed += GsSelectorOnClose;
    }

    /// <summary>
    /// The gs selector on close.
    /// </summary>
    /// <param name="sender">
    /// The sender.
    /// </param>
    /// <param name="eventArgs">
    /// The event args.
    /// </param>
    private static void GsSelectorOnClose(object sender, EventArgs eventArgs)
    {
        groupSwitchSelector.SelectionApplied -= GsSelectorOnSelectionApplied;
        groupSwitchSelector.WindowClosed -= GsSelectorOnClose;
        selectedObjects = null;
        groupSwitchSelector = null;
    }

    /// <summary>
    /// The gs selector on selection applied.
    /// </summary>
    /// <param name="sender">
    /// The sender.
    /// </param>
    /// <param name="eventArgs">
    /// The event args.
    /// </param>
    private static void GsSelectorOnSelectionApplied(object sender, EventArgs eventArgs)
    {
        if (selectedObjects == null)
        {
            return;
        }

        var shouldAsk = true;

        var selectedGroupSwitch = groupSwitchSelector.SelectedGroupSwitch;
        if (selectedGroupSwitch == null)
        {
            if (EditorUtility.DisplayDialog(
                "No group switch selected.", 
                "You haven't selected a group switch.\nDo you want to remove the selected objects from all group switches?", 
                "Yes", 
                "No"))
            {
                shouldAsk = false;
            }
            else
            {
                return;
            }
        }

        RemoveSelectionFromAllGroupSwitches(shouldAsk);

        if (selectedGroupSwitch != null)
        {
            // Remove the objects from the group switch when already registered
            selectedGroupSwitch.ManagedObjects.RemoveAll(obj => selectedObjects.Contains(obj));
            selectedGroupSwitch.ManagedObjects.AddRange(selectedObjects);
        }

        selectedObjects = null;
    }

    /// <summary>
    /// The remove from group switch.
    /// </summary>
    [MenuItem("GameObject/Quick Lod/Remove from group switch", false, 20)]
    private static void RemoveFromGroupSwitch()
    {
        selectedObjects = Selection.gameObjects;
        RemoveSelectionFromAllGroupSwitches(false);

        foreach (var obj in selectedObjects)
        {
            obj.SetActive(true);
        }

        selectedObjects = null;
    }

    /// <summary>
    /// The remove selection from all group switches.
    /// </summary>
    /// <param name="shouldAsk">
    /// The should ask.
    /// </param>
    private static void RemoveSelectionFromAllGroupSwitches(bool shouldAsk)
    {
        // Get all group switches
        var groupSwitches = FindObjectsOfType(typeof(GsGroupSwitch)) as GsGroupSwitch[];
        if (groupSwitches == null)
        {
            return;
        }

        var shouldRemoveOld = true;

        // Look if any group switches already manages one of the objects
        foreach (var gs in groupSwitches)
        {
            if (!shouldAsk && !shouldRemoveOld)
            {
                break;
            }

            var objectsToRemove = gs.ManagedObjects.Where(obj => selectedObjects.Contains(obj)).ToList();
            if (!objectsToRemove.Any())
            {
                continue;
            }

            if (shouldAsk)
            {
                shouldRemoveOld = EditorUtility.DisplayDialog(
                    "Remove old dependencies?", 
                    "Some of the selected objects are already managed by a group switch.\nMultiple group switches managing the same object can lead to unforseen results.\n\nDo you want to remove them from the corresponding group switch?", 
                    "Yes", 
                    "No");
                shouldAsk = false;
            }

            if (shouldRemoveOld)
            {
                gs.ManagedObjects.RemoveAll(obj => selectedObjects.Contains(obj));
            }
        }
    }

    /// <summary>
    /// The validata selected object.
    /// </summary>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    [MenuItem("GameObject/Quick Lod/Add to group switch", true)]
    [MenuItem("GameObject/Quick Lod/Remove from group switch", true)]
    private static bool ValidataSelectedObject()
    {
        return Selection.gameObjects.Length > 0;
    }

    #endregion
}