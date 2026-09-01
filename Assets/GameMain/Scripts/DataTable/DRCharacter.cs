using GameFramework;
using System.IO;
using System.Text;
using UnityGameFramework.Runtime;

namespace ZombiesMustDie
{
    /// <summary>
    /// 角色配置表。
    /// </summary>
    public class DRCharacter : DataRowBase
    {
        private int m_Id = 0;

        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        public string Name { get; private set; }

        public int EntityId { get; private set; }

        public int DefaultWeaponId { get; private set; }

        public int MaxHP { get; private set; }

        public float MoveSpeed { get; private set; }

        public int UnlockMoney { get; private set; }

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
            Name = columnStrings[index++];
            EntityId = int.Parse(columnStrings[index++]);
            DefaultWeaponId = int.Parse(columnStrings[index++]);
            MaxHP = int.Parse(columnStrings[index++]);
            MoveSpeed = float.Parse(columnStrings[index++]);
            UnlockMoney = int.Parse(columnStrings[index++]);
            GeneratePropertyArray();
            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    Name = binaryReader.ReadString();
                    EntityId = binaryReader.Read7BitEncodedInt32();
                    DefaultWeaponId = binaryReader.Read7BitEncodedInt32();
                    MaxHP = binaryReader.Read7BitEncodedInt32();
                    MoveSpeed = binaryReader.ReadSingle();
                    UnlockMoney = binaryReader.Read7BitEncodedInt32();
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
