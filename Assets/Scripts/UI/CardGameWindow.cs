using UnityEngine;
using UnityEngine.UI;
using SK.Framework;
using SK.Framework.UI;
using Mgr;
using Multiplayer;
using proto.Match;
using proto.CardGame;
using System.Collections.Generic;
using System.Collections;

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
        private Text PlayerScore_Text;
        [SerializeField]
        private Text SelfUsername_Text;
        [SerializeField]
        private Text SelfHp_Text;
        [SerializeField]
        private Text SelfScore_Text;
        [SerializeField]
        private Text SelfEnergy_Text;
        [SerializeField]
        private Text PlayerCard_Text;
        [SerializeField]
        private GameObject PlayerCardMask_Object;
        [SerializeField]
        private Text SelfCard_Text;
        [SerializeField]
        private GameObject SelfCardMask_Object;
        [SerializeField]
        private Button[] HandCard_Buttons;
        [SerializeField]
        private Text[] HandCard_Texts;
        [SerializeField]
        private Button Confirm_Button;
        [SerializeField]
        private GameObject WaitingPanel_Object;
        [SerializeField]
        private Button CancelMatch_Button;

        private List<string> cardNames = new List<string>()
        {
            "攻击1",
            "攻击3",
            "防御3",
            "反弹1",
            "回能1"
        };
        public List<int> CardCost = new List<int> { 1, 2, 1, 1, -1 };
        private int energy = 2;
        private CardGameMgr.CardId selectedCardId = (CardGameMgr.CardId)(-1);
        private bool isConfirmed = false;
        protected override void OnInit(IViewData data)
        {
            WaitingPanel_Object.SetActive(true);
            CardGameMgr.Instance.SendMatch(true);
            for (int i = 0; i < HandCard_Texts.Length; i++)
            {
                HandCard_Texts[i].text = cardNames[i];
            }
            PlayerCard_Text.text = string.Empty;
            PlayerCardMask_Object.SetActive(false);
            SelfCard_Text.text = string.Empty;
            SelfCardMask_Object.SetActive(false);
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
            BindHandCardButtonListeners(HandCard_Buttons);
            Confirm_Button.onClick.AddListener(OnConfirmButtonClicked);
        }

        private void BindHandCardButtonListeners(Button[] buttons)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                int index = i;
                buttons[index].onClick.AddListener(() => OnHandCardButtonClicked(index));
            }
        }

        private void ClearCardStatus()
        {
            PlayerCard_Text.text = string.Empty;
            PlayerCardMask_Object.SetActive(false);
            SelfCard_Text.text = string.Empty;
            SelfCardMask_Object.SetActive(false);
            selectedCardId = (CardGameMgr.CardId)(-1);
            isConfirmed = false;
        }

        private void UpdateCardStatus(CardGameRes res)
        {
            energy = res.selfEnergy;
            SelfEnergy_Text.text = energy.ToString();
            if (res.isTurnOver)
            {
                PlayerCard_Text.text = cardNames[res.playerCardId];
                PlayerCardMask_Object.SetActive(false);
                SelfCardMask_Object.SetActive(false);
                PlayerHp_Text.text = res.playerHp.ToString() + "/7";
                SelfHp_Text.text = res.selfHp.ToString() + "/7";
            }
        }

        private IEnumerator TurnOver(CardGameRes res)
        {
            //延迟后继续
            yield return new WaitForSeconds(2f);
            ClearCardStatus();
            if (res.isGameOver)
            {
                string tipsText = string.Empty;
                if (-1 == res.isWin)
                {
                    tipsText = "你输了";
                }
                else if (0 == res.isWin)
                {
                    tipsText = "平局";
                }
                else if (1 == res.isWin)
                {
                    tipsText = "你赢了";
                }
                Main.UI.LoadView("TipsWindow", UIConst.TipsWindow, ViewLevel.TIPS, out _, new TipsData
                {
                    tipsText = tipsText + ",你的分数是" + res.selfScore.ToString(),
                    isShowConfirm = true,
                    isShowCancel = false,
                    onConfirm = () =>
                    {
                        Hide();
                    }
                }, true);
            }
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


        private void OnHandCardButtonClicked(int index)
        {
            if (isConfirmed)
                return;
            selectedCardId = (CardGameMgr.CardId)index;
            SelfCard_Text.text = cardNames[index];
        }

        private void OnConfirmButtonClicked()
        {
            if (selectedCardId == (CardGameMgr.CardId)(-1) || isConfirmed)
                return;
            if (energy < CardCost[(int)selectedCardId])
            {
                Main.UI.LoadView("TipsWindow", UIConst.TipsWindow, ViewLevel.TIPS, out _, new TipsData
                {
                    tipsText = "能量不足",
                    isShowConfirm = true,
                    isShowCancel = false,
                }, true);
                return;
            }
            SelfCardMask_Object.SetActive(true);
            CardGameMgr.Instance.SendCardGame(selectedCardId);
            isConfirmed = true;
        }

        private void OnMatchRes(MatchRes res)
        {
            WaitingPanel_Object.SetActive(false);
            if (res.isBeginGame)
            {
                PlayerUsername_Text.text = res.PlayerUsername;
                PlayerHp_Text.text = res.selfHp.ToString() + "/7";
                SelfScore_Text.text = res.Score.ToString();
                SelfUsername_Text.text = LoginMgr.Instance.Username;
                SelfHp_Text.text = res.selfHp.ToString() + "/7";
                energy = res.selfEnergy;
                SelfEnergy_Text.text = energy.ToString();
            }
            else
            {
                Hide();
            }
        }

        private void OnCardGameRes(CardGameRes res)
        {
            UpdateCardStatus(res);
            if (res.isTurnOver)
            {
                StartCoroutine(TurnOver(res));
            }
        }
    }
}