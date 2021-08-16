using System;
using JetBrains.Annotations;

namespace CSUID
{
    public class Id
    {
        public DateTimeOffset Timestamp { get; }
        public uint Sequence { get; }
        public uint ProcessId { get; }
        public byte[] Mac { get; } = new byte[6];
        public string System { get; }
        public string Environment { get; }


        public Id([NotNull] string id)
        {
            var idSegments = id.Split('_');
            System = idSegments[0];
            Environment = idSegments[1];

            var idKey = Base62.FromBase62(idSegments[2]);

            Buffer.BlockCopy(idKey, 0, Mac, 0, 6);

            ProcessId = BitConverter.ToUInt32(idKey, 6);
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(IdGenerator.Epoch + BitConverter.ToUInt32(idKey, 10));
            Sequence = BitConverter.ToUInt16(idKey, 14);
        }
    }
}
