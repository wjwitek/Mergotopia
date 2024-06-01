using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(Rigidbody2D))]
public class Shape : MonoBehaviour
{
    public bool fixedShape = true;
    public Material material;
    public Color color = Color.white;
    public int initialVerticies = 4;
    public float frameWidth = 0.05f;

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Mesh mesh;
    public PolygonCollider2D polygonCollider;
    public Rigidbody2D rbPhysics;

    private float mergingTimeLeft = 0;
    private bool isMerging = false;
    private float factor = 0;
    private static readonly float hopes = 4;

    virtual public void Reset()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        rbPhysics = GetComponent<Rigidbody2D>();
        rbPhysics.gravityScale = 0;
        rbPhysics.drag = 2;
        rbPhysics.angularDrag = 2;
        mesh = null;
        if (!fixedShape)
        {
            meshFilter.mesh = null;
        }
        meshRenderer.sharedMaterial = null;
        foreach (var collider in GetComponents<PolygonCollider2D>())
        {
            DestroyImmediate(collider);
        }
        polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
    }

    public void Bake()
    {
        Reset();
        if (fixedShape)
        {
            mesh = meshFilter.sharedMesh;
            RemoveDuplicatedVertices(0.0001f);
            MakeVerticesInOrder();
        }
        else
        {
            mesh = new Mesh();
            ResetShapeVertices();
            ResetShapeTriangles();
            meshFilter.sharedMesh = mesh;
        }
        meshRenderer.sharedMaterial = material;

        RecalculateShapeNormals();
        RecalculateShapeColors(color);

        RecalculateColliders();
        RecalculateMass();
    }

    private void RemoveDuplicatedVertices(float epsilon)
    {
        var verticesMapping = new Dictionary<int, int>();
        var remainingVerticesMapping = new Dictionary<int, int>();
        var remainingVertices = new List<Vector3>();
        var verticies = mesh.vertices;
        var triangles = mesh.triangles;
        var remainingTriangles = new List<int>();

        for (int i = 0; i < verticies.Length; i++)
        {
            bool found = false;
            for (int j = i + 1; j < verticies.Length; j++)
            {
                var diff = verticies[i] - verticies[j];
                if (Mathf.Abs(diff.x) < epsilon && Mathf.Abs(diff.y) < epsilon)
                {
                    verticesMapping[i] = j;
                    found = true;
                }
            }
            if (!found)
            {
                verticesMapping[i] = i;
                remainingVerticesMapping[i] = remainingVertices.Count;
                remainingVertices.Add(verticies[i]);
            }
        }

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int v1 = remainingVerticesMapping[verticesMapping[triangles[i]]];
            int v2 = remainingVerticesMapping[verticesMapping[triangles[i + 1]]];
            int v3 = remainingVerticesMapping[verticesMapping[triangles[i + 2]]];
            if (v1 != v2 && v2 != v3 && v3 != v1)
            {
                remainingTriangles.Add(v1);
                remainingTriangles.Add(v2);
                remainingTriangles.Add(v3);
            }
        }

        mesh.triangles = remainingTriangles.ToArray();
        mesh.vertices = remainingVertices.ToArray();
    }

    private void MakeVerticesInOrder()
    {
        var triangles = mesh.triangles;
        var trianglesSet = new HashSet<int>();
        for (int i = 3; i < triangles.Length; i += 3)
        {
            trianglesSet.Add(i);
        }
        var vertices = new List<int>(mesh.vertexCount);
        vertices.Add(triangles[0]);
        vertices.Add(triangles[1]);
        vertices.Add(triangles[2]);

        while (trianglesSet.Count > 0)
        {
            int foundTri = -1;
            foreach (var tri in trianglesSet)
            {
                var insertIndex = new int[2];
                int missing = -1;
                int same = 0;
                for (int j = 0; j < 3; j++)
                {
                    bool found = false;
                    for (int k = 0; k < vertices.Count; k++)
                    {
                        if (vertices[k] == triangles[tri + j])
                        {
                            insertIndex[same] = k;
                            same++;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        missing = triangles[tri + j];
                    }
                    if (same == 2)
                    {
                        break;
                    }
                }
                if (same == 2)
                {
                    if (missing < 0)
                    {
                        missing = triangles[tri + 2];
                    }
                    foundTri = tri;
                    int greaterIndex;
                    int indexDiff;
                    if (insertIndex[0] > insertIndex[1])
                    {
                        greaterIndex = insertIndex[0];
                        indexDiff = insertIndex[0] - insertIndex[1];
                    }
                    else
                    {
                        greaterIndex = insertIndex[1];
                        indexDiff = insertIndex[1] - insertIndex[0];
                    }
                    if (indexDiff > 1)
                    {
                        vertices.Insert(greaterIndex + 1, missing);
                    }
                    else
                    {
                        vertices.Insert(greaterIndex, missing);
                    }
                    break;
                }
            }
            if (foundTri < 0)
            {
                Debug.LogError("Error in MakeVerticesInOrder: triangle not found");
                return;
            }
            trianglesSet.Remove(foundTri);
        }

        var verticesMap = new int[vertices.Count];
        var newVertices = new Vector3[vertices.Count];
        var oldVertices = mesh.vertices;
        for (int i = 0; i < vertices.Count; i++)
        {
            newVertices[vertices.Count - i - 1] = oldVertices[vertices[i]];
            verticesMap[vertices[i]] = vertices.Count - i - 1;
        }

        var newTrianlges = new int[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            newTrianlges[i] = verticesMap[triangles[i]];
        }

        mesh.vertices = newVertices;
        mesh.triangles = newTrianlges;
    }

    protected void ResetShapeVertices()
    {
        float shift = 2 * Mathf.PI / initialVerticies;

        var vertices = new Vector3[initialVerticies];
        for (int i = 0; i < initialVerticies; i++)
        {
            float x = Mathf.Cos(shift * i);
            float y = Mathf.Sin(shift * i);
            vertices[i].x = x;
            vertices[i].y = y;
        }
        mesh.vertices = vertices;
    }

    protected void ResetShapeTriangles()
    {
        int almsotLastVertexIndex = mesh.vertexCount - 2;
        int[] tris = new int[3 * almsotLastVertexIndex];
        for (int i = 0; i < almsotLastVertexIndex; i++)
        {
            int triIndex = i * 3;
            tris[triIndex] = 0;
            tris[triIndex + 1] = i + 1;
            tris[triIndex + 2] = i + 2;
        }
        mesh.triangles = tris;
    }

    protected void RecalculateShapeNormals()
    {
        Vector3[] normals = new Vector3[mesh.vertexCount];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = Vector3.up;
        }
        mesh.normals = normals;
    }

    protected void RecalculateShapeColors(Color newColor)
    {
        Color[] colors = new Color[mesh.vertexCount];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = newColor;
        }
        mesh.colors = colors;
    }

    virtual protected void RecalculateColliders()
    {
        polygonCollider.pathCount = 1;
        var points = new Vector2[mesh.vertexCount];
        var vertices = new List<Vector3>();
        mesh.GetVertices(vertices);
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = vertices[i];
        }
        polygonCollider.SetPath(0, points);
    }

    protected void RecalculateMass()
    {
        var angle = Mathf.PI / mesh.vertexCount;
        rbPhysics.mass = Mathf.Sin(angle) * Mathf.Cos(angle) * mesh.vertexCount;
    }

    public void Merge(Shape other)
    {
        var otherVertices = new Vector3[other.mesh.vertices.Length];
        other.transform.TransformPoints(other.mesh.vertices, otherVertices);
        var vertices = new Vector3[mesh.vertices.Length];
        transform.TransformPoints(mesh.vertices, vertices);

        float top1minDistance = Mathf.Infinity;
        int top1mineIndex = -1;
        int top1otherIndex = -1;
        float top2minDistance = Mathf.Infinity;
        int top2mineIndex = -1;
        int top2otherIndex = -1;
        for (int i = 0; i < vertices.Length - 1; i++)
        {
            for (int j = 0; j < otherVertices.Length - 1; j++)
            {
                float dis = (vertices[i] - otherVertices[j]).sqrMagnitude;
                if (dis < top1minDistance)
                {
                    if (top1mineIndex != i && top1otherIndex != j)
                    {
                        top2minDistance = top1minDistance;
                        top2mineIndex = top1mineIndex;
                        top2otherIndex = top1otherIndex;
                    }
                    top1minDistance = dis;
                    top1mineIndex = i;
                    top1otherIndex = j;
                }
                else if (dis < top2minDistance && top1mineIndex != i && top1otherIndex != j)
                {
                    top2minDistance = dis;
                    top2mineIndex = i;
                    top2otherIndex = j;
                }
            }
        }

        var colors = mesh.colors;
        colors[top1mineIndex] = Color.black;
        colors[top2mineIndex] = Color.black;
        mesh.colors = colors;
        colors = other.mesh.colors;
        colors[top1otherIndex] = Color.cyan;
        colors[top2otherIndex] = Color.cyan;
        other.mesh.colors = colors;

        List<Vector3> newVertices = new List<Vector3>(mesh.vertices);
        List<int> newTris = new List<int>(other.mesh.triangles);
        List<Vector3> newNormals = new List<Vector3>(mesh.normals);
        List<Color> newColors = new List<Color>(mesh.colors);

        for (int i = 0; i < newTris.Count; i++)
        {
            newTris[i] += newVertices.Count;
        }

        newTris.Add(top1mineIndex);
        newTris.Add(top1otherIndex + newVertices.Count);
        newTris.Add(top2otherIndex + newVertices.Count);

        newTris.Add(top1mineIndex);
        newTris.Add(top2mineIndex);
        newTris.Add(top2otherIndex + newVertices.Count);

        transform.InverseTransformPoints(otherVertices);
        newVertices.AddRange(otherVertices);
        newTris.AddRange(mesh.triangles);
        newNormals.AddRange(other.mesh.normals);
        newColors.AddRange(other.mesh.colors);
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTris.ToArray();
        mesh.normals = newNormals.ToArray();
        mesh.colors = newColors.ToArray();

        /*int ii = 0;
        for (int i = top2otherIndex; i != top1otherIndex; i = (i + 1) % (otherVertices.Length / 2))
        {
            ii++;
            int outerIndex = top1mineIndex + ii;
            int innerIndex = top1mineIndex + ii + vertices.Length / 2;
            newVertices.Insert(outerIndex, transform.InverseTransformPoint(otherVertices[i]));
            newVertices.Insert(innerIndex, transform.InverseTransformPoint(otherVertices[i + otherVertices.Length / 2]));

            int lastOuterIndex = vertices.Length / 2 + ii;


            newTris.Insert(ver)

            tris[i * 6 + 0] = i + initialVerticies;
            tris[i * 6 + 1] = i;
            tris[i * 6 + 2] = nextVertex;

            tris[i * 6 + 3] = i + initialVerticies;
            tris[i * 6 + 4] = nextVertex;
            tris[i * 6 + 5] = nextVertex + initialVerticies;
        }*/
    }

    private static readonly float fourPiSquare = 4f * Mathf.PI * Mathf.PI;
    private static readonly float quarterPi = Mathf.PI * 0.25f;
    private float mergingIndicatorFunction(float x)
    {
        return Mathf.Cos(fourPiSquare / (x + quarterPi)) * 0.5f + 0.5f;
    }

    private static readonly float nominator_factor = Mathf.PI * (17f - 2 * hopes) / (4f * (2 * hopes - 1f));
    public void beginMerging(float duration)
    {
        isMerging = true;
        mergingTimeLeft = duration;
        factor = nominator_factor / duration;
    }

    public void endMerging()
    {
        isMerging = false;
        RecalculateShapeColors(color);
    }

    void Update()
    {
        if (!isMerging)
        {
            return;
        }

        var meshColor = mesh.colors[0];
        var val = mergingIndicatorFunction(mergingTimeLeft * factor);
        meshColor.r = color.r + (1f - color.r) * val;
        meshColor.g = color.g + (1f - color.g) * val;
        meshColor.b = color.b + (1f - color.b) * val;
        RecalculateShapeColors(meshColor);
        mergingTimeLeft -= Time.deltaTime;
    }
}
