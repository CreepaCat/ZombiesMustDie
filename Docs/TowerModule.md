# 防御塔脚本接入

## 当前交付

运行时脚本、Tower/TowerLevel 的文本和二进制解析、Weapon.AreaRadius、预加载、GF UI 编号与管理界面逻辑已实现。
表内包含两种塔、每种两级的示例数值。此提交只交付脚本和配置，没有创建正式塔、塔武器或管理界面 Prefab，也没有在地图上放置建造点。

## 场景与资源

1. 在关卡场景的管理对象添加 TowerService（自动要求 PlayerWallet）。每个关卡使用一个服务和一个钱包；
   如玩家已有独立钱包，可通过 Inspector 的 wallet 字段引用它。initialCoins 默认为 500，奖励调用 TryAdd。
2. GF EntityComponent 中配置 Tower、Weapon、Bullet 三个 Entity Group，名字可在 TowerService 中修改。
3. 在固定位置放置 TowerBuildPoint，设置 spawnPoint 与 allowedTowerIds（默认允许 1、2）。
   建造点位置应在道路侧边，塔碰撞体不可堵死导航道路。
4. Entity.txt 中下列路径需要提供正式 Prefab（相对于 Assets/GameMain/Prefabs/Entities）：
   - 50001: Tower/SingleTower.prefab
   - 50002: Tower/AreaTower.prefab
   - 20301: Weapon/TowerGun.prefab
   - 20401: Weapon/TowerArea.prefab
   单体塔还复用现有 30001: Bullet/PlayerBullet.prefab。
5. 塔 Prefab 根节点挂 TowerEntity；依赖会包含 CombatController、CombatTarget、Health、
   TowerCombat 和 TowerTargetDetector。在 TowerCombat 设置炮台 turret 和视线起点 sightOrigin，
   不设置时以根节点为炮台。推荐独立炮台子节点，避免底座跟着俯仰。
   塔武器由服务调用 ShowEntity 动态添加相应武器逻辑，不在场景里预放武器实例；
   单体武器的枪口子节点默认名为 Muzzle。塔/武器/子弹都由 GF 回收。
6. 创建 Assets/GameMain/Prefabs/UI/UIForms/TowerManageForm.prefab，根节点使用 RectTransform 并挂
   TowerManageForm，沿用现有 UGuiForm 配置。绑定 TMP_Dropdown、details/balance/message 的 TMP_Text，
   以及建造、升级、拆除、关闭四个 Button。按钮监听在 OnInit 自动注册，不要重复绑定同样的方法。
   继续使用现有 Default UI Group、Canvas 和 EventSystem。
7. 从现有交互入口调用 buildPoint.OpenManageForm(towerService)，或直接调用
   TryBuild(point, towerId)、TryUpgrade(point)、TryDemolish(point)。
   false 表示未受理，可读取 LastError；true 表示请求已受理，实体异步加载最终结果通过 Changed、
   点位 State 及 LastError 获取。不要将 true 当成加载完成。

## 数据约定

- Tower.InitialLevelId 指向 TowerLevel。初始等级必须为 1；升级配置的等级必须是当前等级加 1。
- TowerLevel.Cost 是进入该级的费用，必须非负；NextLevelId=0 表示满级。
- Weapon.AreaRadius=0 使用 ProjectileWeaponEntity，塔的 PenetrationCount 必须为 0；
  AreaRadius>0 使用 AreaWeaponEntity，在目标位置执行瞬时范围伤害。
- FireInterval、塔射程和转速必须为正且有限。单体塔的 BulletSpeed 必须为正。
- 塔武器强制无限弹药，不受 MagazineSize 限制；正常玩家武器的弹药与换弹行为不变。
- 新表提供文本/二进制读取；Weapon 追加 AreaRadius，旧行缺少该列时按 0 读取。
  项目当前 ProcedurePreload 使用 TXT；如切换二进制资源，需要重新生成 bytes。
- UIForm.txt 保留原 UTF-16 编码，其余本次新表为 UTF-8。

## 生命周期和金币

每次请求分配 Guid，钱包按凭据记录实际支付，拒绝重复支付、重复退款及已退凭据重放。
Balance 为 long，支付费用为非负 int；为已支付金额保留退款容量，奖励不会挤占退款额度。

建造锁点后扣款，塔和武器都完成初始化后才工作。升级暂停旧塔，加载新等级塔及武器，成功后替换；
失败只退本次升级费并恢复旧塔。升级支持不同等级使用不同实体资源。
换武器继承旧武器的冷却截止时间，等待加载期间冷却自然流逝；子弹持有发射时的伤害快照。
缺失资源、错误配置、加载异常/超时均不能留下新的收费占用。

主动拆除回收实体后按历次支付凭据退款。OnHide 不退已完成建造/升级的费用。
ProcedureChangeScene 在 GF HideAll 之前调用 TowerService.ShutdownAll：
取消未完成请求并退回其费用，清理已建塔但不退其投资；随后全局 HideAll 同时清理子弹。
直接禁用服务或建造点也会清理塔和武器。服务是关卡级对象，ShutdownLevel 后不再接收新请求。

塔使用 Player 阵营，CombatTarget.IsTargetable=false、Invulnerable=true；
现有 EnemyTargetDetector 仍然只追踪 Player，不会追踪塔。
免伤通过统一 CombatTarget.TakeDamage 入口执行。

## 验证与复现

Unity 6000.6.0f1 编译通过。使用 Launcher 的 Play Mode 和临时 Prefab，40 项运行检查通过：
交易幂等、余额不足、建造/升级操作锁、实际 GF 加载和挂接、两阶段加载失败回滚、
升级保留冷却、范围多碰撞体去重、满级拒绝、实付退款、取消及超时后的迟到回调、
自动索敌与遮挡、无限弹药、升级期间在途子弹保持原伤害、关卡退出不退已投入金币。

测试脚本位于 AgentScripts/TowerChecks.cs，不进入运行时编译。
从已保存的 Launcher 编辑场景运行：

~~~powershell
unity command run_script --file AgentScripts/TowerChecks.cs --entry TowerChecks.Setup
unity command editor_play
unity command run_script --file AgentScripts/TowerChecks.cs --entry TowerChecks.Run --timeout_ms 25000
unity command run_script --file AgentScripts/TowerChecks.cs --entry TowerChecks.Extra --timeout_ms 25000
unity command editor_stop
unity command run_script --file AgentScripts/TowerChecks.cs --entry TowerChecks.Cleanup
~~~

Setup 使用独立 Preview Scene 生成 __TowerChecks 临时资源；Run/Extra 只在 Play Mode 修改测试用数据，
并清理自己生成的场景对象。退出 Play Mode 后 Cleanup 删除临时资源。
故意引用 MissingTower/MissingWeapon 会产生预期的 GF 加载失败日志。
测试验证了真实生命周期，但没有验证正式塔美术 Prefab、地图布点及正式管理面板的视觉/交互接线。
