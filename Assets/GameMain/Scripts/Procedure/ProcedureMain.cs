using GameFramework.Event;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;
using GameFramework.Procedure;

namespace ZombiesMustDie
{
    public class ProcedureMain : ProcedureBase
    {

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            //显示初始化关卡，显示玩家，敌人，
            Log.Info("进入游戏主场景");
        }

    }
}
