using UnityEngine;
using UnityEngine.UI;
using SK.Framework;
using SK.Framework.UI;

namespace UI
{
    public class ChatWindow : UIView
    {
        [SerializeField]
        private Button Exit_Button;

        protected override void OnInit(IViewData data)
        {
        }

        protected override void BindListeners()
        {
            Exit_Button.onClick.AddListener(() => Unload());
        }
    }
}