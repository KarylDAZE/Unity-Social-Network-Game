using UnityEngine;
using SK.Framework;
using SK.Framework.UI;

public class EnterGameScene : MonoBehaviour
{
    private IUIView loginWindow;

    void Start()
    {
        if (-2 == Main.UI.LoadView("LoginWindow", UIConst.LoginWindow, ViewLevel.NORMAL, out loginWindow, null, true))
        {
            Debug.LogError("Load LoginWindow failed");
            return;
        }

        // network connect
        Main.Custom.Network.Connect("127.0.0.1", 8801);
    }

    void Update()
    {

    }
}
