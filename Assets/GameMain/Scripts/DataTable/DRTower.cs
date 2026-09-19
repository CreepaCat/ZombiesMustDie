using System.IO;
using System.Text;
using GameFramework;
using UnityGameFramework.Runtime;

namespace ZombiesMustDie
{
    public sealed class DRTower : DataRowBase
    {
        private int id;
        public override int Id => id;
        public string Name { get; private set; }
        public int InitialLevelId { get; private set; }
        public override bool ParseDataRow(string dataRowString, object userData)
        {
            var c = dataRowString.Split(DataTableExtension.DataSplitSeparators);
            for (int i = 0; i < c.Length; i++) c[i] = c[i].Trim(DataTableExtension.DataTrimSeparators);
            id = int.Parse(c[1]);
            Name = c[3];
            InitialLevelId = int.Parse(c[4]);
            return true;
        }
        public override bool ParseDataRow(byte[] bytes, int startIndex, int length, object userData)
        {
            using (var stream = new MemoryStream(bytes, startIndex, length, false))
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                id = reader.Read7BitEncodedInt32();
                Name = reader.ReadString();
                InitialLevelId = reader.Read7BitEncodedInt32();
            }
            return true;
        }
    }
}
