using UnityEngine;
using UnityEngine.UI;
using SK.Framework.UI;

public class TipsData : IViewData
{
    public string tipsText;
    public bool isShowConfirm = true;
    public bool isShowCancel = true;
    public OnConfirm onConfirm;
    public OnCancel onCancel;

    public delegate void OnConfirm();
    public delegate void OnCancel();
}

namespace UI
{
    public class TipsWindow : UIView
    {
        private TipsData tipsData;
        [SerializeField]
        private Text Tips_Text;
        [SerializeField]
        private Button Confirm_Button;
        [SerializeField]
        private Button Cancel_Button;
        [SerializeField]
        private Button Exit_Button;

        protected override void OnInit(IViewData data)
        {
            tipsData = (TipsData)data;
            // Tips_Text = GameObject.Find("Tips_Text").GetComponent<Text>();
            // Confirm_Button = GameObject.Find("Confirm_Button").GetComponent<Button>();
            // Cancel_Button = GameObject.Find("Cancel_Button").GetComponent<Button>();
            // Exit_Button = GameObject.Find("Exit_Button").GetComponent<Button>();
            Confirm_Button.gameObject.SetActive(tipsData.isShowConfirm);
            Tips_Text.text = tipsData.tipsText;
            Cancel_Button.gameObject.SetActive(tipsData.isShowCancel);
        }

        protected override void BindListeners()
        {
            Confirm_Button.onClick.AddListener(() =>
            {
                tipsData.onConfirm?.Invoke();
                Unload();
            });
            Cancel_Button.onClick.AddListener(() =>
            {
                tipsData.onCancel?.Invoke();
                Unload();
            });
            Exit_Button.onClick.AddListener(() =>
            {
                tipsData.onCancel?.Invoke();
                Unload();
            });
        }
    }
}