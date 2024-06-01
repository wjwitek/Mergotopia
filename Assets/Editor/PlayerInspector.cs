using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(Player))]
public class PlayerInspector : ShapeInspector
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var player = target as Player;
        player.mergeRange = EditorGUILayout.FloatField("Merge range", player.mergeRange);
        player.mergeAreaSimplicity = EditorGUILayout.FloatField("Merge area simplicity", player.mergeAreaSimplicity);
        player.mergeTime = EditorGUILayout.FloatField("Merge time", player.mergeTime);
        player.mergeSound = EditorGUILayout.ObjectField("Merge sound", player.mergeSound, typeof(AudioSource), false) as AudioSource;
    }
}
