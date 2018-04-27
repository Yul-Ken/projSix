using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapEditor : EditorWindow {
    public class hitInfo {
        public int triID;
        public Vector3 position;
        public Vector3 normal;
    }
    int brushModeIndex = 0;
    float brushRadius = 0.01f;
    float brushWeight = 0.5f;

    bool isEditing = false;
    GameObject target;
    Mesh orginalMesh;
    KMesh mesh;
    string path;
    string[] brushMode = { "sculpt", "subdivid" };

    [MenuItem ("Tools/打开地图编辑窗口 %#x")]
    private static void init () {
        GetWindow<MapEditor> ().Show ();
    }

    // Mesh backUpMesh (Mesh origin) {
    //     Mesh m = new Mesh ();
    //     m.vertices = origin.vertices;
    //     m.normals = origin.normals;
    //     m.tangents = origin.tangents;
    //     m.uv = orginalMesh.uv;
    //     m.uv2 = orginalMesh.uv2;
    //     m.uv3 = orginalMesh.uv3;
    //     m.uv4 = orginalMesh.uv4;
    //     m.triangles = origin.triangles;
    //     m.RecalculateNormals ();
    //     m.RecalculateTangents ();
    //     m.RecalculateBounds ();

    //     return m;
    // }

    private void OnGUI () {
        EditorGUILayout.BeginVertical ();
        if (mesh == null) {
            GUILayout.Label ("current obj: null");
        } else {
            GUILayout.Label ("current obj: " + mesh.name);
        }
        GUILayout.Space (10);
        EditorGUI.BeginChangeCheck ();
        brushModeIndex = GUILayout.Toolbar (brushModeIndex, brushMode);
        brushRadius = EditorGUILayout.Slider ("brushRadius", brushRadius, 0.01f, 5f);
        brushWeight = EditorGUILayout.Slider ("brushWeight", brushWeight, 0.01f, 1f);
        if (GUILayout.Button ("subDivide")) {
            subDivideMesh (mesh);

        }
        if (GUILayout.Button ("reset")) {
            target.GetComponent<MeshFilter> ().sharedMesh = orginalMesh;
        }

        EditorGUILayout.EndVertical ();

    }

    private void OnEnable () {
        SceneView.onSceneGUIDelegate -= updateSceneGUI;
        SceneView.onSceneGUIDelegate += updateSceneGUI;
    }
    private void OnDisable () {
        SceneView.onSceneGUIDelegate -= updateSceneGUI;
    }

    void OnSelectionChange () {
        if (null != Selection.activeGameObject && null != Selection.activeGameObject.GetComponent<MeshFilter> ()) {
            target = Selection.activeGameObject;
            orginalMesh = target.GetComponent<MeshFilter> ().sharedMesh;
            mesh = new KMesh (orginalMesh);
        }
        // orginalMesh = Object.Instantiate (mesh) as Mesh;
        // mesh = Object.Instantiate (mesh) as Mesh;
        if (null != mesh)
            isEditing = true;
        else isEditing = false;
    }

    void saveMesh (Mesh m) {
        path = EditorUtility.SaveFilePanel ("save mesh Asset", "Asset/", mesh.name, "asset");
        Debug.Log (path);
        if (string.IsNullOrEmpty (path)) return;
        path = FileUtil.GetProjectRelativePath (path);
        AssetDatabase.CreateAsset (m, path);
        AssetDatabase.SaveAssets ();
    }

    void updateSceneGUI (SceneView sv) {
        Event e = Event.current;
        if (isEditing) {
            Ray mouseRay = HandleUtility.GUIPointToWorldRay (e.mousePosition);
            Ray localray = KMath.InverseTransformRay (target.transform, mouseRay);
            hitInfo hit = new hitInfo ();

            // if (meshRaycast (localray, mesh, ref hit)) {
            //     Handles.color = Color.green;
            //     Handles.DrawWireDisc (target.transform.TransformPoint (hit.position), target.transform.TransformDirection (hit.normal), 0.2f);
            //     Handles.color = Color.green;
            // }
        }
        Repaint ();
    }

    void drawTest (Vector3 pos) {
        Debug.Log ("pos" + pos);
        Handles.color = Color.green;
        Handles.DrawWireDisc (pos, Vector3.up, 0.05f);
        Handles.color = Color.white;

    }
    bool meshRaycast (Ray ray, KMesh m, ref hitInfo hit) {
        Vector3 a, b, c;
        int[] tri = mesh.GetTriangles ();

        for (int curTri = 0; curTri < tri.Length; curTri = curTri + 3) {
            a = m.vertices[tri[curTri + 0]];
            b = m.vertices[tri[curTri + 1]];
            c = m.vertices[tri[curTri + 2]];
            if (KMath.rayIntersectsTriangle (ref ray, ref a, ref b, ref c, ref hit.position, ref hit.normal)) {
                hit.triID = curTri;
                return true;
            }
        }
        return false;
    }

    void subDivideMesh (KMesh m) {

    }

    void applyMesh (Mesh m) {
        target.GetComponent<MeshFilter> ().mesh = m;
    }
}
