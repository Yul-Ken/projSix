using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class replaceSaderWindow : EditorWindow {
    public Shader oldShader;
    public Shader newShader;
    [MenuItem ("Window/一键替换shader")]
    static void Init () {
        Rect wr = new Rect (0, 0, 500, 200);
        var window = (replaceSaderWindow) EditorWindow.GetWindowWithRect (typeof (replaceSaderWindow), wr, true, "将所有材质球中\"oldShader\"替换为\"newShader\"");
        window.Show ();
    }
    void OnGUI () {
        GUI.enabled = !EditorApplication.isPlaying;
        // GUILayout.BeginHorizontal ();
        oldShader = EditorGUILayout.ObjectField ("oldShader", oldShader, typeof (Shader)) as Shader;
        newShader = EditorGUILayout.ObjectField ("newShader", newShader, typeof (Shader)) as Shader;
        if (GUILayout.Button ("交换")) {
            Shader temp = oldShader;
            oldShader = newShader;
            newShader = temp;
        }
        // GUILayout.EndHorizontal ();
        if (GUILayout.Button ("替换")) {
            replace ();
        }
        // EditorGUI.LabelField (new Rect (3,90, position.width, 20), "将工程中使用oldSahder的材质球替换为newsShader");
    }
    void replace () {
        if (oldShader == null || newShader == null) {
            Debug.Log ("shader不能为空");
            return;
        }
        string[] allMaterialsPath = AssetDatabase.FindAssets ("t:Material", null);
        List<Material> tempMaterials = new List<Material> ();
        for (int i = 0; i < allMaterialsPath.Length; ++i) {
            Material mat = AssetDatabase.LoadAssetAtPath (AssetDatabase.GUIDToAssetPath (allMaterialsPath[i]), typeof (Material)) as Material;
            if (mat.shader.name == oldShader.name) {
                tempMaterials.Add (mat);
            }
        }
        if (tempMaterials.Count > 0) {
            for (int i = 0; i < tempMaterials.Count; i++) {
                tempMaterials[i].shader = newShader;
            }
        }
    }

}