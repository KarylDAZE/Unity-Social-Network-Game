using UnityEngine;
using UnityEngine.UI;
using SK.Framework;
using SK.Framework.UI;
using Mgr;

public class ChatMessageData : IViewData
{
    public string text;
    public PlayerInfoData playerInfoData;
}

namespace UI
{
    public class ChatMessageWidget : UIView
    {
        [SerializeField]
        private Button PlayerInfo_Button;
        [SerializeField]
        private Text Username_Text;
        [SerializeField]
        private Text Message_Text;

        private PlayerInfoData playerInfoData;

        protected override void OnInit(IViewData data)
        {
            playerInfoData = (data as ChatMessageData).playerInfoData;
            Username_Text.text = playerInfoData.username[0].ToString();
            Message_Text.text = (data as ChatMessageData).text;
        }

        protected override void BindListeners()
        {
            PlayerInfo_Button.onClick.AddListener(() =>
            {
                if (!playerInfoData.isFriend && playerInfoData.id != LoginMgr.Instance.Id)
                {
                    Main.UI.LoadView("TipsWindow", UIConst.TipsWindow, ViewLevel.TIPS, out _, new TipsData
                    {
                        tipsText = $"确定要添加{playerInfoData.username}为好友吗",
                        isShowConfirm = true,
                        isShowCancel = true,
                        onConfirm = () =>
                        {
                            FriendMgr.Instance.SendFriendOperation(playerInfoData.id, true);
                        }
                    }, true);
                }
            });
        }
    }
}