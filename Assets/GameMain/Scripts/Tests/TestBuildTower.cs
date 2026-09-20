using UnityEngine;
using UnityEngine.InputSystem;
using ZombiesMustDie;

/// <summary>使用塔类型编号验证核心建造入口。</summary>
public class TestBuildTower : MonoBehaviour
{
    // [SerializeField] private TowerBuildPoint buildPoint;
    private TowerBuildPoint buildPoint;
    [SerializeField] private TowerService towerService;
    [SerializeField] private int towerId = 1;

    private void Start() => Build(buildPoint);

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame) Build(buildPoint);
    }

    public void Build(TowerBuildPoint buildPoint)
    {
        if (towerService == null || buildPoint == null) return;
        if (!towerService.TryBuild(buildPoint, towerId))
            Debug.LogWarning(towerService.LastError, this);
    }
}
