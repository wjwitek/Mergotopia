/*using TMPro;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelButton))]
public class LevelButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var levelButton = target as LevelButton;

        Debug.Log(levelButton.levelName);
        levelButton.levelName = EditorGUILayout.TextField("Level Name", levelButton.levelName);
        levelButton.levelNameLabel = EditorGUILayout.ObjectField("Level Name Label", levelButton.levelNameLabel, typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
        levelButton.stars = EditorGUILayout.ObjectField("Stars", levelButton.stars, typeof(Star), true) as Star;

        if (GUILayout.Button("Reset"))
        {
            levelButton.Reassign();
        }
    }
}*/
