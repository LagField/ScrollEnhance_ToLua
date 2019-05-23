using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;

public class LuaStart : MonoBehaviour
{
    private LuaState luaState;

    private void Start()
    {
        luaState = new LuaState();
        luaState.Start();
        luaState.AddSearchPath(Application.dataPath + "\\ScrollEnhancement");
        DelegateFactory.Init();         
        LuaBinder.Bind(luaState);
        LuaLooper looper = gameObject.AddComponent<LuaLooper>();
        looper.luaState = luaState;

        luaState.Require("Test.Main");
    }

    private void OnDestroy()
    {
        luaState.Dispose();
    }
}