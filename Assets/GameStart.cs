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
        // �Ǳ༭ģʽ��Lua bundleΪ�첽���أ���˴˴����첽�ص���ʽ�ڳ�ʼ����ɺ�ִ��Lua�ű��Ա����ڼ������ǰִ��Lua�ű�
        Manager.Lua.Init(()=>
        {
            Debug.Log("Lua main Init!");
            Manager.Lua.StartLua("main");
            XLua.LuaFunction func = Manager.Lua.LuaEnv.Global.Get<XLua.LuaFunction>("Main");
            func.Call();
        });
    }
}
