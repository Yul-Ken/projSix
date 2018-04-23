using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class subDivide : MonoBehaviour {

    Mesh mesh;

    [System.NonSerialized]
    int[] oldTriangles;
    [System.NonSerialized]
    List<int> triangles = new List<int> ();
    [System.NonSerialized]
    List<Vector3> verts = new List<Vector3> ();

    [System.NonSerialized]
    List<Vector2> uv = new List<Vector2> ();

    private void init () {
        mesh = GetComponent<MeshFilter> ().sharedMesh;
        mesh.GetVertices (verts);
        mesh.GetUVs (0, uv);
        oldTriangles = mesh.triangles;
    }

    [ContextMenu ("subDivideMesh")]
    public void subDivideMesh () {
        init ();

        for (int triID = 0; triID < oldTriangles.Length; triID += 3) {
            reTriangle (triID, verts.Count);

            Vector3 v = insertVert (triID);
            verts.Add (v);

            // Vector2 uvCoordinat = insertUV (triID);
            // uv.Add (uvCoordinat);
        }

        mesh.RecalculateNormals ();
        // mesh.RecalculateTangents();

        mesh.SetVertices (verts);
        // mesh.SetUVs (0, uv);
        mesh.SetTriangles (triangles, 0, true);

        verts.Clear();
        uv.Clear();
        triangles.Clear();

    }

    Vector2 insertUV (int id) {
        return ((mesh.uv[oldTriangles[id]] + mesh.uv[oldTriangles[id + 1]] + mesh.uv[oldTriangles[id + 2]]) / 3);
    }
    Vector3 insertVert (int id) {
        return ((verts[oldTriangles[id]] + verts[oldTriangles[id + 1]] + verts[oldTriangles[id + 2]]) / 3);
    }

    void reTriangle (int tri, int v) {
        triangles.Add (oldTriangles[tri]);
        triangles.Add (oldTriangles[tri + 1]);
        triangles.Add (v);

        triangles.Add (oldTriangles[tri + 1]);
        triangles.Add (oldTriangles[tri + 2]);
        triangles.Add (v);

        triangles.Add (oldTriangles[tri + 2]);
        triangles.Add (oldTriangles[tri]);
        triangles.Add (v);
    }

    [ContextMenu ("showMeshInfo")]
    void showMeshInfo () {
        if (mesh == null) init ();
        // foreach(Vector3 v in mesh.vertices){
        //     Debug.Log(v);
        Debug.Log ("verts:" + mesh.vertices.Length);
        Debug.Log ("triangles:" + mesh.triangles.Length / 3);


    }
    // Vector3[] v = 
    // {
    //     new Vector3(-0.5f,-0.5f,0),
    //     new Vector3(0.5f,0.5f,0),
    //     new Vector3(0.5f,-0.5f,0),
    //     new Vector3(-0.5f,0.5f,0)
    // };
}