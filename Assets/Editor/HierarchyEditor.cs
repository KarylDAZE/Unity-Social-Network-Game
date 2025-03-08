using UnityEditor;
using UnityEngine;

/// <summary>
/// Hierarchy的编辑器扩展
/// </summary>
public class HierarchyEditor
{
    [InitializeOnLoadMethod]
    static void InitializeOnLoadMethod()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowOnGUI;
    }


    static void HierarchyWindowOnGUI(int instanceID, Rect selectionRect)
    {

        //Toggle控制Hierarchy上的对象显隐
        Rect rect = new Rect(selectionRect);
        rect.x += (selectionRect.width - 15f);
        rect.width = 15f;

        GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (go == null) return;
        bool newActive = go.activeSelf;
        bool active = GUI.Toggle(rect, go.activeSelf, string.Empty);

        if (newActive != active)
        {
            newActive = active;
            go.SetActive(newActive);
            EditorUtility.SetDirty(go);
        }
    }
}

