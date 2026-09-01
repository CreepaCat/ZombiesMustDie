using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace ZombiesMustDie
{
    public class ProcedureLauncher : ProcedureBase
    {
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);


            // 声音配置：根据用户配置数据，设置即将使用的声音选项
            InitSoundSettings();

            //加载玩家数据
            InitPlayerData();
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            //切换状态
            ChangeState<ProcedurePreload>(procedureOwner);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
        }

        private void InitSoundSettings()
        {
            //todo:将字符串抽出 用一个静态类统一配置
            GameEntry.Sound.Mute("Music", GameEntry.Setting.GetBool("Setting.MusicMuted", false));
            GameEntry.Sound.SetVolume("Music", GameEntry.Setting.GetFloat("Setting.MusicVolume", 0.3f));
            GameEntry.Sound.Mute("Sound", GameEntry.Setting.GetBool("Setting.SoundMuted", false));
            GameEntry.Sound.SetVolume("Sound", GameEntry.Setting.GetFloat("Setting.SoundVolume", 1f));
            // GameEntry.Sound.Mute("UISound", GameEntry.Setting.GetBool("Setting.UISoundMuted", false));
            // GameEntry.Sound.SetVolume("UISound", GameEntry.Setting.GetFloat("Setting.UISoundVolume", 1f));
            Log.Info("Init sound settings complete.");
        }

        private void InitPlayerData()
        {
            //todo:可以考虑将玩家数据做成单独的GF Component，专门管理玩家数据的加载和保存
            // GameEntry.Setting.GetInt(Constant.Setting.Money, 300);

        }
    }
}
