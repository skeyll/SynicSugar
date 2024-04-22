using System.Collections.Generic;
using MemoryPack;

namespace SynicSugar.P2P {
    /// <summary>
    /// For Host to share basic info with Guests.
    /// </summary>
    [MemoryPackable]
    public partial class BasicInfo {
        public List<string> userIds = new List<string>();
        public List<int> disconnectedUserIndexes = new List<int>();
        [MemoryPackConstructor]
        public BasicInfo(List<string> userids, List<int> disconnecteduserindexes){
            userIds = userids;
            disconnectedUserIndexes = disconnecteduserindexes;
        }
        public BasicInfo(){}
    }
}