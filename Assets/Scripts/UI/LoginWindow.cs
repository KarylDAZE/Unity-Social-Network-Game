using UnityEngine;
using UnityEngine.UI;
using SK.Framework;
using SK.Framework.UI;
using ProtoBuf;
using Mgr;

namespace UI
{
    public class LoginWindow : UIView
    {
        [SerializeField]
        private Text Username_Text;
        [SerializeField]
        private Text Password_Text;
        [SerializeField]
        private Button Login_Button;
        [SerializeField]
        private Button Exit_Button;

        protected override void OnInit(IViewData data)
        {
        }

        protected override void BindListeners()
        {
            Login_Button.onClick.AddListener(() =>
            {
                //check input
                if (string.IsNullOrEmpty(Username_Text.text) || string.IsNullOrEmpty(Password_Text.text))
                {
                    Main.UI.LoadView("TipsWindow", UIConst.TipsWindow, ViewLevel.TIPS, out _, new TipsData
                    {
                        tipsText = "Please input username and password",
                        isShowConfirm = true,
                        isShowCancel = false,
                    }, true);
                    return;
                }

                LoginMgr.Instance.SendLogin(Username_Text.text, Password_Text.text);
            });

            Exit_Button.onClick.AddListener(() =>
            {
                Main.UI.LoadView("TipsWindow", UIConst.TipsWindow, ViewLevel.TIPS, out _, new TipsData
                {
                    tipsText = "确定要退出游戏吗",
                    isShowConfirm = true,
                    isShowCancel = true,
                    onConfirm = () =>
                    {
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
                    }
                }, true);
            });
        }

        protected override void BindEvents()
        {
            Main.Events.Subscribe<bool>(Multiplayer.ProtoEventID.LoginRes, OnLoginRes);
        }

        void OnLoginRes(bool isSuccess)
        {
            string tipsText = isSuccess ? "Login Success!" : "Login Failed!";
            Main.UI.LoadView("TipsWindow", UIConst.TipsWindow, ViewLevel.TIPS, out _, new TipsData
            {
                tipsText = tipsText,
                isShowConfirm = true,
                isShowCancel = false,
                onConfirm = () =>
                {
                    if (isSuccess)
                    {
                        Main.UI.LoadView("MainWindow", UIConst.MainWindow, ViewLevel.NORMAL, out _, null, true);
                        Unload();
                    }
                },
            }, true);
        }
    }
}