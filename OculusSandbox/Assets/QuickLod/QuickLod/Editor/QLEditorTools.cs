// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QLEditorTools.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The ql editor tools.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System;
using System.Collections;
using System.Reflection;

using UnityEditor;

using UnityEngine;

/// <summary>
/// The ql editor tools.
/// </summary>
public static class QLEditorTools
{
    #region Public Methods and Operators

    /// <summary>
    /// Gets the background object from an serialized property
    /// </summary>
    /// <param name="prop">
    /// The prop.
    /// </param>
    /// <returns>
    /// The <see cref="object"/>.
    /// </returns>
    public static object GetObject(SerializedProperty prop)
    {
        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements)
        {
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index =
                    Convert.ToInt32(
                        element.Substring(element.IndexOf("[")).Replace("[", string.Empty).Replace("]", string.Empty));
                obj = GetValue(obj, elementName, index);
            }
            else
            {
                obj = GetValue(obj, element);
            }
        }

        return obj;
    }

    #endregion

    #region Methods

    /// <summary>
    /// The get value.
    /// </summary>
    /// <param name="source">
    /// The source.
    /// </param>
    /// <param name="name">
    /// The name.
    /// </param>
    /// <returns>
    /// The <see cref="object"/>.
    /// </returns>
    private static object GetValue(object source, string name)
    {
        if (source == null)
        {
            return null;
        }

        var type = source.GetType();
        var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (f == null)
        {
            var p = type.GetProperty(
                name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p == null)
            {
                return null;
            }

            return p.GetValue(source, null);
        }

        return f.GetValue(source);
    }

    /// <summary>
    /// The get value.
    /// </summary>
    /// <param name="source">
    /// The source.
    /// </param>
    /// <param name="name">
    /// The name.
    /// </param>
    /// <param name="index">
    /// The index.
    /// </param>
    /// <returns>
    /// The <see cref="object"/>.
    /// </returns>
    private static object GetValue(object source, string name, int index)
    {
        var enumerable = GetValue(source, name) as IEnumerable;
        var enm = enumerable.GetEnumerator();
        while (index-- >= 0)
        {
            enm.MoveNext();
        }

        return enm.Current;
    }

    #endregion
}