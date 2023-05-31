using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // ª∫¥ÊUI
    Dictionary<string, GameObject> m_UI = new Dictionary<string, GameObject>();

    public void OpenUI(string uiName, string luaName)
    {
        GameObject ui = null;
        if (m_UI.TryGetValue(uiName, out ui))
        {
            UILogic uiLogic = ui.GetComponent<UILogic>();
            uiLogic.OnOpen();
            return;
        }

        Manager.Resource.LoadUI(uiName, (UnityEngine.Object obj) =>
        {
            ui = Instantiate(obj) as GameObject;
            m_UI.Add(uiName, ui);
            UILogic uiLogic = ui.AddComponent<UILogic>();
            uiLogic.Init(luaName);
            uiLogic.OnOpen();
        });
    }
}
