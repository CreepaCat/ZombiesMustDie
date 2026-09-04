
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using ZombiesMustDie;
using GameFramework.DataTable;

namespace ZombiesMustDie
{
    public class SelectCharacterForm : UGuiForm
    {

        [SerializeField]
        private TextMeshProUGUI m_MoneyText = null;

        [SerializeField]
        private TextMeshProUGUI m_CharacterNameText = null;


        private ProcedureSelectCharacter m_ProcedureSelectCharacter = null;

        //角色选择显示
        private Transform m_CharacterModelRoot = null;
        private Transform[] m_CharacterModels = null;

        private int m_TotalCharacterCount = 0;
        private int m_CurrentCharacterIndex = 0;

        //todo；当点击选择角色按钮时，切换角色模型，切换角色名称

        public void OnSelectCharacterLeftButtonClick()
        {
            Log.Info("点击了选择角色左按钮");
            m_CurrentCharacterIndex--;
            if (m_CurrentCharacterIndex < 0)
            {
                m_CurrentCharacterIndex = m_TotalCharacterCount - 1; // 循环到最后一个角色
            }
            UpdateCharacterForm();
        }

        public void OnSelectCharacterRightButtonClick()
        {
            Log.Info("点击了选择角色右按钮");

            m_CurrentCharacterIndex++;
            m_CurrentCharacterIndex %= m_TotalCharacterCount; // 循环到第一个角色
            UpdateCharacterForm();
        }

        public void OnChooseLevelButtonClick()
        {
            Log.Info("点击了选择关卡按钮");

            //!测试，直接进入游戏
            m_ProcedureSelectCharacter.ChooseLevel();
        }

        public void OnBackToMenuButtonClick()
        {
            Log.Info("点击了返回菜单按钮");
            m_ProcedureSelectCharacter.BackToMenu(MenuCameraTurnDirection.Right);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            m_ProcedureSelectCharacter = (ProcedureSelectCharacter)userData;
            if (m_ProcedureSelectCharacter == null)
            {
                Log.Warning("ProcedureSelectCharacter is invalid when open SelectCharacterForm.");
                return;
            }

            m_CharacterModelRoot = GameObject.Find("CharacterModelRoot").transform;

            m_TotalCharacterCount = m_CharacterModelRoot.childCount;
            Log.Info("角色模型根节点子Trans数量：" + m_TotalCharacterCount);
            m_CharacterModels = new Transform[m_TotalCharacterCount]; // 假设有3个角色模型
            UpdateCharacterForm();



        }

        private void UpdateCharacterForm()
        {
            // 通过角色索引获取对应的角色数据表行
            IDataTable<DRCharacter> dtCharacter =
                GameEntry.DataTable.GetDataTable<DRCharacter>();
            DRCharacter drCharacter = dtCharacter.GetDataRow(m_CurrentCharacterIndex + 1); // 假设角色ID从1开始

            if (dtCharacter == null)
            {
                Log.Error("DRCharacter data table is not loaded.");
                return;
            }
            for (int i = 0; i < m_TotalCharacterCount; i++)
            {
                m_CharacterModels[i] = m_CharacterModelRoot.GetChild(i).transform;
                m_CharacterModels[i].gameObject.SetActive(i == m_CurrentCharacterIndex); // 只激活当前角色模型

            }


            //同时更新玩家金钱和角色职业名称
            m_MoneyText.text = GameEntry.Setting.GetInt(Constant.Setting.Money, 300).ToString();
            m_CharacterNameText.text = drCharacter.GetChineseName();
            //todo:显示解锁角色所需的金钱数,已解锁角色无需显示
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
        }
    }
}
