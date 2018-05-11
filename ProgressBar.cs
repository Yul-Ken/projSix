using System;
using System.Collections;
using System.Collections.Generic;
using KFrameWork;
using LuaInterface;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : GameUI {
    // [Range (0, 1)]
    // [SerializeField]
    // public SliderExpand progressSlider;
    public ImageExpand background;
    public ImageExpand filler;

    // public ImageExpand percentBG;
    public TextExpand percentLable;

    protected override void OnEnter (AbstractParams p) {

    }

    protected override void OnExit (AbstractParams p) {

    }

    protected override void Refresh (AbstractParams p) {

    }

    protected override void Release () {

    }





    [Button]
    void RunTest () {
        LuaState lua = new LuaState ();
        lua.Start ();
        string path = Application.dataPath + "/UICLassGenerate";
        lua.AddSearchPath (path);
        LuaTable table = (lua.DoFile ("ProgressBar")) [0] as LuaTable;
        lua.DoFile("test");

        // LuaFunction awakeFunc = table.GetLuaFunction ("Awake");
        // awakeFunc.BeginPCall ();
        // awakeFunc.Push (table);
        // awakeFunc.Push  (this.gameObject);
        // awakeFunc.PCall ();
        // awakeFunc.EndPCall ();

        // LuaFunction testFunc = table.GetLuaFunction ("testFunc");
        // testFunc.BeginPCall ();
        // testFunc.Push (table);
        // testFunc.Push (filler);
        // testFunc.PCall ();
        // testFunc.EndPCall ();

        lua.CheckTop ();
        lua.Dispose ();
        lua = null;
    }
}