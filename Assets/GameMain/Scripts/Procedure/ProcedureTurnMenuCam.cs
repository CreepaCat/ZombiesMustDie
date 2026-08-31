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

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_TurnComplete = false;

            // 获取相机旋转方向参数
            int directionValue =
                procedureOwner.GetData<VarInt32>("MenuCameraTurnDirection");

            MenuCameraTurnDirection direction =
                (MenuCameraTurnDirection)directionValue;

            MenuCameraController cameraController =
                GameObject.FindAnyObjectByType<MenuCameraController>();

            if (cameraController == null)
            {
                Log.Error("MenuCameraController is not found.");
                return;
            }

            cameraController.Turn(direction, OnTurnComplete);
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

            // 根据实际流程替换,角色选择界面
            ChangeState<ProcedureSelectCharacter>(procedureOwner);
        }

        private void OnTurnComplete()
        {
            m_TurnComplete = true;
        }

    }
}
