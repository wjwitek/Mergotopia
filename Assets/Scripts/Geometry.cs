using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Geometry
{
    public static float CrossProduct(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    public static float Cross(this Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    public static bool Intersects(Vector2 s1, Vector2 e1, Vector2 s2, Vector2 e2, out Vector2 intersection)
    {
        Vector2 q = s1;
        Vector2 s = e1 - s1;
        Vector2 p = s2;
        Vector2 r = e2 - s2;
        float rCrossS = r.Cross(s);
        if (rCrossS < Vector2.kEpsilon && rCrossS > -Vector2.kEpsilon)
        {
            intersection.x = 0;
            intersection.y = 0;
            return false;
        }
        Vector2 qMinusP = q - p;
        float u = qMinusP.Cross(r) / rCrossS;
        float t = qMinusP.Cross(s) / rCrossS;
        if (1 > u && u > 0 && 1 > t &&  t > 0)
        {
            intersection = q + u * s;
            return true;
        }
        intersection.x = 0;
        intersection.y = 0;
        return false;
    }

    /*public static Vector2 GetPointAlongBisector(Vector2 prev, Vector2 middle, Vector2 next, float distance)
    {
        Vector2 prevVec = prev - middle;
        Vector2 nextVec = next - middle;
        float prevDistance = prevVec.magnitude;
        float nextDistance = nextVec.magnitude;
        float halfCos = Mathf.Sqrt((Vector2.Dot(prevVec, nextVec) / (prevDistance * nextDistance) + 1f) * 0.5f);
        float prevSlide = distance / (prevDistance * halfCos);
        float nextSlide = distance / (nextDistance * halfCos);
        float isConvex = CrossProduct(prevVec, nextVec) < 0 ? 1 : -1;
        Vector2 prevPoint = middle - prevVec * prevSlide * isConvex;
        Vector2 nextPoint = middle - nextVec * nextSlide * isConvex;
        Vector2 middlePoint = (nextPoint + prevPoint) * 0.5f;
        return middlePoint;
    }*/

    public static Vector2 GetPointAlongBisector(Vector2 prev, Vector2 middle, Vector2 next, float distance)
    {
        Vector2 prevVec = (prev - middle).normalized;
        Vector2 nextVec = (next - middle).normalized;
        Vector2 middlePoint = middle - (prevVec + nextVec) * 0.5f;
        float isConvex = CrossProduct(prevVec, nextVec) < 0 ? 1 : -1;
        Vector2 middleVec = (middlePoint - middle) * isConvex;
        return middle + middleVec * (distance / middleVec.magnitude);
    }

    public static bool IsInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 point)
    {
        float dx = point.x - a.x;
        float dy = point.y - a.y;

        bool point_ab = (b.x - a.x) * dy - (b.y - a.y) * dx > 0;

        if (((c.x - a.x) * dy - (c.y - a.y) * dx > 0) == point_ab)
        {
            return false;
        }
        return ((c.x - b.x) * (point.y - b.y) - (c.y - b.y) * (point.x - b.x) > 0) == point_ab;
    }

    private static float GetAngle(Vector2 a, Vector2 b)
    {
        return Mathf.Atan2(Cross(a, b), Vector2.Dot(a, b));
    }

    public static int[] Triangulate(Vector2[] polygon)
    {
        var triangles = new List<int>();
        var vertices = new List<Tuple<int, Vector2>>(polygon.Length);
        int triangles_finded = -1;

        for (int i = 0; i < polygon.Length; i++)
        {
            vertices.Add(new Tuple<int, Vector2>(i, polygon[i]));
        }

        while (triangles_finded != 0)
        {
            triangles_finded = 0;
            while (vertices.Count > 2)
            {
                int prevVertexIndex = vertices.Count - 3;
                int vertexIndex = vertices.Count - 2;
                int nextVertexIndex = vertices.Count - 1;
                Vector2 segment_a = vertices[vertexIndex].Item2 - vertices[prevVertexIndex].Item2;
                Vector2 segment_b = vertices[nextVertexIndex].Item2 - vertices[vertexIndex].Item2;
                if (GetAngle(segment_a, segment_b) > 2 * Mathf.PI)
                {
                    continue;
                }
                bool isValidTriangle = true;
                for (int j = 0; j < polygon.Length; j++)
                {
                    if (IsInTriangle(
                        vertices[prevVertexIndex].Item2,
                        vertices[vertexIndex].Item2,
                        vertices[nextVertexIndex].Item2,
                        polygon[j]))
                    {
                        isValidTriangle = false;
                        break;
                    }
                }
                if (!isValidTriangle)
                {
                    continue;
                }
                triangles.Add(vertices[prevVertexIndex].Item1);
                triangles.Add(vertices[vertexIndex].Item1);
                triangles.Add(vertices[nextVertexIndex].Item1);
                vertices.RemoveAt(vertexIndex);
                triangles_finded++;
                break;
            }
        }
        return triangles.ToArray();
    }
}
