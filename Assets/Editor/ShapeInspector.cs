using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(Shape))]
public class ShapeInspector : Editor
{
    public override void OnInspectorGUI()
    {
        var shape = target as Shape;

        shape.fixedShape = EditorGUILayout.Toggle("Fixed shape", shape.fixedShape);
        shape.material = EditorGUILayout.ObjectField("Material", shape.material, typeof(Material), false) as Material;
        shape.color = EditorGUILayout.ColorField("Color", shape.color);
        if (!shape.fixedShape)
        {
            shape.initialVerticies = EditorGUILayout.IntField("Initial verticies", shape.initialVerticies);
        }
        shape.frameWidth = EditorGUILayout.FloatField("Frame width", shape.frameWidth);
        if (GUILayout.Button("Bake"))
        {
            shape.Bake();
        }
    }
}
