using UnityEngine;
using UnityEngine.UI;
using SK.Framework;
using SK.Framework.UI;
using Mgr;
using Multiplayer;
using proto.Match;
using proto.CardGame;

namespace UI
{
    public class CardGameWindow : UIView
    {
        [SerializeField]
        private Button Exit_Button;
        [SerializeField]
        private Text PlayerUsername_Text;
        [SerializeField]
        private Text PlayerHp_Text;
        [SerializeField]
        private Text SelfUsername_Text;
        [SerializeField]
        private Text SelfHp_Text;
        [SerializeField]
        private Text SelfScore_Text;

        [SerializeField]
        private Button Confirm_Button;
        [SerializeField]
        private GameObject WaitingPanel_Object;
        [SerializeField]
        private Button CancelMatch_Button;

        protected override void OnInit(IViewData data)
        {
            WaitingPanel_Object.SetActive(true);
            CardGameMgr.Instance.SendMatch(true);
        }

        protected override void OnShow(IViewData data)
        {
            WaitingPanel_Object.SetActive(true);
            CardGameMgr.Instance.SendMatch(true);
        }

        protected override void BindEvents()
        {
            Main.Events.Subscribe<MatchRes>(ProtoEventID.MatchRes, OnMatchRes);
            Main.Events.Subscribe<CardGameRes>(ProtoEventID.CardGameRes, OnCardGameRes);
        }

        protected override void BindListeners()
        {
            CancelMatch_Button.onClick.AddListener(OnCancelMatchButton);
            Exit_Button.onClick.AddListener(OnExitButton);
        }

        private void OnCancelMatchButton()
        {
            CardGameMgr.Instance.SendMatch(false);
        }

        private void OnExitButton()
        {
            Main.UI.LoadView("TipsWindow", UIConst.TipsWindow, ViewLevel.TIPS, out _, new TipsData
            {
                tipsText = "确定要退出吗,将视为自动放弃",
                isShowConfirm = true,
                isShowCancel = true,
                onConfirm = () =>
                {
                    CardGameMgr.Instance.SendMatch(false);
                }
            }, true);
        }

        private void OnMatchRes(MatchRes res)
        {
            WaitingPanel_Object.SetActive(false);
            if (res.isBeginGame)
            {
                PlayerUsername_Text.text = res.PlayerUsername;
                PlayerHp_Text.text = "7/7";
                SelfScore_Text.text = res.Score.ToString();
                SelfUsername_Text.text = LoginMgr.Instance.Username;
                SelfHp_Text.text = "7/7";
            }
            else
            {
                Hide();
            }
        }

        private void OnCardGameRes(CardGameRes res)
        {
            if (res.isGameOver)
            {
                Main.UI.LoadView("TipsWindow", UIConst.TipsWindow, ViewLevel.TIPS, out _, new TipsData
                {
                    tipsText = (res.isWin ? "你赢了" : "你输了") + ",你的分数是" + res.selfScore.ToString(),
                    isShowConfirm = true,
                    isShowCancel = false,
                    onConfirm = () =>
                    {
                        Hide();
                    }
                }, true);
            }
        }
    }
}