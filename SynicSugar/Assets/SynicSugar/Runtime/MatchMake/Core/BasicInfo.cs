using System.Collections.Generic;
using MemoryPack;

namespace SynicSugar.P2P {
    /// <summary>
    /// For Host to share basic info with Guests.
    /// </summary>
    [MemoryPackable]
    public partial class BasicInfo {
        public List<string> userIds = new List<string>();
        public List<byte> disconnectedUserIndexes = new List<byte>();
        public uint ElapsedSecSinceStart = 0;
        [MemoryPackConstructor]
        public BasicInfo(List<string> userids, List<byte> disconnecteduserindexes, uint elapsedSecSinceStart){
            userIds = userids;
            disconnectedUserIndexes = disconnecteduserindexes;
            ElapsedSecSinceStart = elapsedSecSinceStart;
        }
        public BasicInfo(){}
    }
}