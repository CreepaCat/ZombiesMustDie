using GameFramework;
using System.IO;
using System.Text;
using UnityGameFramework.Runtime;

namespace ZombiesMustDie
{
    /// <summary>
    /// 武器配置表。
    /// </summary>
    public class DRWeapon : DataRowBase
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
        public int SoundId { get; private set; }

        public int BulletEntityId { get; private set; }

        public int Attack { get; private set; }

        public float BulletSpeed { get; private set; }

        public float FireInterval { get; private set; }

        public int MagazineSize { get; private set; }

        public float SpreadAngle { get; private set; }

        public int PenetrationCount { get; private set; }
        public float AreaRadius { get; private set; }

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
            index++;
            EntityId = int.Parse(columnStrings[index++]);
            SoundId = int.Parse(columnStrings[index++]);
            BulletEntityId = int.Parse(columnStrings[index++]);
            Attack = int.Parse(columnStrings[index++]);
            BulletSpeed = float.Parse(columnStrings[index++]);
            FireInterval = float.Parse(columnStrings[index++]);
            MagazineSize = int.Parse(columnStrings[index++]);
            SpreadAngle = float.Parse(columnStrings[index++]);
            PenetrationCount = int.Parse(columnStrings[index++]);
            AreaRadius = index < columnStrings.Length && !string.IsNullOrEmpty(columnStrings[index])
                ? float.Parse(columnStrings[index], System.Globalization.CultureInfo.InvariantCulture) : 0f;

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
                    SoundId = binaryReader.Read7BitEncodedInt32();
                    BulletEntityId = binaryReader.Read7BitEncodedInt32();
                    Attack = binaryReader.Read7BitEncodedInt32();
                    BulletSpeed = binaryReader.ReadSingle();
                    FireInterval = binaryReader.ReadSingle();
                    MagazineSize = binaryReader.Read7BitEncodedInt32();
                    SpreadAngle = binaryReader.ReadSingle();
                    PenetrationCount = binaryReader.Read7BitEncodedInt32();
                    AreaRadius = memoryStream.Position < memoryStream.Length ? binaryReader.ReadSingle() : 0f;
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
