using UnityEngine;
using UnityEngine.UI;
using SK.Framework;
using SK.Framework.UI;
using Mgr;
using Multiplayer;
using proto.Message;
using System.Collections.Generic;

namespace UI
{
    public class ChatWindow : UIView
    {
        [SerializeField]
        private Button Exit_Button;
        [SerializeField]
        private Button WorldChat_Button;
        [SerializeField]
        private GameObject WorldChatIsOn_Object;
        [SerializeField]
        private Button FriendChat_Button;
        [SerializeField]
        private GameObject FriendChatIsOn_Object;
        [SerializeField]
        private GameObject FriendList_ScrollView;
        [SerializeField]
        public ToggleGroup FriendList_ToggleGroup;
        [SerializeField]
        private GameObject ChatMain_Object;
        [SerializeField]
        private GameObject Chat_ScrollView;
        [SerializeField]
        private ScrollRect Chat_ScrollView_ScrollRect;
        [SerializeField]
        private InputField Input_InputField;
        [SerializeField]
        private Button Send_Button;
        [SerializeField]
        private GameObject FriendOperation_Object;
        [SerializeField]
        private Button Accept_Button;
        [SerializeField]
        private Button Reject_Button;

        private enum SelectedTab
        {
            WorldChat,
            FriendChat
        }
        private SelectedTab selectedTab = (SelectedTab)(-1);

        private PlayerInfoData playerInfoData;

        protected override void OnInit(IViewData data)
        {
            OnWorldChatButtonClicked();
        }

        protected override void OnShow(IViewData data)
        {
            OnWorldChatButtonClicked();
        }

        protected override void BindEvents()
        {
            Main.Events.Subscribe(ProtoEventID.FriendInfoRes, UpdateFriendListScrollView);
            Main.Events.Subscribe<PlayerInfoData, bool>(FriendMgr.Instance.PlayerInfoButton, UpdateChatMain);
            Main.Events.Subscribe<MessageRes>(ProtoEventID.MessageRes, UpdateChatScrollView);
        }

        protected override void BindListeners()
        {
            Exit_Button.onClick.AddListener(() => Hide());
            WorldChat_Button.onClick.AddListener(() => OnWorldChatButtonClicked());
            FriendChat_Button.onClick.AddListener(() => OnFriendChatButtonClicked());
            Accept_Button.onClick.AddListener(() => OnAcceptButtonClicked());
            Reject_Button.onClick.AddListener(() => OnRejectButtonClicked());
            Send_Button.onClick.AddListener(() => OnSendButtonClicked());
        }

        //清空FriendList_ScrollView
        private void ClearFriendListScrollView()
        {
            Transform content = FriendList_ScrollView.transform.Find("Viewport/Content");

            // 逆序删除，避免索引问题
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }

        private void UpdateFriendListScrollView()
        {
            if (selectedTab == SelectedTab.WorldChat)
                return;
            ClearFriendListScrollView();
            GameObject playerInfoWidgetPrefab = Resources.Load<GameObject>(UIConst.PlayerInfoWidget);
            foreach (var friend in FriendMgr.Instance.FriendList)
            {
                IUIView playerInfoWidget;
                var instance = Instantiate(playerInfoWidgetPrefab, FriendList_ScrollView.transform.Find("Viewport/Content"));
                instance.name = "PlayerInfoWidget";

                playerInfoWidget = instance.GetComponent<IUIView>();
                playerInfoWidget.Name = "PlayerInfoWidget";
                playerInfoWidget.Init(new PlayerInfoData
                {
                    id = friend.id,
                    username = friend.username,
                    isFriend = friend.isFriend
                }, true);
            }
        }

        private void UpdateChatMain(PlayerInfoData playerInfoData, bool isPressed)
        {
            this.playerInfoData = playerInfoData;
            FriendOperation_Object.SetActive(isPressed && !playerInfoData.isFriend);
            if (selectedTab == SelectedTab.WorldChat)
                ChatMain_Object.SetActive(true);
            else
            {
                ChatMain_Object.SetActive(isPressed && playerInfoData.isFriend);
            }
        }

        //清空Chat_ScrollView
        private void ClearChatScrollView()
        {
            Transform content = Chat_ScrollView.transform.Find("Viewport/Content");

            // 逆序删除，避免索引问题
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }

        private void UpdateChatScrollView(MessageRes res)
        {
            if (res.FriendId != playerInfoData.id)
                return;

            var messages = res.messages;
            GameObject chatMessageWidgetPrefab = Resources.Load<GameObject>(UIConst.ChatMessageWidget);

            ClearChatScrollView();

            foreach (var message in messages)
            {
                IUIView chatMessageWidget;
                var instance = Instantiate(chatMessageWidgetPrefab, Chat_ScrollView.transform.Find("Viewport/Content"));
                instance.name = "ChatMessageWidget";

                chatMessageWidget = instance.GetComponent<IUIView>();
                chatMessageWidget.Name = "ChatMessageWidget";
                chatMessageWidget.Init(new ChatMessageData
                {
                    text = message.text,
                    playerInfoData = new PlayerInfoData
                    {
                        id = message.playerInfo.id,
                        username = message.playerInfo.username,
                        isFriend = FriendMgr.Instance.GetIsFriend(message.playerInfo.id)
                    }
                }, true);
            }
            Canvas.ForceUpdateCanvases();
            //滚动到底部
            Chat_ScrollView_ScrollRect.verticalNormalizedPosition = 0;
        }

        private void OnWorldChatButtonClicked()
        {
            if (selectedTab == SelectedTab.WorldChat)
                return;
            selectedTab = SelectedTab.WorldChat;
            ChatMain_Object.SetActive(true);
            WorldChatIsOn_Object.SetActive(true);
            FriendChatIsOn_Object.SetActive(false);
            FriendList_ScrollView.SetActive(false);
            FriendOperation_Object.SetActive(false);
            playerInfoData = new PlayerInfoData
            {
                id = 0,
                username = string.Empty,
                isFriend = false
            };
            FriendMgr.Instance.SendFriendInfo();
            FriendMgr.Instance.SendMessage(string.Empty, 0);
        }

        private void OnFriendChatButtonClicked()
        {
            if (selectedTab == SelectedTab.FriendChat)
                return;
            selectedTab = SelectedTab.FriendChat;
            WorldChatIsOn_Object.SetActive(false);
            ChatMain_Object.SetActive(false);
            FriendChatIsOn_Object.SetActive(true);
            FriendList_ScrollView.SetActive(true);
            FriendOperation_Object.SetActive(false);
            FriendMgr.Instance.SendFriendInfo();
            ClearChatScrollView();
        }

        private void OnAcceptButtonClicked()
        {
            FriendMgr.Instance.SendFriendOperation(playerInfoData.id, true);
            FriendMgr.Instance.SendMessage(string.Empty, playerInfoData.id);
        }

        private void OnRejectButtonClicked()
        {
            FriendMgr.Instance.SendFriendOperation(playerInfoData.id, false);
            FriendList_ToggleGroup.SetAllTogglesOff();
        }

        private void OnSendButtonClicked()
        {
            if (string.IsNullOrEmpty(Input_InputField.text))
                return;
            FriendMgr.Instance.SendMessage(Input_InputField.text, SelectedTab.FriendChat == selectedTab ? playerInfoData.id : 0);
            Input_InputField.text = string.Empty;
        }
    }
}