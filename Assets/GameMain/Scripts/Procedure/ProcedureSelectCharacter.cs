using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;
using GameFramework.Procedure;
using GameFramework.Event;
using UnityEditor;

namespace ZombiesMustDie
{
    public class ProcedureSelectCharacter : ProcedureBase
    {

        private bool m_BackToMenu = false;
        private bool m_ChooseLevel = false;//关卡选择
        private MenuCameraTurnDirection m_TurnDirection = MenuCameraTurnDirection.Right;
        private SelectCharacterForm m_SelectCharacterForm = null;

        public void BackToMenu(MenuCameraTurnDirection turnDirection)
        {
            if (m_BackToMenu) //防止重复调用
            {
                return;
            }
            m_BackToMenu = true;

            m_TurnDirection = turnDirection;
        }

        public void ChooseLevel()
        {
            if (m_ChooseLevel) //防止重复调用
            {
                return;
            }
            m_ChooseLevel = true;
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("进入角色选择流程");
            GameEntry.Event.Subscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);

            // m_StartGame = false;
            //显示菜单UI
            GameEntry.UI.OpenUIForm(UIFormId.SelectCharacterForm, this);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            //todo:进入关卡选择界面 或 返回菜单界面

            if (m_BackToMenu)
            {
                m_BackToMenu = false;

                //相机旋转方向参数，由FSM管理器记录
                procedureOwner.SetData<VarInt32>(
                    "MenuCameraTurnDirection",
                    (int)m_TurnDirection);

                ChangeState<ProcedureTurnMenuCam>(procedureOwner);
            }

            if (m_ChooseLevel)
            {
                Log.Info("进入游戏");
                //测试，当前不进入管卡选择界面，直接进入游戏Main场景
                procedureOwner.SetData<VarInt32>(
                "NextSceneId",
                GameEntry.Config.GetInt("Scene.Main", 2));
                ChangeState<ProcedureChangeScene>(procedureOwner);
            }
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            //注销事件
            GameEntry.Event.Unsubscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);

            if (m_SelectCharacterForm != null)
            {
                GameEntry.UI.CloseUIForm(m_SelectCharacterForm);
                m_SelectCharacterForm = null;
            }
        }

        private void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs ne = (OpenUIFormSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            m_SelectCharacterForm = (SelectCharacterForm)ne.UIForm.Logic;
        }


    }
}
