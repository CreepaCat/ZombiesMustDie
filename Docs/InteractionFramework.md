# 最小玩家交互框架

当前仅实现目标检测、交互输入、提示接口、Linework 高亮和建造选择请求，不包含 NPC 脚本或建造选择 UI。

## 接入场景

1. 在已有 Player 所在物体上添加 PlayerInteraction，使用 Player 已配置的 InputReader。Interact 已从 F 改为 E。
2. 设置 Interaction Radius 和 Interaction Mask。目标自身或子物体需要 Collider，Trigger 也支持；检测层以 Collider 所在物体的 Layer 为准。距离按交互目标根节点计算。
3. TowerBuildPoint 已继承 Interactable，无需额外挂交互脚本。只有空闲且未被 TowerService 占用的点位允许交互。
4. 在 URP Renderer 的 Linework 描边配置中指定一个专用 Rendering Layer，并把建造点的 Highlight Layer 配成相同的位。初始 Renderer 不要包含该专用位，也不要使用 Default 或灯光正在使用的位。未配置 Highlight Layer 时不会显示高亮。
5. 描边作用于目标初始子层级内的 MeshRenderer 和 SkinnedMeshRenderer，不包含粒子。建造点只有粒子时，需要增加一个底座模型作为描边对象。

## 提示和建造选择接口

- PlayerInteraction.CurrentTarget：当前目标。
- PlayerInteraction.CurrentPrompt：当前提示内容，例如“建造”；没有目标时为空。UI 可组合为“E 建造”。
- PlayerInteraction.TargetChanged：目标切换或清空时通知 UI；UI 启用时也应读取当前值。
- TowerBuildPoint.OnBuildSelectionRequested：UnityEvent<TowerBuildPoint>，可在 Inspector 或代码中绑定，参数就是当前建造点。按 E 只发出请求，不自动建塔，也不自动禁用输入。
- 未来界面确认选择后调用现有 TowerService.TryBuild(point, towerId)，使用 LastError 展示失败原因。

## 界面与生命周期

真正打开模态界面时调用 InputReader.DisablePlayerAction()，关闭时调用 EnablePlayerControl()。交互组件读取实际输入启用状态，不维护另一份交互锁。暂停、死亡、输入禁用和组件禁用都会清空目标。目标自身禁用时清除高亮，玩家下一次检测清除提示。

UI 尚未实现，因此关闭失效目标的界面及完整的移动／射击暂停效果应在 UI 接入时一并验证。本次未修改场景、预制体或 Linework Renderer 配置。

## 验证

Unity 编辑器内通过 13 项检查：最近目标、提示通知去重、保留其他渲染层、目标高亮切换、等距保持当前目标、碰撞缓冲扩容、排除禁用目标、建造事件参数、拒绝忙碌点位、超出距离、禁用清理、销毁清理及 E 键绑定。

这些检查验证框架逻辑，不代表已验证场景中的描边画面或完整 Play Mode 建造流程。
