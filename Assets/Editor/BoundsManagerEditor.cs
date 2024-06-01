using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoundsManager))]
public class BoundsManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var boundsManager = target as BoundsManager;

        boundsManager.color = EditorGUILayout.ColorField("Color", boundsManager.color);
        if (GUILayout.Button("Assign color"))
        {
            boundsManager.AssignColor();
        }
    }
}
