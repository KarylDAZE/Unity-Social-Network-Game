using UnityEngine;
using UnityEngine.UI;
using SK.Framework.UI;

namespace UI
{
    public class LoginWindow : UIView
    {
        private Text Account_Text;
        private Text Aassword_Text;
        private Button Login_Button;
        private Button Exit_Button;

        protected override void OnInit(IViewData data)
        {
            Account_Text = GameObject.Find("Account_Text").GetComponent<Text>();
            Aassword_Text = GameObject.Find("Password_Text").GetComponent<Text>();
            Login_Button = GameObject.Find("Login_Button").GetComponent<Button>();
            Exit_Button = GameObject.Find("Exit_Button").GetComponent<Button>();
            AddListeners();
        }

        private void AddListeners()
        {
            Login_Button.onClick.AddListener(() =>
            {
                //send proto
                Hide();
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
    }
}