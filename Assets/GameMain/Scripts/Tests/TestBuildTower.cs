using UnityEngine;
using UnityEngine.InputSystem;
using ZombiesMustDie;

public class TestBuildTower : MonoBehaviour
{
    [SerializeField] TowerBuildPoint buildPoint;
    [SerializeField] TowerService towerService;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        towerService.TryBuild(buildPoint, 50001);
    }

    void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            if (!towerService.TryBuild(buildPoint, 50001))
            {
                Debug.LogError("建造塔失败");
            }
        }
    }



}
