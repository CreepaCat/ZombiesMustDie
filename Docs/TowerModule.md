# 防御塔核心逻辑

当前仅保留建造点、塔实体与武器加载、升级、自动攻击和关卡清理。
金币模块、防御塔管理 UI、旧金币/UI 验证脚本以及主动拆除功能已移除。

## 调用入口

- `TowerService.TryBuild(point, towerId)`：在空建造点创建指定类型的塔，无需钱包或费用。
- `TowerService.TryUpgrade(point)`：应用下一等级，无需费用。
- `TowerService.ShutdownLevel()`：关卡退出时取消未完成请求、回收塔与武器。
- 返回 true 表示已受理异步加载请求；最终结果读取点位 State、Tower 和服务 LastError。
- Changed 事件通知状态变化，不依赖 UI。监听器应避免抛出异常。
- 没有主动拆除接口；点位禁用及关卡退出仍执行生命周期清理。

`towerId` 是 Tower.txt 的 Id（当前为 1、2），不是 Entity.txt 的实体资源编号（50001、50002）。
场景中的 TestBuildTower 保留，默认创建塔类型 1，按 B 可再次提交。

## 数据与场景

1. 关卡管理对象挂 TowerService，无需 PlayerWallet。
2. EntityComponent 配置 Tower、Weapon、Bullet 三个 Entity Group。
3. 固定位置挂 TowerBuildPoint，设置 spawnPoint 和 allowedTowerIds。
4. Tower.txt 指向初始 TowerLevel；TowerLevel 保留等级、实体、武器、射程、转速与下一等级编号。
   Cost 列及对应文本/二进制读取已移除；如使用旧二进制资源，必须按新结构重新生成。
5. 塔与武器仍通过 ShowEntity/HideEntity 创建和回收。
   TowerCombat 设置炮台与视线起点，TowerTargetDetector 设置敌人层。
6. 单体塔复用 ProjectileWeaponEntity/BulletEntity；范围塔使用 AreaWeaponEntity。
   塔武器无限弹药，射速由武器冷却控制。升级保留冷却截止时间，已发射子弹保持原伤害。
7. ProcedureChangeScene 先停止所有塔服务，再回收全关卡实体及子弹。

## 失败与回收

点位在建造/升级期间锁定，重复请求被拒绝。塔或武器加载失败、初始化失败及超时都会回收候选实体。
建造失败恢复空点位；升级失败恢复原塔。操作编号与回调数据身份防止过期回调重新占用点位。
同步异常通过 finally 恢复点位和操作锁，异步失败通过 GF 事件处理。
不使用 try-catch 或 do-while。

场景 Main 中原有的钱包组件随模块删除一并清理，避免留下 Missing Script。
