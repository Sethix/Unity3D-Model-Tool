using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ClipperLib;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

public class CustomMesh
{

    // Our custom triangle class to keep things cleaner.
    public class Triangle
    { public int a, b, c; public Triangle(int A, int B, int C) { a = A; b = B; c = C; } }

    // Our base mesh object.
    public Mesh mesh;

    public Material material;

    // Our mesh's triangles.
    public List<Triangle> tris;

    // Our 2-D and 3-D polygon coordinates.
    public List<Vector2> flatCoordinates;
    public List<Vector3> realCoordinates;

    // When creating our this sets if it will pop out in both directions.
    public bool singleDirectionalExtrusions;


    // How many times do we want our mesh to pop out?
    public int extrusions;

    // How far should it pop out?
    public float extrusionDistance;

    // Finally, how much are we offsetting each vertex per extrusion?
    public float polygonOffset;


    // Our constructor requires some points to create the shape from.
    public CustomMesh(List<Vector2> points, Material mat, bool oneWay, int extrusionCount, float distancePerExtrusion, float offsetPerExtrusion)
    {
        flatCoordinates = points;
        material = mat;

        tris = new List<Triangle>();

        extrusions = extrusionCount;
        extrusionDistance = distancePerExtrusion;
        polygonOffset = offsetPerExtrusion;
        singleDirectionalExtrusions = oneWay;

        // Boolean for checking if the points are counter-clockwise.
        bool CCW = isCounterClockwise(points.ToArray());

        // If the order is counter-clockwise, we'll want to reverse the list.
        if (CCW) points.Reverse();

        // Using a triangulator we'll now sort the coordinates.
        // Credit to "runevision" of the unity wiki.
        // TODO: Make my own triangulator for deeper understanding and experience.
        Triangulator triangulator = new Triangulator(flatCoordinates.ToArray());

        // Now we'll create the indices in correct order.
        List<int> triangleIndices = new List<int>();

        // List of vertices in the shape we're about to create.
        Vector3[] vertices;

        int doubleCoordCount = flatCoordinates.Count * 2;

        // If we're just creating a flat object...
        if(extrusions <= 0)
        {
            // First we'll populate the indices with a triangulated list of our current points.
            triangleIndices = new List<int>(triangulator.Triangulate());

            // Next we reverse the indices to draw the back side.
            triangleIndices.Reverse();

            // Then we re-triangulate to get the front triangles.
            triangleIndices.AddRange(triangulator.Triangulate());

            // Our vertices will need double the size of the coordinates
            // to allow for each side to have it's own vertices.
            vertices = new Vector3[doubleCoordCount];

            for(int i = 0; i < flatCoordinates.Count; ++i)
            {
                // This will be our front vertex at this coordinate
                vertices[i] = new Vector3(flatCoordinates[i].x, flatCoordinates[i].y, 0);

                // This will be our back vertex at this coordinte
                vertices[vertices.Length - 1 - i] = new Vector3(flatCoordinates[i].x, flatCoordinates[i].y, 0);
            }
        }

        // If we're extruding...
        else
        {
            // If we're only extruding in one direction...
            if(singleDirectionalExtrusions)
            {
                vertices = new Vector3[flatCoordinates.Count * extrusions + flatCoordinates.Count];
            }

            // If we're going both ways...
            else
            {
                vertices = new Vector3[doubleCoordCount * (extrusions + 1)];

                int fwdVert = 0, bwdVert = 0;
                int prevFwd = 0, prevBwd = 0;

                for (int i = 0; i < flatCoordinates.Count; ++i)
                {
                    // We now create lists of int points (int vector2) for use with ClipperLib.
                    // We do this to keep the points consistant with the offset extrusions.
                    Path newShape = new Path();
                    Paths newPolygon = new Paths();

                    // We feed our 2D shape into clipper and multiply the numbers by a large amount.
                    // Clipper only deals with integers and we don't want to lose our numbers.
                    foreach (Vector2 V2 in flatCoordinates)
                        newShape.Add(new IntPoint(V2.x * 100000, V2.y * 100000));

                    // We create an offset to use in offsetting the polygons.
                    // In this case we're only re-ordering so there is no offsetting.
                    ClipperOffset offset = new ClipperOffset();

                    // We then add our clipper shape in to the offset and run the offsetting algorithm.
                    offset.AddPath(newShape, JoinType.jtMiter, EndType.etClosedPolygon);
                    offset.Execute(ref newPolygon, 0f);

                    // This will be our front vertex at this coordinate
                    vertices[i] = new Vector3(newPolygon[0][i].X * 0.00001f, newPolygon[0][i].Y * 0.00001f, 0);

                    // This will be our back vertex at this coordinte
                    vertices[doubleCoordCount - 1 - i] = new Vector3(newPolygon[0][i].X * 0.00001f, newPolygon[0][i].Y * 0.00001f, 0);
                }

                // For each extrusion...
                for (int ext = 1; ext <= extrusions; ++ext)
                {
                    // And for each point...
                    for(int pt = 0; pt < flatCoordinates.Count; ++pt)
                    {
                        // We keep track of the previous points,
                        prevFwd = fwdVert;
                        prevBwd = bwdVert;

                        // Then we set our current points.
                        // Forward Vertex is set to our current point on the front.
                        // Backward Vertex is set to our current point on the back.
                        fwdVert = (ext * doubleCoordCount) + pt;
                        bwdVert = ((ext + 1) * doubleCoordCount) - 1 - pt;

                        // We now create lists of int points (int vector2) for use with ClipperLib.
                        Path newShape = new Path();
                        Paths newPolygon = new Paths();

                        // We feed our 2D shape into clipper and multiply the numbers by a large amount.
                        // Clipper only deals with integers and we don't want to lose our numbers.
                        foreach (Vector2 V2 in flatCoordinates)
                            newShape.Add(new IntPoint(V2.x * 100000, V2.y * 100000));

                        // We create an offset to use in offsetting the polygons.
                        ClipperOffset offset = new ClipperOffset();

                        // We then add our clipper shape in to the offset and run the offsetting algorithm.
                        // It will be offset by the offset per extrusion variable.
                        offset.AddPath(newShape, JoinType.jtMiter, EndType.etClosedPolygon);
                        offset.Execute(ref newPolygon, ext * (offsetPerExtrusion * 100000));

                        // Now we set our vertices to our newly created shape.
                        vertices[fwdVert] = new Vector3(newPolygon[0][pt].X * 0.00001f, 
                                                        newPolygon[0][pt].Y * 0.00001f,
                                                        ext * extrusionDistance);

                        vertices[bwdVert] = new Vector3(newPolygon[0][pt].X * 0.00001f, 
                                                        newPolygon[0][pt].Y * 0.00001f,
                                                        -(ext * extrusionDistance));

                        // Next, we'll create quads between each extrusion.

                        // If this isn't the first vertex...
                        if (pt > 0)
                        {
                            // Front side

                            // Create a triangle starting from our previous vertex,
                            // going to our current vertex on the previous extrusion,
                            // ending on our current vertex.
                            tris.Add(new Triangle
                                         (fwdVert - 1,
                                          fwdVert - doubleCoordCount,
                                          fwdVert));

                            // Next we once again start at our previous vertex,
                            // going to our previous vertex on the previous extrusion,
                            // ending on our current vertex on the previous extrusion.
                            tris.Add(new Triangle
                                         (fwdVert - 1,
                                          fwdVert - 1 - doubleCoordCount,
                                          fwdVert - doubleCoordCount));

                            // Back side

                            // Create a triangle starting from our previous vertex on the current extrusions back
                            // going to our current vertex on the previous extrusions back,
                            // ending on our current vertex on our current extrusions back.
                            tris.Add(new Triangle
                                         (fwdVert - 1 + flatCoordinates.Count,
                                          fwdVert - flatCoordinates.Count,
                                          fwdVert + flatCoordinates.Count));

                            // Next we once again start from our previous vertex on the current extrusions back
                            // going to our previous vertex on the previous extrusion
                            // ending on our current vertex on the previous extrusion.
                            tris.Add(new Triangle
                                         (fwdVert - 1 + flatCoordinates.Count,
                                          fwdVert - 1 - flatCoordinates.Count,
                                          fwdVert - flatCoordinates.Count));
                        }

                        // Otherwise, if it is the first vertex...
                        else
                        {
                            // Front side

                            // Create a triangle starting from the last vertex on the shape,
                            // going to our current vertex on the previous extrusion,
                            // ending on our current vertex.
                            tris.Add(new Triangle
                                         (fwdVert - 1 + flatCoordinates.Count,
                                          fwdVert - doubleCoordCount,
                                          fwdVert));

                            // Next we once again start from the last vertex,
                            // then we go to the last vertex on the previous extrusion,
                            // ending on our current vertex on the last extrusion.
                            tris.Add(new Triangle
                                         (fwdVert - 1 + flatCoordinates.Count,
                                          fwdVert - 1 - flatCoordinates.Count,
                                          fwdVert - doubleCoordCount));

                            // Back side

                            // Create a triangle starting from our previous vertex on our current extrusion,
                            // going to our current vertex on our previous extrusion,
                            // ending on our current vertex on our current extrusion.
                            tris.Add(new Triangle
                                         (fwdVert - 1 + doubleCoordCount,
                                          fwdVert - flatCoordinates.Count,
                                          fwdVert + flatCoordinates.Count));

                            // Next we once again start from the last vertex on the current extrusion,
                            // then we go to the last vertex on the previous extrusion,
                            // ending on the our current vertex on the previous extrusion.
                            tris.Add(new Triangle
                                         (fwdVert - 1 + doubleCoordCount,
                                          fwdVert - 1,
                                          fwdVert - flatCoordinates.Count));

                        }
                    }
                }
            }

            // Exit loop

            // Now we'll add the triangles to our indices.
            foreach (Triangle t in tris)
            {
                triangleIndices.Add(t.a);
                triangleIndices.Add(t.b);
                triangleIndices.Add(t.c);
            }

            // Now we'll create our list of 3D coordinates with the vertices.
            realCoordinates = new List<Vector3>(vertices);

            // Now that we've created the triangles between each extrusion we'll want to
            // create the triangles on the front and back faces.

            // We'll start by grabbing the face vertices.
            List<Vector2> frontVertices = new List<Vector2>(flatCoordinates.Count);
            List<Vector2> backVertices = new List<Vector2>(flatCoordinates.Count);

            for(int i = 0; i < flatCoordinates.Count; ++i)
            {
                frontVertices.Add(vertices[doubleCoordCount * extrusions + i]);
                backVertices.Add(vertices[(doubleCoordCount + 1) * extrusions - i - 1]);
            }

            backVertices.Reverse();

            // Next we want to triangulate their indices.
            Triangulator frontTriangulator = new Triangulator(frontVertices.ToArray());
            Triangulator backTriangulator = new Triangulator(backVertices.ToArray());

            List<int> tempIndices = new List<int>();

            // Finally, we'll add the indices to the list at their correct end positions.
            foreach (int i in frontTriangulator.Triangulate())
                tempIndices.Add(i + doubleCoordCount * extrusions);

            tempIndices.Reverse();

            triangleIndices.AddRange(tempIndices);

            foreach (int i in backTriangulator.Triangulate())
                triangleIndices.Add(i + doubleCoordCount * extrusions + flatCoordinates.Count);
        }

        // Now we'll create the actual mesh object.
        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangleIndices.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Temp
        Debug.Log(mesh.vertexCount.ToString() + " V, " + (triangleIndices.Count / 3).ToString() + " T");
    }

    bool isCounterClockwise(Vector2[] points)
    {
        // If this float is positive our points are clock-wise.
        float polarityOfPoints = 0;

        for(int i = 0; i < points.Length; ++i)
            polarityOfPoints += (points[(i + 1) % points.Length].x - points[i].x) *
                                (points[(i + 1) % points.Length].y + points[i].y);

        if (System.Math.Sign(polarityOfPoints) >= 0) return true;
        else return false;
    }
	
}
