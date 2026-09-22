using GameFramework;
using System.Globalization;
using System.IO;
using System.Text;
using UnityGameFramework.Runtime;

namespace ZombiesMustDie
{
    /// <summary>
    /// 敌人波次配置表。
    /// </summary>
    public class DREnemyWave : DataRowBase
    {
        private int m_Id = 0;
        //波次分组id
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }
        //所属波次
        public int Wave { get; private set; }
        //当前组生成角色id
        public int CharacterId { get; private set; }
        //每组生成角色数量
        public int Count { get; private set; }
        //角色生成间隔时间
        public float SpawnInterval { get; private set; }


        /// <summary>读取制表符分隔的波次配置。</summary>
        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);
            for (int i = 0; i < columnStrings.Length; i++)
            {
                columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);
            }

            int index = 0;
            index++;
            m_Id = int.Parse(columnStrings[index++]);
            Wave = int.Parse(columnStrings[index++]);
            CharacterId = int.Parse(columnStrings[index++]);
            Count = int.Parse(columnStrings[index++]);
            SpawnInterval = float.Parse(columnStrings[index++], CultureInfo.InvariantCulture);
            GeneratePropertyArray();
            return true;
        }

        /// <summary>按与文本表相同的字段顺序读取二进制配置。</summary>
        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    Wave = binaryReader.Read7BitEncodedInt32();
                    CharacterId = binaryReader.Read7BitEncodedInt32();
                    Count = binaryReader.Read7BitEncodedInt32();
                    SpawnInterval = binaryReader.ReadSingle();
                }
            }

            GeneratePropertyArray();
            return true;
        }

        private void GeneratePropertyArray()
        {

        }
    }
}
