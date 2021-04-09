using System.Net;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering;
public class ShaderVariantsCollectionWindow : EditorWindow
{
    //系统带的、material.shaderKeywords拿不到的宏添到这里,含有这些关键字的shader会启用对应变体
    HashSet<string> ForceEnabledGlobalKeywords = new HashSet<string>()
    {
        "_MAIN_LIGHT_SHADOWS","_MAIN_LIGHT_SHADOWS_CASCADE","_SHADOWS_SOFT","LIGHTMAP_ON",
        "UNITY_HDR_ON","_SHADOWS_SOFT","_ADDITIONAL_LIGHTS",
    };
    HashSet<string> ForceDisabledGlobalKeywords = new HashSet<string>()
    {

    };
    static List<string> collectingPath = new List<string>() { "Assets" };
    static string savePath = "Assets";
    static ShaderVariantCollection collection;
    static string log;
    [MenuItem("Tools/ShaderVariants搜集工具 &F9")]
    public static void Init()
    {
        var window = GetWindow(typeof(ShaderVariantsCollectionWindow), false, "收集shader变体", true) as ShaderVariantsCollectionWindow;
        window.position = new Rect(600, 400, 400, 100);
        log = string.Empty;
        window.Show();
    }
    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("添加"))
        {
            var path = collectingPath.Count > 0 ? collectingPath[collectingPath.Count - 1] : "Assets";
            var str = $"{EditorUtility.OpenFolderPanel("选择收集路径", path, "")}";
            str = str.Substring(str.LastIndexOf("Assets"));
            var flg = true;
            foreach (var p in collectingPath) if (p.Equals(str)) flg = false;
            if (flg) collectingPath.Add(str);
            log = "Not Collected...";
        }
        if (GUILayout.Button("清理"))
        {
            collectingPath.Clear();
            log = "path is null...";
        }
        GUILayout.Label($"收集路径:    {string.Join("/;", collectingPath.ToArray())}/;");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Print All Macros Used In Shader"))
        {
            string[] keys;
            // var material = Selection.activeObject as Material;
            // if (material == null) return;
            // keys = material.shaderKeywords;
            keys = ShaderUtilImpl.GetAllGlobalKeywords();
            // keys = ShaderUtilImpl.GetShaderLocalKeywords(material.shader);
            // keys = ShaderUtilImpl.GetShaderGlobalKeywords(material.shader);
            // Debug.Log(material.IsKeywordEnabled("_MAIN_LIGHT_SHADOWS"));

            // string[] remainingKeys;
            // string[] filterKeys = null;
            // var passTypes = new int[] { (int)PassType.Normal, (int)PassType.ShadowCaster };
            // ShaderUtilImpl.GetShaderVariantEntriesFiltered(material.shader, 1000, filterKeys, new ShaderVariantCollection(), out passTypes, out keys, out remainingKeys);
            foreach (var key in keys)
            {
                Debug.Log(key);
            }
        }
        if (GUILayout.Button("收集"))
        {
            Collection();
        }
        if (GUILayout.Button("保存"))
        {
            Save();
        }
        GUILayout.Label(log);
    }
    void Collection()
    {
        if (collectingPath.Count <= 0) { log = "Path is null, Select at least one !"; return; }
        collection = new ShaderVariantCollection();
        var materialGUIDs = AssetDatabase.FindAssets("t:Material", collectingPath.ToArray());
        foreach (var guid in materialGUIDs)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            // if (path.EndsWith("FBX") || path.EndsWith("fbx") || path.EndsWith("obj")) continue;
            var material = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;

            // AddVariantOfPassTypeToCollection(PassType.Normal, material);
            // AddVariantOfPassTypeToCollection(PassType.ScriptableRenderPipelineDefaultUnlit, material);
            AddVariantOfPassTypeToCollection(PassType.ScriptableRenderPipeline, material);
            if (material.FindPass("ShadowCaster") != -1) AddVariantOfPassTypeToCollection(PassType.ShadowCaster, material);
        }
        log = $"Found  {collection.shaderCount} Shaders & {collection.variantCount} Variants  Used In All Materials.";
    }
    void AddVariantOfPassTypeToCollection(PassType passType, Material material)
    {
        var shader = material.shader;
        var keywords = new List<string>();
        var shaderAllkeyworlds = GetShaderAllKeyworlds(shader);
        if (shaderAllkeyworlds.Contains("FOG_LINEAR") || shaderAllkeyworlds.Contains("FOG_EXP") || shaderAllkeyworlds.Contains("FOG_EXP2"))
        {
            if (RenderSettings.fog)
            {
                switch (RenderSettings.fogMode)
                {
                    case FogMode.Linear:
                        keywords.Add("FOG_LINEAR");
                        break;
                    case FogMode.Exponential:
                        keywords.Add("FOG_EXP");
                        break;
                    case FogMode.ExponentialSquared:
                        keywords.Add("FOG_EXP2");
                        break;
                    default:
                        break;
                }
            }
        }
        if (material.enableInstancing) keywords.Add("INSTANCING_ON");
        foreach (var key in material.shaderKeywords) keywords.Add(key);
        foreach (var key in ForceEnabledGlobalKeywords) { if (shaderAllkeyworlds.Contains(key)) keywords.Add(key); }
        foreach (var key in ForceDisabledGlobalKeywords) keywords.Remove(key);

        collection.Add(CreateVariant(shader, passType, keywords.ToArray()));
    }
    ShaderVariantCollection.ShaderVariant CreateVariant(Shader shader, PassType passType, string[] keywords)
    {
        // foreach (var k in keywords)
        // {
        //     Debug.Log($"{shader.name}:{passType}:{k}");
        // }
        try
        {
            // var variant = new ShaderVariantCollection.ShaderVariant(shader, passType, keywords);//这构造函数就是个摆设,铁定抛异常(╯‵□′)╯︵┻━┻
            var variant = new ShaderVariantCollection.ShaderVariant();
            variant.shader = shader;
            variant.passType = passType;
            variant.keywords = keywords;
            return variant;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            return new ShaderVariantCollection.ShaderVariant();
        }
    }
    Dictionary<Shader, List<string>> shaderKeyworldsDic = new Dictionary<Shader, List<string>>();
    List<string> GetShaderAllKeyworlds(Shader shader)
    {
        List<string> keyworlds = null;
        shaderKeyworldsDic.TryGetValue(shader, out keyworlds);
        if (keyworlds == null)
        {
            keyworlds = new List<string>(ShaderUtilImpl.GetShaderGlobalKeywords(shader));
            shaderKeyworldsDic.Add(shader, keyworlds);
        }
        return keyworlds;
    }
    void Save()
    {
        var str = $"{EditorUtility.SaveFilePanel("选择保存路径", savePath, "NewShaderVariants", "shadervariants")}";
        savePath = str.Substring(str.LastIndexOf("Assets"));
        if (collection && !string.IsNullOrEmpty(savePath))
        {
            if (File.Exists(str)) AssetDatabase.DeleteAsset(savePath);
            AssetDatabase.CreateAsset(collection, savePath);
            UnityEditor.EditorUtility.FocusProjectWindow();
            UnityEditor.Selection.activeObject = collection;
            log = $"(shader:{collection.shaderCount}, variant:{collection.variantCount}) Collection Saved At: {savePath} !";
            collection = null;//overwrite goes wrong...
        }
        else log = "Not Saved, Please Collect Them First!";
    }

}
public class ShaderUtilImpl
{
    delegate string[] GetShaderGlobalKeywords_type(Shader shader);
    static GetShaderGlobalKeywords_type GetShaderGlobalKeywords_impl;
    public static string[] GetShaderGlobalKeywords(Shader shader)
    {
        if (GetShaderGlobalKeywords_impl == null) GetShaderGlobalKeywords_impl = Delegate.CreateDelegate
        (
            typeof(GetShaderGlobalKeywords_type),
            typeof(UnityEditor.ShaderUtil).GetMethod("GetShaderGlobalKeywords", BindingFlags.Static | BindingFlags.NonPublic)
        ) as GetShaderGlobalKeywords_type;
        return GetShaderGlobalKeywords_impl(shader) as string[];
    }
    delegate string[] GetAllGlobalKeywords_type();
    static GetAllGlobalKeywords_type GetAllGlobalKeywords_impl;
    public static string[] GetAllGlobalKeywords()
    {
        if (GetAllGlobalKeywords_impl == null) GetAllGlobalKeywords_impl = Delegate.CreateDelegate
       (
           typeof(GetAllGlobalKeywords_type),
           typeof(UnityEditor.ShaderUtil).GetMethod("GetAllGlobalKeywords", BindingFlags.Static | BindingFlags.NonPublic)
       ) as GetAllGlobalKeywords_type;
        return GetAllGlobalKeywords_impl() as string[];
    }
    delegate string[] GetShaderLocalKeywords_type(Shader shader);
    static GetShaderLocalKeywords_type GetShaderLocalKeywords_impl;
    public static string[] GetShaderLocalKeywords(Shader shader)
    {
        if (GetShaderLocalKeywords_impl == null) GetShaderLocalKeywords_impl = Delegate.CreateDelegate
       (
           typeof(GetShaderLocalKeywords_type),
           typeof(UnityEditor.ShaderUtil).GetMethod("GetShaderLocalKeywords", BindingFlags.Static | BindingFlags.NonPublic)
       ) as GetShaderLocalKeywords_type;
        return GetShaderLocalKeywords_impl(shader) as string[];
    }
    delegate void GetShaderVariantEntriesFiltered_type(
        Shader shader,
        int maxEntries,
        string[] filterKeywords,
        ShaderVariantCollection excludeCollection,
        out int[] passTypes,
        out string[] keywordLists,
        out string[] remainingKeywords
    );
    static GetShaderVariantEntriesFiltered_type GetShaderVariantEntriesFiltered_impl;
    public static void GetShaderVariantEntriesFiltered(
        Shader shader,
        int maxEntries,
        string[] filterKeywords,
        ShaderVariantCollection excludeCollection,
        out int[] passTypes,
        out string[] keywordLists,
        out string[] remainingKeywords)
    {
        if (GetShaderVariantEntriesFiltered_impl == null) GetShaderVariantEntriesFiltered_impl = Delegate.CreateDelegate
       (
           typeof(GetShaderVariantEntriesFiltered_type),
           typeof(UnityEditor.ShaderUtil).GetMethod("GetShaderVariantEntriesFiltered", BindingFlags.Static | BindingFlags.NonPublic)
       ) as GetShaderVariantEntriesFiltered_type;
        GetShaderVariantEntriesFiltered_impl(shader, maxEntries, filterKeywords, excludeCollection, out passTypes, out keywordLists, out remainingKeywords);
    }

