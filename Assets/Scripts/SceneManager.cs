using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public GameObject a, b;
    public Material shapesMaterial;
    public float shapesFrameWidth;
    public float playersMergeRange = 0.4f;
    public float playersMergeAreaSimplicity = 0.2f;

    public void AssignMergeRangeToPlayers()
    {
        foreach (var player in findAllPlayers())
        {
            player.mergeRange = playersMergeRange;
        }
    }

    public void AssignMergeAreaSimplicityToPlayers()
    {
        foreach (var player in findAllPlayers())
        {
            player.mergeAreaSimplicity = playersMergeAreaSimplicity;
        }
    }

    public void AssignFrameWidthToShapes()
    {
        foreach (var shape in findAllShapes())
        {
            shape.frameWidth = shapesFrameWidth;
        }
    }

    public void AssignMaterialToShapes()
    {
        foreach (var shape in findAllShapes())
        {
            shape.material = shapesMaterial;
        }
    }

    public void BakeAllShapes()
    {
        foreach (var shape in findAllShapes())
        {
            shape.Bake();
        }
    }

    public void Merge()
    {
        var shapes = findAllShapes();
        shapes[0].Merge(shapes[1]);
    }

    private Shape[] findAllShapes()
    {
        return FindObjectsOfType<Shape>();
    }

    private Player[] findAllPlayers()
    {
        return FindObjectsOfType<Player>();
    }
}
