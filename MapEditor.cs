using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapEditor : EditorWindow {
    struct hitInfo {
        public int triID;
        public float distance;
        public Vector3 position;
        public Vector3 normal;
    }
    int brushModeIndex = 0;
    float brushRadius = 0.01f;
    float brushWeight = 0.5f;

    bool isEditing = false;
    GameObject target;
    Mesh orginalMesh, mesh;
    string path;
    string[] brushMode = { "sculpt", "subdivid" };

    [MenuItem ("Tools/打开地图编辑窗口 %#x")]
    private static void init () {
        GetWindow<MapEditor> ().Show ();
    }

    Mesh backUpMesh (Mesh origin) {
        Mesh m = new Mesh ();
        m.vertices = origin.vertices;
        m.normals = origin.normals;
        m.tangents = origin.tangents;
        m.uv = orginalMesh.uv;
        m.uv2 = orginalMesh.uv2;
        m.uv3 = orginalMesh.uv3;
        m.uv4 = orginalMesh.uv4;
        m.triangles = origin.triangles;
        m.RecalculateNormals ();
        m.RecalculateTangents ();
        m.RecalculateBounds ();

        return m;
    }

    private void OnGUI () {
        EditorGUILayout.BeginVertical ();
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
            Debug.Log (target.name);
            mesh = target.GetComponent<MeshFilter> ().sharedMesh;
            orginalMesh = Object.Instantiate (mesh) as Mesh;
            mesh = Object.Instantiate (mesh) as Mesh;
            isEditing = true;
        } else { isEditing = false; }
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
        if (isEditing && null != mesh) {
            Ray mouseRay = HandleUtility.GUIPointToWorldRay (e.mousePosition);
            Ray localray = InverseTransformRay (target.transform, mouseRay);
            hitInfo hit = new hitInfo ();
            if (meshRaycast (localray, mesh, ref hit)) {
                Handles.color = Color.green;
                Handles.DrawWireDisc (target.transform.TransformPoint (hit.position), target.transform.TransformDirection (hit.normal), 0.2f);
                Handles.color = Color.green;                
            }
        }
    }

    void drawTest (Vector3 pos) {
        Debug.Log ("pos" + pos);
        Handles.color = Color.green;
        Handles.DrawWireDisc (pos, Vector3.up, 0.05f);
        Handles.color = Color.white;

    }
    bool meshRaycast (Ray ray, Mesh m, ref hitInfo hit) {
        Vector3 a, b, c;
        int[] tri = mesh.GetTriangles (0);

        for (int curTri = 0; curTri < tri.Length; curTri = curTri + 3) {
            a = m.vertices[tri[curTri + 0]];
            b = m.vertices[tri[curTri + 1]];
            c = m.vertices[tri[curTri + 2]];
            if (RayIntersectsTriangle (ray.origin, ray.direction, a, b, c, ref hit)) {
                hit.triID = curTri;
                hit.position = ray.GetPoint (hit.distance);
                return true;
            }
        }
        return false;
    }

    //http://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
    static bool RayIntersectsTriangle (Vector3 o, Vector3 d, Vector3 a, Vector3 b, Vector3 c, ref hitInfo hit) {
        Vector3 e1, e2, p, q, t;
        float det, u, v;
        e1 = b - a;
        e2 = c - a;
        p = Vector3.Cross (d, e2);
        det = Vector3.Dot (e1, p);
        if (det < Mathf.Epsilon) return false;
        q = o - a;
        u = Vector3.Dot (q, p);
        if (u < 0 || u > det) return false;
        t = Vector3.Cross (q, e1);
        v = Vector3.Dot (d, t);
        if (v < 0 || u + v > det) return false;
        hit.distance = Vector3.Dot (e2, t) * (1 / det);
        if (hit.distance > Mathf.Epsilon) {
            hit.normal = Vector3.Cross (e1, e2);
            return true;
        }

        return false;
    }

    public Ray InverseTransformRay (Transform transform, Ray InWorldRay) {
        Vector3 o = InWorldRay.origin;
        o -= transform.position;
        o = transform.worldToLocalMatrix * o;
        Vector3 d = transform.worldToLocalMatrix.MultiplyVector (InWorldRay.direction);
        return new Ray (o, d);
    }

    void subDivideMesh (Mesh m) {

    }

    void applyMesh (Mesh m) {
        target.GetComponent<MeshFilter> ().mesh = m;
    }
}

public class kmesh {
    List<Vector3> vertices;
    List<int> triangles;
    List<Vector2> uv;
    List<Vector2> uv1;
    List<Vector2> uv2;
    List<Vector2> uv3;
    public kmesh (Mesh mesh) {
        for (int i = 0; i < mesh.vertices.Length; ++i) this.vertices.Add (mesh.vertices[i]);
        for (int i = 0; i < mesh.triangles.Length; ++i) this.triangles.Add (mesh.triangles[i]);
        for (int i = 0; i < mesh.uv.Length; ++i) this.uv.Add (mesh.uv[i]);
    }

}
