using System;
using UnityEngine;
namespace ZombiesMustDie
{
    /// <summary>
    ///目标检测器
    /// </summary>
    public class EnemyTargetDetector : MonoBehaviour
    {

        Player _player;

        void Awake()
        {
            _player = Player.GetInstance();
        }

        public float GetDistanceToPlayer()
        {
            _player ??= Player.GetInstance();
            if (_player == null)
            {
                Debug.Log("EnemyTargetDetector找不到玩家目标");
                return 0f;
            }
            return Vector3.Distance(_player.transform.position, transform.position);
        }

        internal Vector3 GetPlayerPosition()
        {
            _player ??= Player.GetInstance();
            if (_player == null)
            {
                Debug.Log("EnemyTargetDetector找不到玩家目标");
                return transform.position;
            }

            return _player.transform.position;
        }
    }
}
