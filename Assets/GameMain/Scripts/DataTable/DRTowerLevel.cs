using System.Globalization;
using System.IO;
using System.Text;
using GameFramework;
using UnityGameFramework.Runtime;

namespace ZombiesMustDie
{
    public sealed class DRTowerLevel : DataRowBase
    {
        private int id;
        public override int Id => id;
        public int Level { get; private set; }
        public int EntityId { get; private set; }
        public int WeaponId { get; private set; }
        public float Range { get; private set; }
        public float TurnSpeed { get; private set; }
        public int Cost { get; private set; }
        public int NextLevelId { get; private set; }
        public override bool ParseDataRow(string dataRowString, object userData)
        {
            var c = dataRowString.Split(DataTableExtension.DataSplitSeparators);
            for (int i = 0; i < c.Length; i++) c[i] = c[i].Trim(DataTableExtension.DataTrimSeparators);
            id = int.Parse(c[1]);
            Level = int.Parse(c[3]);
            EntityId = int.Parse(c[4]);
            WeaponId = int.Parse(c[5]);
            Range = float.Parse(c[6], CultureInfo.InvariantCulture);
            TurnSpeed = float.Parse(c[7], CultureInfo.InvariantCulture);
            Cost = int.Parse(c[8]);
            NextLevelId = int.Parse(c[9]);
            return true;
        }
        public override bool ParseDataRow(byte[] bytes, int startIndex, int length, object userData)
        {
            using (var stream = new MemoryStream(bytes, startIndex, length, false))
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                id = reader.Read7BitEncodedInt32();
                Level = reader.Read7BitEncodedInt32();
                EntityId = reader.Read7BitEncodedInt32();
                WeaponId = reader.Read7BitEncodedInt32();
                Range = reader.ReadSingle();
                TurnSpeed = reader.ReadSingle();
                Cost = reader.Read7BitEncodedInt32();
                NextLevelId = reader.Read7BitEncodedInt32();
            }
            return true;
        }
    }
}
