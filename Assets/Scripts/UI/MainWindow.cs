using UnityEngine;
using UnityEngine.UI;
using SK.Framework;
using SK.Framework.UI;

namespace UI
{
    public class MainWindow : UIView
    {
        [SerializeField]
        private GameObject PlayerInfo;
        [SerializeField]
        private Image Avatar_Image;
        [SerializeField]
        private Text Username_Text;
        [SerializeField]
        private Button Chat_Button;
        [SerializeField]
        private Button CardGame_Button;
        [SerializeField]
        private Button Rank_Button;
        [SerializeField]
        private Button Exit_Button;

        protected override void OnInit(IViewData data)
        {
            Username_Text.text = Mgr.LoginMgr.Instance.Username;
        }

        protected override void BindListeners()
        {

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
    }
}