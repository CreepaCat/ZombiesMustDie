using TMPro;
using UnityEngine;
namespace ZombiesMustDie
{
    /// <summary>
    /// 基地血条UI
    /// </summary>
    public class HealthBar_HQ : MonoBehaviour
    {
        [SerializeField] RectTransform bg;
        [SerializeField] RectTransform fill;
        [SerializeField] TextMeshProUGUI tmp_currHp;
        [SerializeField] TextMeshProUGUI tmp_maxHp;
        HeadQuarter headQuarter;
        private void OnEnable()
        {
            UpdateUI(50f, 100f);
            headQuarter ??= HeadQuarter.GetInstance().GetComponent<HeadQuarter>();
            if (headQuarter == null) return;

            headQuarter.HealthChanged += UpdateUI;
        }

        private void OnDisable()
        {
            if (headQuarter == null) return;
            headQuarter.HealthChanged -= UpdateUI;
        }
        public void UpdateUI(float currHp, float maxHp)
        {
            tmp_currHp.text = currHp.ToString("f0");
            tmp_maxHp.text = "/" + maxHp.ToString("f0");
            var fillPercent = currHp / maxHp;

            //按血量百分比变化fill
            fill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
      bg.rect.width * fillPercent);

        }
    }
}
