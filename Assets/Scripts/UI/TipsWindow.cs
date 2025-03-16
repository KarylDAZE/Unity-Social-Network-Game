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

        protected override void OnInit(IViewData data)
        {
            tipsData = (TipsData)data;
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
        }
    }
}