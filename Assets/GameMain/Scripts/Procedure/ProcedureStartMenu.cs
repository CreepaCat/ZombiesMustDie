using GameFramework.Event;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;
using GameFramework.Procedure;

namespace ZombiesMustDie
{
    public class ProcedureStartMenu : ProcedureBase
    {
        private bool m_StartGame = false;
        private MenuCameraTurnDirection m_TurnDirection = MenuCameraTurnDirection.Left;
        private StartMenuForm m_MenuForm = null;

        public void StartGame(MenuCameraTurnDirection turnDirection)
        {
            if (m_StartGame) //防止重复调用
            {
                return;
            }
            m_StartGame = true;
            m_TurnDirection = turnDirection;
        }
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("进入开始菜单流程");
            GameEntry.Event.Subscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);

            m_StartGame = false;
            //显示菜单UI
            GameEntry.UI.OpenUIForm(UIFormId.MenuForm, this);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            //todo:如果点击了开始，则旋转镜头，进行人物选择
            if (m_StartGame)
            {
                m_StartGame = false;

                //相机旋转方向参数，由FSM管理器记录
                procedureOwner.SetData<VarInt32>(
                    "MenuCameraTurnDirection",
                    (int)m_TurnDirection);

                ChangeState<ProcedureTurnMenuCam>(procedureOwner);
            }


        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            //注销事件
            GameEntry.Event.Unsubscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);

            //关闭菜单UI
            if (m_MenuForm != null)
            {
                GameEntry.UI.CloseUIForm(m_MenuForm);
                m_MenuForm = null;
            }
        }


        private void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs ne = (OpenUIFormSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            m_MenuForm = (StartMenuForm)ne.UIForm.Logic;
        }
    }
}
