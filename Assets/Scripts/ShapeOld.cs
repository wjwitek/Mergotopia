using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class ShapeOld : MonoBehaviour
{
    public Material material;
    public Color color = Color.white;
    public int initialVerticies = 4;
    public float frameWidth = 0.05f;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Reset()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter.mesh = null;
        meshRenderer.sharedMaterial = null;
    }

    public void ResetShape()
    {
        Reset();
        mesh = new Mesh();
        meshRenderer.sharedMaterial = material;

        ResetShapeVertices();
        ResetShapeTriangles();
        ResetShapeNormals();
        ResetShapeColors();

        meshFilter.mesh = mesh;
    }

    private void ResetShapeVertices()
    {
        float shift = 2 * Mathf.PI / initialVerticies;
        float inner_radius = 1 - frameWidth / Mathf.Cos(Mathf.PI / initialVerticies);

        Vector3[] vertices = new Vector3[2 * initialVerticies];
        for (int i = 0; i < initialVerticies; i++)
        {
            float x = Mathf.Cos(shift * i);
            float y = Mathf.Sin(shift * i);
            vertices[i] = new Vector3(x, y, 0);
            vertices[i + initialVerticies] = new Vector3(x * inner_radius, y * inner_radius, 0);
        }
        mesh.vertices = vertices;
    }

    private void ResetShapeTriangles()
    {
        int[] tris = new int[6 * initialVerticies];
        int lastVertex = initialVerticies - 1;
        for (int i = 0; i < lastVertex; i++)
        {
            int nextVertex = i + 1;
            tris[i * 6 + 0] = i + initialVerticies;
            tris[i * 6 + 1] = i;
            tris[i * 6 + 2] = nextVertex;

            tris[i * 6 + 3] = i + initialVerticies;
            tris[i * 6 + 4] = nextVertex;
            tris[i * 6 + 5] = nextVertex + initialVerticies;
        }
        // Last pair of triangles calulcated separately to avoid modulo operation
        int lastTriangle = lastVertex * 6;
        tris[lastTriangle + 0] = lastVertex + initialVerticies;
        tris[lastTriangle + 1] = lastVertex;
        tris[lastTriangle + 2] = 0;

        tris[lastTriangle + 3] = lastVertex + initialVerticies;
        tris[lastTriangle + 4] = 0;
        tris[lastTriangle + 5] = initialVerticies;
        mesh.triangles = tris;
    }

    private void ResetShapeNormals()
    {
        Vector3[] normals = new Vector3[2 * initialVerticies];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = Vector3.up;
        }
        mesh.normals = normals;
    }

    private void ResetShapeColors()
    {
        Color[] colors = new Color[2 * initialVerticies];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = color;
        }
        mesh.colors = colors;
    }

    public void Merge(ShapeOld other)
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
        for (int i = 0; i < vertices.Length / 2; i++)
        {
            for (int j = 0; j < otherVertices.Length / 2; j++)
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

        newTris.Add(top1mineIndex + vertices.Length / 2);
        newTris.Add(top1mineIndex);
        newTris.Add(top1otherIndex + newVertices.Count);

        newTris.Add(top1mineIndex + vertices.Length / 2);
        newTris.Add(top1otherIndex + newVertices.Count);
        newTris.Add(top1otherIndex + newVertices.Count + otherVertices.Length / 2);

        newTris.Add(top2mineIndex + vertices.Length / 2);
        newTris.Add(top2mineIndex);
        newTris.Add(top2otherIndex + newVertices.Count);

        newTris.Add(top2mineIndex + vertices.Length / 2);
        newTris.Add(top2otherIndex + newVertices.Count);
        newTris.Add(top2otherIndex + newVertices.Count + otherVertices.Length / 2);

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

    void Update()
    {

    }
}
