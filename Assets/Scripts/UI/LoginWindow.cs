using UnityEngine;
using UnityEngine.UI;
using SK.Framework;
using SK.Framework.UI;
using ProtoBuf;

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
            // Username_Text = GameObject.Find("Account_Text").GetComponent<Text>();
            // Password_Text = GameObject.Find("Password_Text").GetComponent<Text>();
            // Login_Button = GameObject.Find("Login_Button").GetComponent<Button>();
            // Exit_Button = transform.Find("Exit_Button").GetComponent<Button>();
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

                //send proto
                var loginArg = new proto.Login.LoginArg
                {
                    username = Username_Text.text,
                    //password encryption
                    password = Password_Text.text
                };
                Main.Custom.Network.Send(loginArg);
            });
            Exit_Button.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }

        protected override void BindEvents()
        {
            Main.Events.Subscribe<IExtensible>(Multiplayer.ProtoEventID.LoginRes, OnLoginRes);
        }

        void OnLoginRes(IExtensible proto)
        {
            var res = proto as proto.Login.LoginRes;
            string tipsText = 0 == res.ErrCode ? "Login Success!" : "Login Failed!";
            Main.UI.LoadView("TipsWindow", UIConst.TipsWindow, ViewLevel.TIPS, out _, new TipsData
            {
                tipsText = tipsText,
                isShowConfirm = true,
                isShowCancel = false,
            }, true);
            if (0 == res.ErrCode)
            {
                Unload();
            }
        }
    }
}