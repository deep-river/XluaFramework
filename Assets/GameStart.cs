using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{

    public GameMode GameMode;

    // Start is called before the first frame update
    void Start()
    {
        AppConst.GameMode = GameMode;
        DontDestroyOnLoad(this);
        TestLuaScript();
    }

    private void TestLuaScript()
    {
        Manager.Resource.ParseVersionFile();
        // 非编辑模式下Lua bundle为异步加载，因此此处以异步回调方式在初始化完成后执行Lua脚本以避免在加载完成前执行Lua脚本
        Manager.Lua.Init(()=>
        {
            Debug.Log("Lua main Init!");
            Manager.Lua.StartLua("main");
            XLua.LuaFunction func = Manager.Lua.LuaEnv.Global.Get<XLua.LuaFunction>("Main");
            func.Call();
        });
    }
}
