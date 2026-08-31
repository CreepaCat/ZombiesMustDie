
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using ZombiesMustDie;

public class SelectCharacterForm : UGuiForm
{

    [SerializeField]
    private TextMeshProUGUI m_MoneyText = null;

    [SerializeField]
    private TextMeshProUGUI m_CharacterNameText = null;

    private ProcedureSelectCharacter m_ProcedureSelectCharacter = null;

    //todo；当点击选择角色按钮时，切换角色模型，切换角色名称

    public void OnSelectCharacterLeftButtonClick()
    {
        Log.Info("点击了选择角色左按钮");
    }

    public void OnSelectCharacterRightButtonClick()
    {
        Log.Info("点击了选择角色右按钮");
    }

    public void OnChooseLevelButtonClick()
    {
        Log.Info("点击了选择关卡按钮");
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

        //初始化金钱和角色职业名称
        m_MoneyText.text = "988";
        m_CharacterNameText.text = "工程师";

    }

    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }
}
