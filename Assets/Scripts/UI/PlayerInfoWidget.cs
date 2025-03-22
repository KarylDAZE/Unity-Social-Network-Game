using UnityEngine;
using UnityEngine.UI;
using SK.Framework;
using SK.Framework.UI;
using Mgr;

public class PlayerInfoData : IViewData
{
    public int id;
    public string username;
    public bool isFriend;
}

namespace UI
{
    public class PlayerInfoWidget : UIView
    {
        [SerializeField]
        private Toggle PlayerInfo_Toggle;
        [SerializeField]
        private Text Username_Text;
        [SerializeField]
        private Button Delete_Button;
        [SerializeField]
        private GameObject IsPressed_Object;

        private PlayerInfoData playerInfoData;

        protected override void OnInit(IViewData data)
        {
            playerInfoData = data as PlayerInfoData;
            Username_Text.text = playerInfoData.username;
            PlayerInfo_Toggle.group = Main.UI.GetView<ChatWindow>().FriendList_ToggleGroup;
            Delete_Button.gameObject.SetActive(playerInfoData.isFriend);
        }

        protected override void BindListeners()
        {
            PlayerInfo_Toggle.onValueChanged.AddListener((isOn) =>
            {
                IsPressed_Object.SetActive(isOn);
                Main.Events.Publish(FriendMgr.Instance.PlayerInfoButton, playerInfoData, isOn);
                if (isOn && playerInfoData.isFriend)
                    FriendMgr.Instance.SendMessage(string.Empty, playerInfoData.id);
            });

            Delete_Button.onClick.AddListener(() =>
            {
                Main.UI.LoadView("TipsWindow", UIConst.TipsWindow, ViewLevel.TIPS, out _, new TipsData
                {
                    tipsText = "确定要删除该好友吗",
                    isShowConfirm = true,
                    isShowCancel = true,
                    onConfirm = () =>
                    {
                        FriendMgr.Instance.SendFriendOperation(playerInfoData.id, false);
                    }
                }, true);
            });
        }
    }
}