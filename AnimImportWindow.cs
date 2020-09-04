using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
// using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngineInternal;

public class AnimImportWindow : EditorWindow {
    string currentPath = "Assets/";
    Dictionary<string, string> replaceDic = new Dictionary<string, string> ();

    void OnEnable () {
        replaceDic.Add ("_001", "@idle");
        replaceDic.Add ("_002", "@idle2");
        replaceDic.Add ("_003", "@walk");
        replaceDic.Add ("_004", "@attack");
        replaceDic.Add ("_005", "@attack2");
        replaceDic.Add ("_006", "@beattack");
        replaceDic.Add ("_007", "@dead");
        replaceDic.Add ("_008", "@attack3");
        replaceDic.Add ("_009", "@attack4");
    }

    [MenuItem ("Tools/AnimeImport")]
    private static void ShowWindow () {
        var window = GetWindow<AnimImportWindow> (false, "AnimImportWindow");
    }

    void ChangeFileName (FileInfo f, string folderName) {
        var oldName = f.Name;
        var newName = GetNewName (oldName);
        var path = f.FullName.Substring (f.FullName.LastIndexOf ("Assets"));
        AssetDatabase.RenameAsset (path, newName);

    }

    string GetNewName (string s) {
        string str = s;
        foreach (var key in replaceDic) {
            Debug.Log (key.Key);
            if (s.Contains (key.Key)) {
                str = s.Replace (key.Key, key.Value);
            }
        }
        Debug.Log (str);
        return str;
    }
    void OnGUI () {
        EditorGUILayout.LabelField ("当前文件夹: " + currentPath);
        if (GUILayout.Button ("选择文件夹")) {
            string s = EditorUtility.OpenFolderPanel ("选择文件夹", currentPath, "");
            if ( /*AssetDatabase.IsValidFolder (p) && */ s.Contains ("Assets/")) {
                currentPath = s;
            } else {
                Debug.Log ("不是有效文件夹! :" + s);
            }
        }

        if (GUILayout.Button ("Process")) {
            // ChangeNameProcess ();
            SettingAnimatorProcess ();

            AssetDatabase.SaveAssets ();
            AssetDatabase.Refresh ();
        }
    }
    AnimatorController CreatAnimatorController (string path) {
        var controller = AnimatorController.CreateAnimatorControllerAtPath (path);

        var layer1 = new AnimatorControllerLayer ();
        layer1.stateMachine = new AnimatorStateMachine ();
        layer1.name = "Attack";
        layer1.defaultWeight = 1;
        var state = new AnimatorState ();
        state.name = "Stop";
        layer1.stateMachine.AddState (state, new Vector3 (30, 200, 0)); //Attack 层添加一个空节点Stoop,???
        layer1.stateMachine.AddEntryTransition (state);
        layer1.stateMachine.AddAnyStateTransition (state);
        controller.AddLayer (layer1);

        var layer2 = new AnimatorControllerLayer ();
        layer2.stateMachine = new AnimatorStateMachine ();
        layer2.name = "Death";
        layer2.defaultWeight = 1;
        controller.AddLayer (layer2);

        return controller;
    }
    void SetupAnimatorController (ref AnimatorController controller, List<AnimationClip> clips) {
        for (int i = 0; i < clips.Count; i++) {
            AddClipToAnimator (ref controller, clips[i]);
        }
    }
    AnimatorState AddAnimatorState (AnimatorStateMachine machine, AnimationClip clip, Vector3 pos) {
        var state = new AnimatorState ();
        state.name = clip.name;
        state.motion = clip;
        machine.AddState (state, pos);
        return state;
    }
    void AddClipToAnimator (ref AnimatorController controller, AnimationClip clip) {
        AnimatorState state = null;
        switch (clip.name) {
            case "idle":
                state = AddAnimatorState (controller.layers[0].stateMachine, clip, new Vector3 (300, 40, 0));
                controller.layers[0].stateMachine.AddEntryTransition (state);
                break;
            case "idle2":
                AddAnimatorState (controller.layers[0].stateMachine, clip, new Vector3 (300, 100, 0));
                break;
            case "walk":
                AddAnimatorState (controller.layers[0].stateMachine, clip, new Vector3 (300, 160, 0));
                break;
            case "attack":
                AddAnimatorState (controller.layers[1].stateMachine, clip, new Vector3 (300, 40, 0));

                break;
            case "attack2":
                AddAnimatorState (controller.layers[1].stateMachine, clip, new Vector3 (300, 100, 0));
                break;
            case "attack3":
                AddAnimatorState (controller.layers[1].stateMachine, clip, new Vector3 (300, 160, 0));
                break;
            case "attack4":
                AddAnimatorState (controller.layers[1].stateMachine, clip, new Vector3 (300, 220, 0));
                break;
            case "beattack":
                AddAnimatorState (controller.layers[1].stateMachine, clip, new Vector3 (300, 280, 0));
                break;
            case "dead":
                state = AddAnimatorState (controller.layers[2].stateMachine, clip, new Vector3 (300, 40, 0));
                controller.layers[2].stateMachine.AddEntryTransition (state);
                break;
            default:
                // Debug.Log ("未定义片段:" + clip.name);
                break;
        }
    }

