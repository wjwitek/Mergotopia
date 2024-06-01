using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaChecker
{
    private readonly float gridSize;

    public AreaChecker(float gridSize)
    {
        this.gridSize = gridSize;
    }

    public float GetAreasSimilarity(GameObject a, GameObject b, float minCover)
    {
        var aColliders = a.GetComponentsInChildren<Collider2D>();
        var bColliders = b.GetComponentsInChildren<Collider2D>();

        float boxMaxX = -Mathf.Infinity;
        float boxMinX = Mathf.Infinity;
        float boxMaxY = -Mathf.Infinity;
        float boxMinY = Mathf.Infinity;

        foreach (var aCollider in aColliders)
        {
            if (aCollider.isTrigger)
            {
                continue;
            }
            var box = aCollider.bounds;
            boxMaxX = Mathf.Max(boxMaxX, box.max.x);
            boxMinX = Mathf.Min(boxMinX, box.min.x);
            boxMaxY = Mathf.Max(boxMaxY, box.max.y);
            boxMinY = Mathf.Min(boxMinY, box.min.y);
        }

        foreach (var bCollider in bColliders)
        {
            if (bCollider.isTrigger)
            {
                continue;
            }
            var box = bCollider.bounds;
            boxMaxX = Mathf.Max(boxMaxX, box.max.x);
            boxMinX = Mathf.Min(boxMinX, box.min.x);
            boxMaxY = Mathf.Max(boxMaxY, box.max.y);
            boxMinY = Mathf.Min(boxMinY, box.min.y);
        }

        int gridWidth = Mathf.RoundToInt((boxMaxX - boxMinX) / gridSize);
        int gridHeight = Mathf.RoundToInt((boxMaxY - boxMinY) / gridSize);
        int pointsInA = 0;
        int pointsInB = 0;
        int pointsInAB = 0;

        var point = new Vector2(boxMinX - 0.00001f, boxMinY - 0.00001f);
        
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                bool isInA = false;
                foreach (var aCollider in aColliders)
                {
                    if (aCollider.isTrigger)
                    {
                        continue;
                    }
                    if (aCollider.OverlapPoint(point))
                    {
                        pointsInA++;
                        isInA = true;
                        break;
                    }
                }
                foreach (var bCollider in bColliders)
                {
                    if (bCollider.isTrigger)
                    {
                        continue;
                    }
                    if (bCollider.OverlapPoint(point))
                    {
                        pointsInB++;
                        if (isInA)
                        {
                            pointsInAB++;
                        }
                        break;
                    }
                }
                point.y += gridSize;
            }
            point.x += gridSize;
            point.y = boxMinY - 0.00001f;
        }

        //Debug.Log("A: " + pointsInA + " B: " + pointsInB + " AB: " + pointsInAB);
        
        Debug.DrawLine(new Vector3(boxMinX, boxMinY, 0), new Vector3(boxMinX, boxMaxY, 0), Color.green, 10f);
        Debug.DrawLine(new Vector3(boxMinX, boxMaxY, 0), new Vector3(boxMaxX, boxMaxY, 0), Color.blue, 10f);
        Debug.DrawLine(new Vector3(boxMaxX, boxMaxY, 0), new Vector3(boxMaxX, boxMinY, 0), Color.red, 10f);
        Debug.DrawLine(new Vector3(boxMaxX, boxMinY, 0), new Vector3(boxMinX, boxMinY, 0), Color.white, 10f);

        float minCoverPoints = pointsInB * minCover * minCover;
        float similarity = Mathf.Clamp((2 * pointsInAB - pointsInA) / minCoverPoints, 0, 1);
        return similarity;
    }
}
