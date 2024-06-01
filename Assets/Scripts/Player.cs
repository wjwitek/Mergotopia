using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class Player : Shape
{
    private struct MergingInfo
    {
        public MergingInfo(int c = 1, float t = 0)
        {
            timer = t;
            enterCounter = c;
        }
        public float timer;
        public int enterCounter;
    };

    public float mergeRange = 0.4f;
    public float mergeAreaSimplicity = 0.2f;
    public float mergeTime = 4;
    private Dictionary<Shape, MergingInfo> mergingShapes = new Dictionary<Shape, MergingInfo>();

    public PolygonCollider2D mergeCollider;
    public AudioSource mergeSound;

    override public void Reset()
    {
        base.Reset();
        mergeCollider = gameObject.AddComponent<PolygonCollider2D>();
        mergeCollider.isTrigger = true;
    }

    override protected void RecalculateColliders()
    {
        base.RecalculateColliders();
        RecalculateMergeArea();
    }

    protected void RecalculateMergeArea()
    {
        var mergeColliders = new List<PolygonCollider2D>();
        var colliders = new List<PolygonCollider2D>();
        foreach (var collider in GetComponents<PolygonCollider2D>())
        {
            if (collider.isTrigger)
            {
                mergeColliders.Add(collider);
            }
            else
            {
                colliders.Add(collider);
            }
        }
        Assert.IsTrue(mergeColliders.Count == colliders.Count);
        for (int i = 0; i < mergeColliders.Count && i < colliders.Count; i++)
        {
            var points = SimplifyCollider(getMergeAreaPoints(colliders[i]), mergeAreaSimplicity);
            mergeColliders[i].points = points.ToArray();
        }
    }

    private List<Vector2> getMergeAreaPoints(PolygonCollider2D collider)
    {
        var points = new List<Vector2>();
        var orginalPoints = collider.points;

        for (int i = 0; i < orginalPoints.Length; i++)
        {
            int i_next = (i + 1) % orginalPoints.Length;
            int i_prev = (i + orginalPoints.Length - 1) % orginalPoints.Length;

            if (Mathf.Abs(Geometry.CrossProduct(orginalPoints[i_prev] - orginalPoints[i], orginalPoints[i_next] - orginalPoints[i])) > 0.001f)
            {
                points.Add(Geometry.GetPointAlongBisector(orginalPoints[i_prev], orginalPoints[i], orginalPoints[i_next], mergeRange));
            }

            Vector2 prev = orginalPoints[i];
            Vector2 next = orginalPoints[i_next];
            Vector2 halfPoint = (next + prev) * 0.5f;
            Vector2 seg = next - prev;
            seg = new Vector2(seg.y, -seg.x);
            seg = seg * mergeRange / seg.magnitude;

            points.Add(prev + seg);
            points.Add(next + seg);

            //Debug.DrawLine(transform.TransformPoint(points[points.Count - 1]), transform.TransformPoint(orginalPoints[i_next]), Color.magenta, 300f);
            //Debug.DrawLine(transform.TransformPoint(points[points.Count - 2]), transform.TransformPoint(orginalPoints[i]), Color.magenta, 300f);
            //Debug.DrawLine(transform.TransformPoint(points[points.Count - 3]), transform.TransformPoint(orginalPoints[i]), Color.magenta, 300f);
        }

        int leftMostIndex = 0;
        for (int i = 1; i < points.Count; i++)
        {
            if (points[i].x < points[leftMostIndex].x)
            {
                leftMostIndex = i;
            }
        }

        //Debug.DrawLine(transform.TransformPoint(points[leftMostIndex]), transform.TransformPoint(Vector3.zero), Color.green, 300f);
        //Debug.Log("leftMostIndex: " + leftMostIndex);

        //for (int i = 0; i < points.Count; i++)
        //{
        //    int i_next = (i + 1) % points.Count;
        //    Debug.DrawLine(transform.TransformPoint(points[i]), transform.TransformPoint(points[i_next]), Color.black, 300f);
        //}

        //Debug.Log("Points count: " + points.Count);

        int i_start = leftMostIndex;
        int i_end = (leftMostIndex + 1) % points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 closestIntersection = points[i_end];
            float closestIntersectionDistance = (points[i_start] - closestIntersection).sqrMagnitude;
            int intersection_end = -1;
            for (int j = 0; j < points.Count; j++)
            {
                int j_start = (leftMostIndex + j) % points.Count;
                int j_end = (leftMostIndex + j + 1) % points.Count;
                if (j_start == i_start || j_start == i_end || j_end == i_start)
                {
                    continue;
                }
                Vector2 intersection;
                if (Geometry.Intersects(points[i_start], points[i_end], points[j_start], points[j_end], out intersection))
                {
                    float dist = (points[i_start] - intersection).sqrMagnitude;
                    if (dist < closestIntersectionDistance)
                    {
                        intersection_end = j_end;
                        closestIntersection = intersection;
                        closestIntersectionDistance = dist;
                    }
                }
            }
            if (intersection_end > 0)
            {
                //Debug.Log("Intersects!");
                int pointsToRemove = intersection_end - i_end;
                if (pointsToRemove < 0)
                {
                    pointsToRemove += points.Count;
                    int pointsToRemoveTillEnd = points.Count - i_end;
                    //Debug.Log("remove from: " + i_end + " up to: " + (i_end + pointsToRemoveTillEnd - 1));
                    points.RemoveRange(i_end, pointsToRemoveTillEnd);
                    pointsToRemove -= pointsToRemoveTillEnd;
                    //Debug.Log("remove from: " + 0 + " up to: " + (pointsToRemove - 1));
                    points.RemoveRange(0, pointsToRemove);
                    //Debug.Log("insert at: " + (i_end - pointsToRemove));
                    points.Insert(i_end - pointsToRemove, closestIntersection);
                    leftMostIndex -= pointsToRemove;
                }
                else
                {
                    //Debug.Log("remove from: " + i_end + " up to: " + (intersection_end - 1));
                    points.RemoveRange(i_end, intersection_end - i_end);
                    //Debug.Log("insert at: " + i_end);
                    points.Insert(i_end, closestIntersection);
                }
            }
            i_start = (i_start + 1) % points.Count;
            i_end = (i_end + 1) % points.Count;
        }
        return points;
    }

    private List<Vector2> SimplifyCollider(List<Vector2> points, float minDistance)
    {
        float minDistanceSqr = minDistance * minDistance;
        List<Vector2> reducedPoints = new List<Vector2>();

        int pointsCount = points.Count;
        Vector2 center = points[0];
        int pointsToMerge = 1;
        int prevPoint = pointsCount - 1;
        while (prevPoint > 0 && (points[0] - points[prevPoint]).sqrMagnitude < minDistanceSqr)
        {
            center += points[prevPoint];
            pointsToMerge++;
            prevPoint--;
        }
        if (pointsToMerge > 1)
        {
            int centerPointIndex = prevPoint + 1;
            while (prevPoint > 0 && (points[centerPointIndex] - points[prevPoint]).sqrMagnitude < minDistanceSqr)
            {
                center += points[prevPoint];
                pointsToMerge++;
                prevPoint--;
            }
            pointsCount -= pointsToMerge - 1;
            points[0] = center / pointsToMerge;
        }

        for (int i = 0; i < pointsCount; i++)
        {
            center = points[i];
            pointsToMerge = 1;
            int i_next = i + 1;

            while (i_next < pointsCount && (points[i] - points[i_next]).sqrMagnitude < minDistanceSqr)
            {
                center += points[i_next];
                pointsToMerge++;
                i_next++;
            }
            int centerPointIndex = i_next - 1;
            while (i_next < pointsCount && (points[centerPointIndex] - points[i_next]).sqrMagnitude < minDistanceSqr)
            {
                center += points[i_next];
                pointsToMerge++;
                i_next++;
            }
            i += pointsToMerge - 1;
            reducedPoints.Add(center / pointsToMerge);
        }

        return reducedPoints;
    }

    private struct MergeArea
    {
        public Vector2 leftFrom, leftTo, rightFrom, rightTo;
        public bool isLeftFromVertex, isRightFromVertex;
        public int nextLeftFromVertex, nextLeftToVertex, nextRightFromVertex, nextRightToVertex;
    };

    private void MergeWith(Shape shape)
    {
        var positionOffset = shape.transform.position - transform.position;
        var positionOffset2 = (Vector2)positionOffset;
        var vertices = new List<Vector3>(shape.mesh.vertexCount);
        var newVertices = new List<Vector3>(mesh.vertexCount + shape.mesh.vertexCount);
        mesh.GetVertices(newVertices);
        shape.mesh.GetVertices(vertices);
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = transform.InverseTransformPoint(shape.transform.TransformPoint(vertices[i]));
        }
        newVertices.AddRange(vertices);

        var triangles = new List<int>(vertices.Count * 3);
        var newTriangles = new List<int>(newVertices.Count * 3);
        mesh.GetTriangles(newTriangles, 0);
        shape.mesh.GetTriangles(triangles, 0);
        for (int i = 0; i < triangles.Count; i++)
        {
            triangles[i] += mesh.vertexCount;
        }
        newTriangles.AddRange(triangles);

        var colors = new List<Color>(vertices.Count);
        var newColors = new List<Color>(newVertices.Count);
        mesh.GetColors(newColors);
        shape.mesh.GetColors(colors);
        newColors.AddRange(colors);

        Destroy(shape.gameObject);
        mesh.SetVertices(newVertices);
        mesh.SetTriangles(newTriangles, 0);
        mesh.SetColors(newColors);
        RecalculateShapeNormals();

        var newCollider = gameObject.AddComponent<PolygonCollider2D>();
        var points = shape.polygonCollider.points;
        for (int j = 0; j < points.Length; j++)
        {
            points[j] = transform.InverseTransformPoint(shape.transform.TransformPoint(points[j]));
        }
        newCollider.points = points;
        var newMergeArea = gameObject.AddComponent<PolygonCollider2D>();
        newMergeArea.isTrigger = true;

        RecalculateMergeArea();
        //rbPhysics.mass += shape.rbPhysics.mass;
    }

    private void Update()
    {
        foreach (var shape in mergingShapes.Keys.ToArray())
        {
            if (mergingShapes[shape].timer > mergeTime)
            {
                shape.endMerging();
                // Might be unnecessary as it seems destoryed objects call OnTriggerExit2D
                mergingShapes.Remove(shape);
                MergeWith(shape);
                mergeSound.Play();
            }
            else
            {
                var mergingInfo = mergingShapes[shape];
                mergingInfo.timer += Time.deltaTime;
                mergingShapes[shape] = mergingInfo;
            }
        }
    }

    /*private void MergeWith(Shape shape, bool debug)
    {
        int wasHit = -1;
        var mergeAreas = new List<MergeArea>();
        for (int j = 0; j < shape.polygonCollider.points.Length; j++)
        {
            for (int i = 0; i < mergeCollider.points.Length; i++)
            {
                Vector2 intersectionPoint;
                if (Geometry.Intersects(
                    transform.TransformPoint(mergeCollider.points[i]),
                    transform.TransformPoint(mergeCollider.points[(i + 1) % mergeCollider.points.Length]),
                    shape.transform.TransformPoint(shape.polygonCollider.points[j]),
                    shape.transform.TransformPoint(shape.polygonCollider.points[(j + 1) % shape.polygonCollider.points.Length]),
                    out intersectionPoint))
                {
                    Vector2 direction = ((Vector2)transform.position - intersectionPoint).normalized;
                    Physics2D.queriesHitTriggers = false;
                    RaycastHit2D hit = Physics2D.Raycast(intersectionPoint + direction * Vector2.kEpsilon, direction);
                    Debug.DrawLine(intersectionPoint, polygonCollider.ClosestPoint(intersectionPoint), Color.blue);
                    if (hit.collider != null && hit.collider != shape.polygonCollider)
                    {
                        wasHit *= -1;
                        //Debug.DrawLine(intersectionPoint, hit.point, Color.blue);
                        if (wasHit < 0)
                        {
                            var mergeArea = mergeAreas[mergeAreas.Count - 1];
                            mergeArea.rightFrom = intersectionPoint;
                            mergeArea.rightTo = hit.point;
                            mergeArea.isRightFromVertex = false;
                            mergeArea.nextRightFromVertex = (j + 1) % shape.polygonCollider.points.Length;
                            mergeArea.nextRightToVertex = (i + 1) % mergeCollider.points.Length;
                            if (!debug)
                            {

                            }
                        }
                        else
                        {
                            var mergeArea = new MergeArea();
                            mergeArea.leftFrom = intersectionPoint;
                            mergeArea.leftTo = hit.point;
                            mergeArea.isLeftFromVertex = false;
                            mergeArea.nextLeftFromVertex = (j + 1) % shape.polygonCollider.points.Length;
                            mergeArea.nextLeftToVertex = (i + 1) % mergeCollider.points.Length;
                            mergeAreas.Add(mergeArea);
                        }
                    }
                    else if (hit.collider != null)
                    {
                        int jOffset = 1;
                        if (wasHit < 0)
                        {
                            jOffset = 0;
                        }
                        for (int n = 0; n < shape.polygonCollider.points.Length; n -= wasHit)
                        {
                            int otherVertexIndex = (n + j + jOffset + shape.polygonCollider.points.Length) % shape.polygonCollider.points.Length;
                            Vector2 point = shape.transform.TransformPoint(shape.polygonCollider.points[otherVertexIndex]);
                            direction = ((Vector2)transform.position - point).normalized;
                            hit = Physics2D.Raycast(point + direction * Vector2.kEpsilon, direction);
                            //Debug.DrawLine(intersectionPoint, polygonCollider.bounds.ClosestPoint(intersectionPoint), Color.blue);
                            if (hit.collider != null && hit.collider != shape.polygonCollider)
                            {
                                wasHit *= -1;
                                //Debug.DrawLine(point, hit.point, Color.red);
                                if (wasHit < 0)
                                {
                                    var mergeArea = mergeAreas[mergeAreas.Count - 1];
                                    mergeArea.rightFrom = point;
                                    mergeArea.rightTo = hit.point;
                                    mergeArea.isRightFromVertex = true;
                                    mergeArea.nextRightFromVertex = otherVertexIndex;
                                    mergeArea.nextRightToVertex = i;
                                    if (!debug)
                                    {

                                    }
                                }
                                else
                                {
                                    var mergeArea = new MergeArea();
                                    mergeArea.leftFrom = point;
                                    mergeArea.leftTo = hit.point;
                                    mergeArea.isLeftFromVertex = true;
                                    mergeArea.nextLeftFromVertex = otherVertexIndex;
                                    mergeArea.nextLeftToVertex = i;
                                    mergeAreas.Add(mergeArea);
                                }
                                break;
                            }
                        }
                    }
                    Physics2D.queriesHitTriggers = true;
                }
            }
        }

        foreach (var mergeArea in mergeAreas)
        {
            if(false)
            if (mergeArea.isLeftFromVertex)
            {
                Debug.DrawLine(shape.transform.TransformPoint(shape.mesh.vertices[mergeArea.nextLeftFromVertex]), mergeArea.leftTo, Color.red);
                Debug.DrawLine(shape.transform.TransformPoint(shape.mesh.vertices[mergeArea.nextLeftFromVertex]), shape.transform.position, Color.black);
                Debug.DrawLine(shape.transform.TransformPoint(shape.mesh.vertices[(mergeArea.nextLeftFromVertex + 1) % shape.mesh.vertexCount]), shape.transform.position, Color.green);
                Debug.DrawLine(transform.TransformPoint(mesh.vertices[(mergeArea.nextLeftToVertex + mesh.vertexCount - 1) % mesh.vertexCount]), transform.position, Color.black);
                Debug.DrawLine(transform.TransformPoint(mesh.vertices[mergeArea.nextLeftToVertex]), transform.position, Color.green);
            }
            else
            {
                Debug.DrawLine(mergeArea.leftFrom, mergeArea.leftTo, Color.blue);

                Debug.DrawLine(shape.transform.TransformPoint(shape.mesh.vertices[(mergeArea.nextLeftFromVertex + shape.mesh.vertexCount - 1) % shape.mesh.vertexCount]), shape.transform.position, Color.black);
                Debug.DrawLine(shape.transform.TransformPoint(shape.mesh.vertices[mergeArea.nextLeftFromVertex]), shape.transform.position, Color.green);

                Debug.DrawLine(transform.TransformPoint(mesh.vertices[(mergeArea.nextLeftToVertex + mesh.vertexCount - 1) % mesh.vertexCount]), transform.position, Color.black);
                Debug.DrawLine(transform.TransformPoint(mesh.vertices[mergeArea.nextLeftToVertex]), transform.position, Color.green);
            }
        }
    }*/

    private void OnTriggerEnter2D(Collider2D other)
    {
        Shape shape = other.GetComponent<Shape>();
        if (shape == null)
        {
            return;
        }
        if (!other.gameObject.Equals(gameObject))
        {
            shape.beginMerging(mergeTime);
            if (mergingShapes.ContainsKey(shape))
            {
                var mergingInfo = mergingShapes[shape];
                mergingInfo.enterCounter++;
                mergingShapes[shape] = mergingInfo;
            }
            else
            {
                mergingShapes.Add(shape, new MergingInfo(1));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Shape shape = other.GetComponent<Shape>();
        if (shape == null)
        {
            return;
        }
        if (!other.gameObject.Equals(gameObject))
        {
            if (mergingShapes.ContainsKey(shape))
            {
                shape.endMerging();
                var mergingInfo = mergingShapes[shape];
                if (mergingInfo.enterCounter == 1)
                {
                    mergingShapes.Remove(shape);
                }
                else
                {
                    mergingInfo.enterCounter--;
                    mergingShapes[shape] = mergingInfo;
                }
            }
        }
    }
}
