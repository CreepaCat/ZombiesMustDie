using UnityEngine;

namespace ZombiesMustDie
{
    /// <summary>
    /// 总部基地，玩家保护对象
    /// </summary>
    public class HeadQuarter : MonoBehaviour
    {
        public static Vector3 GetPosition()
        {
            return GameObject.FindGameObjectWithTag("HeadQuarter").transform.position;
        }
    }
}
