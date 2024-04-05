using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.MessageBox;

public class Tester : MonoBehaviour
{
    public List<Vector2> vertices = new List<Vector2>();
    public List<Vector2Int> constraints = new List<Vector2Int>();
    List<Triangle> trianglesWC;
    List<Triangle> triangles;
    public bool run = false;
    public bool remove = false;

    private void Start()
    {
        vertices.Clear();

        for (int i = 0; i < 500; i++)
        {
            Vector2 randomVector = new Vector2(Random.Range(0f, 50f), Random.Range(0f, 50f));
            
            if(!vertices.Contains(randomVector))
            {
                vertices.Add(randomVector);
            }

        }
    }

    void Update()
    {
        if (run)
        {
            //run = false;

            trianglesWC = DelaunayTriangulator.triangulateWithConstraints(remove, vertices, constraints);
         
        }
    }

    private void OnDrawGizmos()
    {

        if (vertices != null)
        {
            Gizmos.color = Color.red;

            for(int i = 0; i < vertices.Count; i++)
            {
                Gizmos.DrawSphere(vertices[i], 0.1f);
                //Handles.Label(vertices[i] + new Vector2(0.3f, 0f), i.ToString());
            }
        }



        //triangles = DelaunayTriangulator.triangulate(vertices);
        if (triangles != null)
        {
            Gizmos.color = Color.green;
            foreach (Triangle t in triangles)
            {
                Gizmos.DrawLine(vertices[t.vertices[0]], vertices[t.vertices[1]]);
                Gizmos.DrawLine(vertices[t.vertices[1]], vertices[t.vertices[2]]);
                Gizmos.DrawLine(vertices[t.vertices[2]], vertices[t.vertices[0]]);

                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.red;
                Handles.Label(vertices[t.vertices[0]] + new Vector2(0.3f, 0f), t.vertices[0].ToString(), style);
                Handles.Label(vertices[t.vertices[1]] + new Vector2(0.3f, 0f), t.vertices[1].ToString(), style);
                Handles.Label(vertices[t.vertices[2]] + new Vector2(0.3f, 0f), t.vertices[2].ToString(), style);

                Vector2 mid = vertices[t.vertices[0]] + vertices[t.vertices[1]] +vertices[t.vertices[2]];
                mid /= 3;

                /*
                style.normal.textColor = Color.blue;
                Vector2 mE = (vertices[t.vertices[0]] + vertices[t.vertices[1]]) / 2;
                mE = (mE + mE + mid) / 3;
                Handles.Label(mE, t.adjacentTriangle[0].ToString(), style);

                mE = (vertices[t.vertices[1]] + vertices[t.vertices[2]]) / 2;
                mE = (mE + mE + mid) / 3;
                Handles.Label(mE, t.adjacentTriangle[1].ToString(), style);

                mE = (vertices[t.vertices[2]] + vertices[t.vertices[0]]) / 2;
                mE = (mE + mE + mid) / 3;
                Handles.Label(mE, t.adjacentTriangle[2].ToString(), style);
                */

                style.normal.textColor = Color.green;
                Handles.Label(mid, triangles.IndexOf(t).ToString(), style);
            }



        }


        foreach (Vector2Int c in constraints)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(vertices[c.x], vertices[c.y]);
        }


        if (trianglesWC != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Triangle t in trianglesWC)
            {
                Gizmos.DrawLine(vertices[t.vertices[0]], vertices[t.vertices[1]]);
                Gizmos.DrawLine(vertices[t.vertices[1]], vertices[t.vertices[2]]);
                Gizmos.DrawLine(vertices[t.vertices[2]], vertices[t.vertices[0]]);

            }

        }



    }


}
