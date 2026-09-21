using UnityEngine;
using UnityGameFramework.Runtime;
namespace ZombiesMustDie
{
    [RequireComponent(typeof(EnemyAnimation))]
    [RequireComponent(typeof(EnemyNavigation))]
    [RequireComponent(typeof(EnemyTargetDetector))]
    [RequireComponent(typeof(CombatController))]
    [RequireComponent(typeof(Health))]
    public class EnemyEntity : EntityLogic
    {

        public EnemyEntityData Data { private set; get; }
        public EnemyAnimation Animation { private set; get; }
        public EnemyNavigation Navigation { private set; get; }
        public EnemyStateMachine StateMachine { private set; get; }
        public EnemyTargetDetector TargetDetector { private set; get; }

        public CombatController Combat { private set; get; }

        private Health m_Health;

        public bool IsDead => m_Health.IsDead;
        public bool IsInteracting => Animation.IsInteracting;

        //死亡回收入池
        public bool CanBeRecycle { private set; get; }

        float recycleTimeout = 5f;
        float recycleTimer;
        bool startRecycle = false;
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            Animation = GetComponent<EnemyAnimation>();
            Navigation = GetComponent<EnemyNavigation>();
            TargetDetector = GetComponent<EnemyTargetDetector>();
            Combat = GetComponent<CombatController>();
            m_Health = GetComponent<Health>();
        }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);
            Data = userData as EnemyEntityData;
            if (Data == null) return;
            //todo:放置到出生点
            m_Health.SetMaxHealth((int)Data.MaxHealth);
            m_Health.Died += OnDied;

            Navigation.InitAgent(Data.MoveSpeed, Data.RotationSpeed, Data.StoppingDistance);
            InitStateMachine();

            CanBeRecycle = false;
            startRecycle = false;
            recycleTimer = 0f;
            ProcesseSpawn();
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            StateMachine.LoagicUpdate();
            if (!startRecycle) return;
            recycleTimer += elapseSeconds;
            if (recycleTimer > recycleTimeout)
            {
                CanBeRecycle = true;
            }
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            var previous = Data;
            Data = null;
            m_Health.Died -= OnDied;
            base.OnHide(isShutdown, userData);
        }


        #region 自定义方法

        private void InitStateMachine()
        {
            StateMachine = new();
            Enemy_Idle idle = new(this);
            Enemy_MoveToFortress moveToFortress = new(this);
            Enemy_ChasePlayer chasePlayer = new(this);
            Enemy_Attack attack = new(this);
            Enemy_Death death = new(this);

            StateMachine.AddState(idle);
            StateMachine.AddState(moveToFortress);
            StateMachine.AddState(chasePlayer);
            StateMachine.AddState(attack);
            StateMachine.AddState(death);

            StateMachine.Init(idle.GetType());
        }

        public Vector3 GetDirectionToPlayer()
        {
            var dirToPlayer = (TargetDetector.GetPlayerPosition() - transform.position).normalized;
            dirToPlayer.y = 0f;
            return dirToPlayer;
        }

        public void OnBornOver()
        {
            Debug.Log("将目标点设为玩家总部基地");
            StateMachine.ChangeState(typeof(Enemy_MoveToFortress));
        }

        #region 战斗相关

        internal void Attack()
        {
            Animation.PlayTargetAnimation(EnemyAnimationParamConfig.Clip_Attack01, true);
        }

        internal void OnAtkHit()
        {
            Debug.Log("OnAtkHit");
            Combat.MeleeAttack();
        }

        internal void OnDied()
        {
            StateMachine.ChangeState(typeof(Enemy_Death));
        }

        //处理死亡
        internal void ProcesseDie()
        {
            Navigation.StopMoving();
            GetComponent<Collider>().enabled = false; //死亡时关闭了Collider,相应的重生时要恢复
            Animation.PlayDeath();

            //一段时间后，隐藏尸体
            startRecycle = true;

        }

        private void ProcesseSpawn()
        {
            GetComponent<Collider>().enabled = true;
        }
        #endregion
        #endregion
    }

}
