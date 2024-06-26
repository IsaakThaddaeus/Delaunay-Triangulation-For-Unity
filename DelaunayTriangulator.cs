using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public static class DelaunayTriangulator
{

    public static List<Triangle> triangulate(List<Vector2> verts){
        List<Vector2> vertices = new List<Vector2>(verts);       
        List<Triangle> triangles = new List<Triangle>();
        Stack stack = new Stack(); stack.Clear();

        normalize(vertices);

        int numPTS = vertices.Count;

        Triangle superTriangle = new Triangle(); triangles.Add(superTriangle);
        superTriangle.vertices[0] = numPTS;
        superTriangle.vertices[1] = numPTS + 1;
        superTriangle.vertices[2] = numPTS + 2;
        superTriangle.adjacentTriangle[0] = -1;
        superTriangle.adjacentTriangle[1] = -1;
        superTriangle.adjacentTriangle[2] = -1;

        vertices.Add(new Vector2(-100, -100));
        vertices.Add(new Vector2(0, 100));
        vertices.Add(new Vector2( 100, -100));
        


        int numTRI = 0;

        
        for (int i = 0; i < numPTS; i++){
            int t = locateTriangle(vertices[i], vertices, triangles, numTRI);

            int a = triangles[t].adjacentTriangle[0];
            int b = triangles[t].adjacentTriangle[1];
            int c = triangles[t].adjacentTriangle[2];
            int v1 = triangles[t].vertices[0];
            int v2 = triangles[t].vertices[1];
            int v3 = triangles[t].vertices[2];

            triangles[t].vertices[0] = i;
            triangles[t].vertices[1] = v1;
            triangles[t].vertices[2] = v2;
            triangles[t].adjacentTriangle[0] = numTRI + 2;
            triangles[t].adjacentTriangle[1] = a;
            triangles[t].adjacentTriangle[2] = numTRI + 1;

            numTRI++;
            triangles.Add(new Triangle());
            triangles[numTRI].vertices[0] = i;
            triangles[numTRI].vertices[1] = v2;
            triangles[numTRI].vertices[2] = v3;
            triangles[numTRI].adjacentTriangle[0] = t;
            triangles[numTRI].adjacentTriangle[1] = b;
            triangles[numTRI].adjacentTriangle[2] = numTRI + 1;

            numTRI++;
            triangles.Add(new Triangle());
            triangles[numTRI].vertices[0] = i;
            triangles[numTRI].vertices[1] = v3;
            triangles[numTRI].vertices[2] = v1;
            triangles[numTRI].adjacentTriangle[0] = numTRI - 1;
            triangles[numTRI].adjacentTriangle[1] = c;
            triangles[numTRI].adjacentTriangle[2] = t;


            if(a != -1){
                stack.Push(t);
            }

            if(b != -1){
                triangles[b].adjacentTriangle[edg(b, t, triangles)] = numTRI - 1;
                stack.Push(numTRI - 1);         
            }

            if(c != -1){
                triangles[c].adjacentTriangle[edg(c, t, triangles)] = numTRI;
                stack.Push(numTRI);
            }

            

            while (stack.Count > 0){
                int l = (int) stack.Pop();
                int r = triangles[l].adjacentTriangle[1];

                int erl = edg(r, l, triangles);
                int era = (erl + 1) % 3;
                int erb = (era + 1) % 3;
                v1 = triangles[r].vertices[erl];
                v2 = triangles[r].vertices[era];
                v3 = triangles[r].vertices[erb];

                if (swap(vertices[v2], vertices[v1], vertices[v3], vertices[i]) == true){

                    a = triangles[r].adjacentTriangle[era];
                    b = triangles[r].adjacentTriangle[erb];
                    c = triangles[l].adjacentTriangle[2];

                    triangles[l].vertices[2] = v3;
                    triangles[l].adjacentTriangle[1] = a;
                    triangles[l].adjacentTriangle[2] = r;

                    triangles[r].vertices[0] = i;
                    triangles[r].vertices[1] = v3;
                    triangles[r].vertices[2] = v1;
                    triangles[r].adjacentTriangle[0] = l;
                    triangles[r].adjacentTriangle[1] = b;
                    triangles[r].adjacentTriangle[2] = c;

                    if(a != -1){
                        triangles[a].adjacentTriangle[edg(a, r, triangles)] = l;
                        stack.Push(l);
                    }

                    if (b != -1){
                        stack.Push(r);
                    }

                    if(c != -1){
                        triangles[c].adjacentTriangle[edg(c, l, triangles)] = r;
                    }
                }
            }
        }

        
        int tStart = 0, tStop = 0;

        for(int t = 0; t < triangles.Count; t++){
            if (triangles[t].vertices[0] > numPTS - 1 ||
                triangles[t].vertices[1] > numPTS - 1 ||
                triangles[t].vertices[2] > numPTS - 1){

                for(int i = 0; i < 3; i++){
                    int a = triangles[t].adjacentTriangle[i];
                    if(a != -1){
                        triangles[a].adjacentTriangle[edg(a, t, triangles)] = -1;
                    }
                }

                tStart = t + 1;
                tStop = numTRI + 1;
                numTRI = t - 1;
                break;
            }
        }

        for (int t = tStart; t < tStop; t++)
        {
            if (triangles[t].vertices[0] > numPTS - 1 ||
                triangles[t].vertices[1] > numPTS - 1 ||
                triangles[t].vertices[2] > numPTS - 1)
            {

                for (int i = 0; i < 3; i++){
                    int a = triangles[t].adjacentTriangle[i];
                    if (a != -1){
                        triangles[a].adjacentTriangle[edg(a, t, triangles)] = -1;
                    }
                }
            }

            else{
                numTRI++;
                for(int i = 0; i < 3; i++){
                    int a = triangles[t].adjacentTriangle[i];
                    triangles[numTRI].adjacentTriangle[i] = a;
                    triangles[numTRI].vertices[i] = triangles[t].vertices[i];
                    
                    if(a != -1){
                        triangles[a].adjacentTriangle[edg(a, t, triangles)] = numTRI;
                    }
                }
            }
        }
 
        triangles.RemoveRange(numTRI + 1, triangles.Count - numTRI - 1);
        

        return triangles;
    }
    public static List<Triangle> triangulateWithConstraints(bool deleteTrianglesOutsideDomain, List<Vector2> verts, List<Vector2Int> constraints)
    {
        double timeD0 = Time.realtimeSinceStartupAsDouble;

        List<Triangle> triangles = triangulate(verts);

        enforceConstraints(triangles, constraints, verts);

        if (deleteTrianglesOutsideDomain){
            identifyTrianglesOutsideDomain(triangles, constraints, verts);
            removeTrianglesOutsideDomain(triangles);
        }


        double timeD1 = Time.realtimeSinceStartupAsDouble;
        Debug.LogWarning("Elapsed Time: " +  (timeD1 - timeD0) + " D0: " + timeD0 + " D1: " + timeD1);

        return triangles;
    }

    static int locateTriangle(Vector2 vertex, List<Vector2> vertices, List<Triangle> triangles, int numTri){
        int t = numTri;

        loop:
        for(int i = 0; i < 3; i++){
            int v1 = triangles[t].vertices[i];
            int v2 = triangles[t].vertices[(i + 1) % 3];

            if ( ((vertices[v1].y - vertex.y) * (vertices[v2].x - vertex.x)) < ((vertices[v1].x - vertex.x) * (vertices[v2].y - vertex.y)) ){
                t = triangles[t].adjacentTriangle[i];
                goto loop;
            }
        }

        return t;
    }
    static int edg(int l, int k, List<Triangle> triangles){
    //Find the edge of triangle l wguch is adjacent to triangle k
        for(int i = 0; i < 3; i++){
           if(k == triangles[l].adjacentTriangle[i]){
                return i;
           }
        }
        return -1;
    }
    static bool swap(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 p){
        Vector2 v13 = v1 - v3;
        Vector2 v23 = v2 - v3;
        Vector2 v1p = v1 - p;
        Vector2 v2p = v2 - p;

        float cosA = (v13.x * v23.x) + (v13.y * v23.y);
        float cosB = (v2p.x * v1p.x) + (v1p.y * v2p.y);

        if(cosA >= 0f && cosB >= 0f) { return false; }
        else if(cosA  < 0f && cosB  < 0f) { return true; }
        else
        {
            float sinA = (v13.x * v23.y) - (v23.x * v13.y);
            float sinB = (v2p.x * v1p.y) - (v1p.x * v2p.y);
            float sinAB = (sinA * cosB) + (sinB * cosA);
            if (sinAB < 0f) { return true; }
            else { return false; }
        }
    }
    static void normalize(List<Vector2> vertices)
    {
        float xMin = vertices[0].x;
        float xMax = xMin;

        float yMin = vertices[0].y;
        float yMax = yMin;

        for (int i = 1; i < vertices.Count; i++){
            xMin = Mathf.Min(xMin, vertices[i].x);
            xMax = Mathf.Max(xMax, vertices[i].x);
            yMin = Mathf.Min(yMin, vertices[i].y);
            yMax = Mathf.Max(yMax, vertices[i].y);
        }

        float factor = 1 / Mathf.Max(xMax - xMin, yMax - yMin);

        for (int i = 0; i < vertices.Count; i++){
            vertices[i] = new Vector2((vertices[i].x - xMin) * factor, (vertices[i].y - yMin) * factor);
        }
    }

    static void enforceConstraints(List<Triangle> triangles, List<Vector2Int> constraints, List<Vector2> verts)
    {
        foreach (Vector2Int constraint in constraints)
        {
            if (edgeIsAlreadyInTriangulation(constraint, triangles)) {
              //  Debug.Log("Already in Triangulation");
                continue;
            }

            List<Vector2Int> intersectingEdges = getIntersectingEdges(triangles, verts, constraint);
            List<Vector2Int> newEdges = new List<Vector2Int>();

            int z = 0;
            while (intersectingEdges.Count > 0 && z < 10000)
            {
                Vector2Int edge = intersectingEdges[0]; intersectingEdges.RemoveAt(0);

                int triangleX = getTriangleByEdgeVertices(edge.x, edge.y, triangles);
                int triangleY = getTriangleByEdgeVertices(edge.y, edge.x, triangles);

                int edgeX = edg(triangleX, triangleY, triangles);
                int edgeY = edg(triangleY, triangleX, triangles);

                int vA = triangles[triangleX].vertices[(edgeX + 1) % 3];
                int vB = triangles[triangleX].vertices[(edgeX + 2) % 3];
                int vC = triangles[triangleY].vertices[(edgeY + 1) % 3];
                int vD = triangles[triangleY].vertices[(edgeY + 2) % 3];

                int eA = triangles[triangleX].adjacentTriangle[(edgeX + 1) % 3];
                int eB = triangles[triangleX].adjacentTriangle[(edgeX + 2) % 3];
                int eC = triangles[triangleY].adjacentTriangle[(edgeY + 1) % 3];
                int eD = triangles[triangleY].adjacentTriangle[(edgeY + 2) % 3];

                if (!isConvexQuadrilateral(verts[vA], verts[vB], verts[vC], verts[vD]))
                {
                    intersectingEdges.Add(new Vector2Int(vA, vC));
                    z++;
                    continue;
                }

                triangles[triangleX].vertices[0] = vD;
                triangles[triangleX].vertices[1] = vB;
                triangles[triangleX].vertices[2] = vC;
                triangles[triangleX].adjacentTriangle[0] = triangleY;
                triangles[triangleX].adjacentTriangle[1] = eB;
                triangles[triangleX].adjacentTriangle[2] = eC;


                triangles[triangleY].vertices[0] = vB;
                triangles[triangleY].vertices[1] = vD;
                triangles[triangleY].vertices[2] = vA;
                triangles[triangleY].adjacentTriangle[0] = triangleX;
                triangles[triangleY].adjacentTriangle[1] = eD;
                triangles[triangleY].adjacentTriangle[2] = eA;

                if (eC != -1)
                {
                    triangles[eC].adjacentTriangle[edg(eC, triangleY, triangles)] = triangleX;
                }

                if (eA != -1)
                {
                    triangles[eA].adjacentTriangle[edg(eA, triangleX, triangles)] = triangleY;
                }


                if (doIntersect(verts[vB], verts[vD], verts[constraint.x], verts[constraint.y]))
                {
                    intersectingEdges.Add(new Vector2Int(vB, vD));
                }

                else
                {
                    newEdges.Add(new Vector2Int(vB, vD));
                }

                z++;
            }

            
            
            for(int i = 0; i < newEdges.Count; i++)
            {

                int triangleX = getTriangleByEdgeVertices(newEdges[i].x, newEdges[i].y, triangles);
                int triangleY = getTriangleByEdgeVertices(newEdges[i].y, newEdges[i].x, triangles);

                int edgeX = edg(triangleX, triangleY, triangles);
                int edgeY = edg(triangleY, triangleX, triangles);

                int vA = triangles[triangleX].vertices[(edgeX + 1) % 3];
                int vB = triangles[triangleX].vertices[(edgeX + 2) % 3];
                int vC = triangles[triangleY].vertices[(edgeY + 1) % 3];
                int vD = triangles[triangleY].vertices[(edgeY + 2) % 3];

                int eA = triangles[triangleX].adjacentTriangle[(edgeX + 1) % 3];
                int eB = triangles[triangleX].adjacentTriangle[(edgeX + 2) % 3];
                int eC = triangles[triangleY].adjacentTriangle[(edgeY + 1) % 3];
                int eD = triangles[triangleY].adjacentTriangle[(edgeY + 2) % 3];

                if (edgeIsEqualToConstraint(constraints, new Vector2Int(newEdges[i].x, newEdges[i].y))) { 
                   // Debug.Log("Already in Triangulation");
                    continue;
                };

                if (swap(verts[vC], verts[vA], verts[vD], verts[vB]))
                {

                 //   Debug.Log("Swapped");

                    triangles[triangleX].vertices[0] = vD;
                    triangles[triangleX].vertices[1] = vB;
                    triangles[triangleX].vertices[2] = vC;
                    triangles[triangleX].adjacentTriangle[0] = triangleY;
                    triangles[triangleX].adjacentTriangle[1] = eB;
                    triangles[triangleX].adjacentTriangle[2] = eC;


                    triangles[triangleY].vertices[0] = vB;
                    triangles[triangleY].vertices[1] = vD;
                    triangles[triangleY].vertices[2] = vA;
                    triangles[triangleY].adjacentTriangle[0] = triangleX;
                    triangles[triangleY].adjacentTriangle[1] = eD;
                    triangles[triangleY].adjacentTriangle[2] = eA;

                    if (eC != -1)
                    {
                        triangles[eC].adjacentTriangle[edg(eC, triangleY, triangles)] = triangleX;
                    }

                    if (eA != -1)
                    {
                        triangles[eA].adjacentTriangle[edg(eA, triangleX, triangles)] = triangleY;
                    }
                }
            }
            
            

        }
    }
    static void identifyTrianglesOutsideDomain(List<Triangle> triangles, List<Vector2Int> constraints, List<Vector2> verts)
    {

        for (int i = 0; i < triangles.Count; i++)
        {
            Vector2 v1 = verts[triangles[i].vertices[0]];
            Vector2 v2 = verts[triangles[i].vertices[1]];
            Vector2 v3 = verts[triangles[i].vertices[2]];

            Vector2 center = (v1 + v2 + v3) / 3;
            Vector2 end = center + new Vector2(10000, 222);

            int intersections = 0;

            foreach (Vector2Int constraint in constraints)
            {
                Vector2 c1 = verts[constraint.x];
                Vector2 c2 = verts[constraint.y];

                if (doIntersect(center, end, c1, c2))
                {
                    intersections++;
                }
            }

            if (intersections % 2 != 0)
            {
                triangles[i].inside = true;
            }
        }
    }
    static void removeTrianglesOutsideDomain(List<Triangle> triangles)
    {
        int tStart = 0, tStop = 0, numTRI = triangles.Count - 1;

        for (int t = 0; t < triangles.Count; t++)
        {
            if (triangles[t].inside == false)
            {
                for (int i = 0; i < 3; i++)
                {
                    int a = triangles[t].adjacentTriangle[i];
                    if (a != -1)
                    {
                        triangles[a].adjacentTriangle[edg(a, t, triangles)] = -1;
                    }
                }

                tStart = t + 1;
                tStop = numTRI + 1;
                numTRI = t - 1;
                break;
            }
        }

        for (int t = tStart; t < tStop; t++)
        {
            if (triangles[t].inside == false)
            {
                for (int i = 0; i < 3; i++)
                {
                    int a = triangles[t].adjacentTriangle[i];
                    if (a != -1)
                    {
                        triangles[a].adjacentTriangle[edg(a, t, triangles)] = -1;
                    }
                }
            }

            else
            {
                numTRI++;
                for (int i = 0; i < 3; i++)
                {
                    int a = triangles[t].adjacentTriangle[i];
                    triangles[numTRI].adjacentTriangle[i] = a;
                    triangles[numTRI].vertices[i] = triangles[t].vertices[i];
                    if (a != -1)
                    {
                        triangles[a].adjacentTriangle[edg(a, t, triangles)] = numTRI;
                    }
                }
            }
        }

        triangles.RemoveRange(numTRI + 1, triangles.Count - numTRI - 1);
    }
    static List<Vector2Int> getIntersectingEdges(List<Triangle> triangles, List<Vector2> vertices, Vector2Int constraint)
    {
        List<Vector2Int> intersectingEdges = new List<Vector2Int>();

        for (int i = 0; i < triangles.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int v1 = triangles[i].vertices[j];
                int v2 = triangles[i].vertices[(j + 1) % 3];

                if (doIntersect(vertices[v1], vertices[v2], vertices[constraint.x], vertices[constraint.y]) && !edgeContained(intersectingEdges, v1, v2))
                {
                    intersectingEdges.Add(new Vector2Int(v1, v2));
                }
            }
        }

        return intersectingEdges;
    }
   
    /*
        static List<Vector2Int> getIntersectingEdges(List<Triangle> triangles, List<Vector2> vertices, Vector2Int constraints) {
            int v1 = constraints.x;
            int v2 = constraints.y;
            int triangle = 0;
            Debug.Log("goal: " + v1 + " " + v2);

            List<Vector2Int> intersectingEdges = new List<Vector2Int>();

            for (int i = 0; i < triangles.Count; i++) {

                int index = Array.IndexOf(triangles[i].vertices, v1);
                if (index != -1) {
                    {
                        int a = (index + 1) % 3;
                        int b = (index + 2) % 3;

                        if (doIntersect(vertices[triangles[i].vertices[a]], vertices[triangles[i].vertices[b]], vertices[v1], vertices[v2]))
                        {
                            Debug.Log("found: " + triangles[i].vertices[a] + " " + triangles[i].vertices[b]);
                            intersectingEdges.Add(new Vector2Int(triangles[i].vertices[a], triangles[i].vertices[b]));
                            triangle = triangles[i].adjacentTriangle[a];
                            break;
                        }
                    }
                }
            }



            int z = 0;
            while(z < 10000 )
            {
                for (int i = 0; i < 3; i++) {
                    int j = (i + 1) % 3;

                    if (triangles[triangle].vertices[i] == v2)
                    {
                        Debug.Log("end");

                        foreach (Vector2Int edge in intersectingEdges)
                        {
                            Debug.Log("intersecting edge " + edge);
                        }

                        return intersectingEdges;
                    }

                    if (doIntersect(vertices[triangles[triangle].vertices[i]], vertices[triangles[triangle].vertices[j]], vertices[v1], vertices[v2]) && !edgeContained(intersectingEdges, triangles[triangle].vertices[i], triangles[triangle].vertices[j]))
                    {
                        Debug.Log("found: " + triangles[triangle].vertices[i] + " " + triangles[triangle].vertices[j]);
                        intersectingEdges.Add(new Vector2Int(triangles[triangle].vertices[i], triangles[triangle].vertices[j]));
                        triangle = triangles[triangle].adjacentTriangle[i];
                        break;
                    }
                }

                z++;
            }

            if (z == 10000)
            {
                Debug.LogError("----------------------------------Error----------------------------");
            }
            return null;
        }
        */

    static bool edgeContained(List<Vector2Int> intersectionEdges, int t1, int t2)
    {
        foreach(Vector2Int edge in intersectionEdges)
        {
            if (edge.x == t1 && edge.y == t2 || edge.x == t2 && edge.y == t1)
            {
                return true;
            }
        }

        return false;
    }
    static public bool doIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2){
        // See https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
        // for a full code explanation.

        int o1 = orientation(p1, q1, p2);
        int o2 = orientation(p1, q1, q2);
        int o3 = orientation(p2, q2, p1);
        int o4 = orientation(p2, q2, q1);

        if (p1 == p2 || p1 == q2 || q1 == p2 || q1 == q2) { return false; }
        if (o1 != o2 && o3 != o4) { return true; }
        return false;
    }
    static bool isConvexQuadrilateral(Vector2 a, Vector2 b, Vector2 c, Vector2 d){
        if (orientation(a, b, c) == orientation(b, c, d) && orientation(b, c, d) == orientation(c, d, a) &&
            orientation(c, d, a) == orientation(d, a, b) && orientation(d, a, b) == orientation(a, b, c)){
            return true;
        }

        return false;
    }
    public static int orientation(Vector2 p, Vector2 q, Vector2 r){
        float val = (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);

        if (val == 0) return 0; // collinear

        return (val > 0) ? 1 : 2; // clock or counterclock wise
    }
    static bool edgeIsAlreadyInTriangulation(Vector2Int edge, List<Triangle> triangles){
        for(int i = 0; i < triangles.Count; ++i){
            if (triangles[i].vertices.Contains(edge.x) && triangles[i].vertices.Contains(edge.y))
            {
                return true;
            }
        }

        return false;
    }
    static int getTriangleByEdgeVertices(int a, int b, List<Triangle> triangles)
    {
        for (int i = 0; i < triangles.Count; ++i)
{
            for(int j = 0; j < 3; j++)
            {
                if (triangles[i].vertices[j] == a && triangles[i].vertices[(j + 1) % 3] == b)
                {
                    return i;
                }
            }
        }

        return -1;
    }
    static bool edgeIsEqualToConstraint(List<Vector2Int> constraints, Vector2Int edge)
    {
        for (int i = 0; i < constraints.Count; ++i)
        {
            if ( (constraints[i].x == edge.x && constraints[i].y == edge.y) || (constraints[i].x == edge.y && constraints[i].y == edge.x)){
                return true;
            }
        }

        return false;
    }

}
public class Triangle{
    public int[] vertices = new int[3];
    public int[] adjacentTriangle = new int[3];
    public bool inside = false;
}



