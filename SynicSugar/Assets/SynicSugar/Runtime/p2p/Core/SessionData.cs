using System;
using MemoryPack;
namespace SynicSugar.P2P {
    [MemoryPackable]
    public partial class SessionData
    {
        public string LobbyID { get; set; }
        public DateTime SessionStartTimestamp { get; set; }
        
        [MemoryPackConstructor]
        public SessionData(string lobbyId, DateTime sessionStartTimestamp)
        {
            LobbyID = lobbyId;
            SessionStartTimestamp = sessionStartTimestamp;
        }
    }
}