    void SetAnimator (DirectoryInfo dir) {
        var animators = dir.GetFiles ("*.controller", SearchOption.TopDirectoryOnly);
        string animatorPath;
        if (animators.Length != 0) {
            animatorPath = animators[0].FullName.Substring (animators[0].FullName.LastIndexOf ("Assets"));
        } else {
            animatorPath = dir.FullName.Substring (dir.FullName.LastIndexOf ("Assets"));
            animatorPath = animatorPath + "\\" + "AniCtr_" + dir.Name + ".controller";
        }
        Debug.Log (animatorPath);
        var controller = CreatAnimatorController (animatorPath);
        var files = dir.GetFiles ("*.fbx", SearchOption.TopDirectoryOnly);
        var animClips = new List<AnimationClip> ();
        for (int i = 0; i < files.Length; i++) {
            if (!files[i].Name.Contains ("@")) continue; //跳过空片段，todo：清理空片段
            var animPath = files[i].FullName.Substring (files[0].FullName.LastIndexOf ("Assets"));
            var anim = AssetDatabase.LoadAssetAtPath<AnimationClip> (animPath);
            animClips.Add (anim);
        }
        SetupAnimatorController (ref controller, animClips);
        AssetDatabase.SaveAssets ();
        AssetDatabase.Refresh ();
    }

    void SettingAnimatorProcess () {
        var dirInfo = new DirectoryInfo (currentPath);
        var dirs = dirInfo.GetDirectories ();
        for (int i = 0; i < dirs.Length; i++) {
            var dir = dirs[i];
            SetAnimator (dir);
        }
    }

    void ChangeNameProcess () {
        var dirInfo = new DirectoryInfo (currentPath);
        var dirs = dirInfo.GetDirectories ();
        for (int i = 0; i < dirs.Length; i++) {
            var dir = dirs[i];
            var files = dir.GetFiles ("*.fbx", SearchOption.TopDirectoryOnly);
            for (int j = 0; j < files.Length; j++) {
                // RenameBeAttack (files[j]);
                ChangeFileName (files[j], dir.Name);
                // if (files[j].Name.Contains ("idle")) ChangeAnimeLoopState (files[j].FullName);
            }
        }
    }

    void RenameBeAttack (FileInfo f) {
        Debug.Log (f.Name);
        var newName = f.Name.Replace ("beatack", "beattack");
        var path = f.FullName.Substring (f.FullName.LastIndexOf ("Assets"));
        AssetDatabase.RenameAsset (path, newName);
        AssetDatabase.SaveAssets ();
        AssetDatabase.Refresh ();
    }

    void ChangeAnimeLoopState (string filePath) {
        // Debug.Log (filePath);
        var path = filePath.Substring (filePath.LastIndexOf ("Assets"));

        ModelImporter importer = AssetImporter.GetAtPath (path) as ModelImporter;
        importer.animationWrapMode = WrapMode.Loop;
        var impClips = importer.defaultClipAnimations;
        foreach (var clip in impClips) {
            clip.loop = true;
            clip.loopTime = true;
            clip.loopPose = true;
        }

        importer.clipAnimations = impClips;
        importer.SaveAndReimport ();
    }

}