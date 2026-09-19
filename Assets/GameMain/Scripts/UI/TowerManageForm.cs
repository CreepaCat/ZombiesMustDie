using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ZombiesMustDie
{
    public sealed class TowerManageFormData
    {
        public TowerManageFormData(TowerService service, TowerBuildPoint point) { Service = service; Point = point; }
        public TowerService Service { get; }
        public TowerBuildPoint Point { get; }
    }

    /// <summary>GF UI 管理界面；按钮仅提交服务操作，不直接修改金币或实体。</summary>
    public sealed class TowerManageForm : UGuiForm
    {
        [SerializeField] private TMP_Dropdown buildOptions;
        [SerializeField] private TMP_Text details;
        [SerializeField] private TMP_Text balance;
        [SerializeField] private TMP_Text message;
        [SerializeField] private UnityEngine.UI.Button buildButton;
        [SerializeField] private UnityEngine.UI.Button upgradeButton;
        [SerializeField] private UnityEngine.UI.Button demolishButton;
        [SerializeField] private UnityEngine.UI.Button closeButton;
        private TowerManageFormData context;
        private readonly List<int> towerIds = new List<int>();

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (buildButton != null) buildButton.onClick.AddListener(Build);
            if (upgradeButton != null) upgradeButton.onClick.AddListener(Upgrade);
            if (demolishButton != null) demolishButton.onClick.AddListener(Demolish);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (buildOptions != null) buildOptions.onValueChanged.AddListener(_ => Refresh());
        }

        protected override void OnOpen(object userData) { base.OnOpen(userData); Bind(userData as TowerManageFormData); }
        public void Bind(TowerManageFormData data)
        {
            Unbind();
            context = data;
            towerIds.Clear();
            if (!Valid()) { Close(true); return; }
            context.Service.Changed += Refresh;
            context.Service.Wallet.BalanceChanged += OnBalanceChanged;
            var labels = new List<string>();
            var rows = GameEntry.DataTable.GetDataTable<DRTower>()?.GetAllDataRows();
            if (rows != null) foreach (var row in rows)
            {
                if (!context.Point.Allows(row.Id)) continue;
                towerIds.Add(row.Id);
                var level = GameEntry.DataTable.GetDataTable<DRTowerLevel>()?.GetDataRow(row.InitialLevelId);
                labels.Add($"{row.Name} ({level?.Cost ?? 0} 金币)");
            }
            if (buildOptions != null) { buildOptions.ClearOptions(); buildOptions.AddOptions(labels); }
            if (message != null) message.text = string.Empty;
            Refresh();
        }

        private bool Valid() => context != null && context.Service != null && context.Service.isActiveAndEnabled &&
            context.Service.Wallet != null && context.Point != null && context.Point.isActiveAndEnabled;
        private void OnBalanceChanged(long _) => Refresh();
        private void Refresh()
        {
            if (!Valid()) return;
            var point = context.Point;
            var data = point.Tower != null ? point.Tower.Data : null;
            var table = GameEntry.DataTable.GetDataTable<DRTowerLevel>();
            var next = data != null ? table?.GetDataRow(data.Level.NextLevelId) : null;
            int selected = buildOptions != null ? buildOptions.value : 0;
            var selectedTower = selected >= 0 && selected < towerIds.Count ? GameEntry.DataTable.GetDataTable<DRTower>()?.GetDataRow(towerIds[selected]) : null;
            var initial = selectedTower != null ? table?.GetDataRow(selectedTower.InitialLevelId) : null;
            long coins = context.Service.Wallet.Balance;
            if (balance != null) balance.text = $"金币：{coins}";
            if (buildButton != null) buildButton.interactable = !point.IsBusy && data == null && initial != null && coins >= initial.Cost;
            if (upgradeButton != null) upgradeButton.interactable = !point.IsBusy && next != null && coins >= next.Cost;
            if (demolishButton != null) demolishButton.interactable = !point.IsBusy && data != null;
            if (buildOptions != null) buildOptions.interactable = !point.IsBusy && data == null;
            var shownLevel = data?.Level ?? initial;
            var weapon = shownLevel != null ? GameEntry.DataTable.GetDataTable<DRWeapon>()?.GetDataRow(shownLevel.WeaponId) : null;
            if (details != null) details.text = shownLevel == null ? "请选择防御塔" :
                $"等级：{shownLevel.Level}\n攻击：{weapon?.Attack ?? 0}  间隔：{weapon?.FireInterval ?? 0}s\n" +
                $"射程：{shownLevel.Range}  转速：{shownLevel.TurnSpeed}\n范围半径：{weapon?.AreaRadius ?? 0}\n" +
                (data == null ? $"建造：{shownLevel.Cost} 金币" :
                    $"{(next == null ? "已满级" : $"升级：{next.Cost} 金币")}\n拆除退款：{data.TotalPaid}") +
                (point.IsBusy ? "\n正在处理…" : string.Empty);
            if (message != null && !string.IsNullOrEmpty(context.Service.LastError)) message.text = context.Service.LastError;
        }

        public void Build()
        {
            if (!Valid()) return;
            int selected = buildOptions != null ? buildOptions.value : 0;
            if (selected >= 0 && selected < towerIds.Count) ShowResult(context.Service.TryBuild(context.Point, towerIds[selected]));
        }
        public void Upgrade() { if (Valid()) ShowResult(context.Service.TryUpgrade(context.Point)); }
        public void Demolish() { if (Valid()) ShowResult(context.Service.TryDemolish(context.Point)); }
        private void ShowResult(bool accepted) { if (message != null) message.text = accepted ? string.Empty : context.Service.LastError; Refresh(); }
        protected override void OnUpdate(float elapsed, float realElapsed)
        {
            base.OnUpdate(elapsed, realElapsed);
            if (context != null && !Valid()) Close(true);
        }
        protected override void OnClose(bool shutdown, object userData) { Unbind(); base.OnClose(shutdown, userData); }
        private void Unbind()
        {
            if (context?.Service != null)
            {
                context.Service.Changed -= Refresh;
                if (context.Service.Wallet != null) context.Service.Wallet.BalanceChanged -= OnBalanceChanged;
            }
            context = null;
        }
    }
}
