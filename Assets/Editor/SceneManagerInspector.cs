using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneManager))]
public class SceneManagerInspector : Editor
{
    private AreaChecker areaChecker = new AreaChecker(0.01f);

    public override void OnInspectorGUI()
    {
        var sceneManager = target as SceneManager;

        sceneManager.shapesMaterial
            = EditorGUILayout.ObjectField("Shapes material", sceneManager.shapesMaterial, typeof(Material), false) as Material;
        if (GUILayout.Button("Assign material to all"))
        {
            sceneManager.AssignMaterialToShapes();
        }
        sceneManager.shapesFrameWidth = EditorGUILayout.FloatField("Shapes frame width", sceneManager.shapesFrameWidth);
        if (GUILayout.Button("Assign frame width to all"))
        {
            sceneManager.AssignFrameWidthToShapes();
        }
        if (GUILayout.Button("Bake all"))
        {
            sceneManager.BakeAllShapes();
        }

        sceneManager.playersMergeRange = EditorGUILayout.FloatField("Players merge range", sceneManager.playersMergeRange);
        if (GUILayout.Button("Assign merge range to all"))
        {
            sceneManager.AssignMergeRangeToPlayers();
        }
        sceneManager.playersMergeAreaSimplicity = EditorGUILayout.FloatField("Players merge area simplicity", sceneManager.playersMergeAreaSimplicity);
        if (GUILayout.Button("Assign merge area simplicity to all"))
        {
            sceneManager.AssignMergeAreaSimplicityToPlayers();
        }

        sceneManager.a = EditorGUILayout.ObjectField("GO A", sceneManager.a, typeof(GameObject), true) as GameObject;
        sceneManager.b = EditorGUILayout.ObjectField("GO B", sceneManager.b, typeof(GameObject), true) as GameObject;
        if (GUILayout.Button("TEST"))
        {
            Debug.Log(areaChecker.GetAreasSimilarity(sceneManager.a, sceneManager.b, 0.9f));
        }
    }
}