    public struct ShaderVariantEntriesData
    {
        public int[] passTypes;
        public string[] keywordLists;
        public string[] remainingKeywords;
    }
    delegate ShaderVariantEntriesData GetShaderVariantEntriesFilteredInternal_type(
        Shader shader,
        int maxEntries,
        string[] filterKeywords,
        ShaderVariantCollection excludeCollection
  );
    static GetShaderVariantEntriesFilteredInternal_type GetShaderVariantEntriesFilteredInternal_impl;
    public static ShaderVariantEntriesData GetShaderVariantEntriesFilteredInternal(
        Shader shader,
        int maxEntries,
        string[] filterKeywords,
        ShaderVariantCollection excludeCollection)
    {
        if (GetShaderVariantEntriesFilteredInternal_impl == null) GetShaderVariantEntriesFilteredInternal_impl = Delegate.CreateDelegate
       (
           typeof(GetShaderVariantEntriesFilteredInternal_type),
           typeof(UnityEditor.ShaderUtil).GetMethod("GetShaderVariantEntriesFilteredInternal", BindingFlags.Static | BindingFlags.NonPublic)
       ) as GetShaderVariantEntriesFilteredInternal_type;
        return GetShaderVariantEntriesFilteredInternal_impl(shader, maxEntries, filterKeywords, excludeCollection);
    }
}
