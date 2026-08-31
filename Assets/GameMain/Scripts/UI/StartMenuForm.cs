
using UnityEngine;
using UnityGameFramework.Runtime;

namespace ZombiesMustDie
{
    public class StartMenuForm : UGuiForm
    {
        [SerializeField]
        private GameObject m_QuitButton = null;

        private ProcedureStartMenu m_ProcedureMenu = null;

        public void OnStartButtonClick()
        {
            //旋转相机
            m_ProcedureMenu.StartGame(MenuCameraTurnDirection.Left);
        }

        public void OnSettingButtonClick()
        {
            Log.Info("打开设置界面");
            GameEntry.UI.OpenUIForm(UIFormId.SettingForm);
        }

        public void OnAboutButtonClick()
        {
            GameEntry.UI.OpenUIForm(UIFormId.AboutForm);
        }

        public void OnQuitButtonClick()
        {
            // GameEntry.UI.OpenDialog(new DialogParams()
            // {
            //     Mode = 2,
            //     Title = GameEntry.Localization.GetString("AskQuitGame.Title"),
            //     Message = GameEntry.Localization.GetString("AskQuitGame.Message"),
            //     OnClickConfirm = delegate (object userData) { UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Quit); },
            // });
            //退出游戏
            UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Quit);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnOpen(object userData)
#else
        protected internal override void OnOpen(object userData)
#endif
        {
            base.OnOpen(userData);

            m_ProcedureMenu = (ProcedureStartMenu)userData;
            if (m_ProcedureMenu == null)
            {
                Log.Warning("ProcedureStartMenu is invalid when open MenuForm.");
                return;
            }

            m_QuitButton.SetActive(Application.platform != RuntimePlatform.IPhonePlayer);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnClose(bool isShutdown, object userData)
#else
        protected internal override void OnClose(bool isShutdown, object userData)
#endif
        {
            m_ProcedureMenu = null;

            base.OnClose(isShutdown, userData);
        }
    }


}
