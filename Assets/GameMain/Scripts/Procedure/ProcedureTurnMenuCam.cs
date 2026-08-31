using GameFramework.Procedure;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace ZombiesMustDie
{
    /// <summary>
    /// 转动菜单相机
    /// </summary>
    public class ProcedureTurnMenuCam : ProcedureBase
    {

        private bool m_TurnComplete;
        private MenuCameraTurnDirection m_TurnDirection;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_TurnComplete = false;

            // 获取相机旋转方向参数
            int directionValue =
                procedureOwner.GetData<VarInt32>("MenuCameraTurnDirection");

            m_TurnDirection =
                (MenuCameraTurnDirection)directionValue;

            MenuCameraController cameraController =
                GameObject.FindAnyObjectByType<MenuCameraController>();

            if (cameraController == null)
            {
                Log.Error("MenuCameraController is not found.");
                return;
            }

            cameraController.Turn(m_TurnDirection, OnTurnComplete);
        }

        protected override void OnUpdate(
            ProcedureOwner procedureOwner,
            float elapseSeconds,
            float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_TurnComplete)
            {
                return;
            }

            // 根据实际流程替换,进入角色选择界面 还是返回菜单界面
            if (m_TurnDirection == MenuCameraTurnDirection.Right)
            {
                ChangeState<ProcedureStartMenu>(procedureOwner);
            }
            else if (m_TurnDirection == MenuCameraTurnDirection.Left)
            {
                ChangeState<ProcedureSelectCharacter>(procedureOwner);
            }
        }

        private void OnTurnComplete()
        {
            m_TurnComplete = true;
        }

    }
}
