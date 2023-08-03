using PlayEveryWare.EpicOnlineServices;
using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using MemoryPack;

namespace SynicSugar.P2P {
    /// <summary>
    /// Hold user ids in Room player.
    /// </summary>
    public class UserIds {
        internal UserId LocalUserId;
        internal List<UserId> RemoteUserIds;

        //Options
        internal UserId HostUserId;
        // For the Host to pass the user's data to the player.
        internal List<UserId> LeftUsers = new List<UserId>();
        // If true, host can manage the this local user's data in direct.
        // If not, only the local user can manipulate the local user's data.
        // For Anti-Cheat to rewrite other player data.
        internal bool isJustReconnected;
        internal UserIds(){
            LocalUserId = new UserId(EOSManager.Instance.GetProductUserId());
        }
        /// <summary>
        /// Remove user ID of leaving lobby.<br />
        /// </summary>
        /// <param name="targetId"></param>
        internal void RemoveUserId(ProductUserId targetId){
            UserId userId = new UserId(targetId);
            RemoteUserIds.Remove(userId);
        }
        /// <summary>
        /// Move UserID from RemotoUserIDs to LeftUsers not to SendPacketToALl in vain.<br />
        /// </summary>
        /// <param name="targetId"></param>
        internal void MoveTargetUserIdToLefts(ProductUserId targetId){
            UserId userId = new UserId(targetId);
            RemoteUserIds.Remove(userId);
            LeftUsers.Add(userId);
        }
        /// <summary>
        /// Move UserID to RemotoUserIDs from LeftUsers on reconnect.
        /// </summary>
        /// <param name="targetId"></param>
        /// <returns></returns>
        internal void MoveTargetUserIdToRemoteUsersFromLeft(ProductUserId targetId){
            UserId userId = new UserId(targetId);
            LeftUsers.Remove(userId);
            RemoteUserIds.Add(userId);
        }
    }
    public struct UserId {
        #nullable enable
        readonly ProductUserId? value;
        #nullable disable
        readonly string stringValue;
        public UserId(ProductUserId id){
            this.value = id;
            this.stringValue = id != null ? id.ToString() : System.String.Empty;
        }
        public UserId(UserId id){
            this.value = id.AsEpic;
            this.stringValue = id != null ? id.ToString() : System.String.Empty;
        }
        public UserId(string idString){
            this.value = ProductUserId.FromString(idString);
            this.stringValue = idString;
        }
        public readonly ProductUserId AsEpic => value;
        public static explicit operator ProductUserId(UserId id) => id.value;
        public static explicit operator UserId(ProductUserId value) => new UserId(value);
        public bool Equals(UserId other) => value.Equals(other.value);
        #nullable enable
        public override bool Equals(object? obj){
            if (obj is null || obj.GetType() != typeof(UserId)) { return false; }
            return Equals((UserId)obj);
        }
        #nullable restore
        public override int GetHashCode() => value.GetHashCode();
        public override string ToString() => stringValue;
        public static bool operator ==(in UserId x, in UserId y) => x.value.Equals(y.value);
        public static bool operator !=(in UserId x, in UserId y) => !x.value.Equals(y.value);
    }
    public class SugarPacket {
        public byte ch;
        public string UserID;
        public ArraySegment<byte> payload;
    }
    public class LargePacketInfomation {
        public byte chunk;
        public byte phase;
        public bool syncSinglePhase;
        public int currentSize;
    }
    [MemoryPackable]
    // This way is bad performance. Please let me know if you have a good idea to serialize and send data.
    public partial class SynicContainer {
        public string SynicItem0;
        public string SynicItem1;
        public string SynicItem2;
        public string SynicItem3;
        public string SynicItem4;
        public string SynicItem5;
        public string SynicItem6;
        public string SynicItem7;
        public string SynicItem8;
        public string SynicItem9;
    }
}
