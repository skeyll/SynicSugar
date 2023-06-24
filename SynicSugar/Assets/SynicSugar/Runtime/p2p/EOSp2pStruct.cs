using PlayEveryWare.EpicOnlineServices;
using System;
using System.Collections.Generic;
using Epic.OnlineServices;

namespace SynicSugar.P2P {
    /// <summary>
    /// Hold user ids in Room player.
    /// </summary>
    public class UserIds {
        public UserId LocalUserId;
        public List<UserId> RemoteUserIds;

        //Options
        internal UserId HostUserId;
        // For the Host to pass the user's data to the player.
        public List<UserId> LeftUsers = new List<UserId>();
        // If true, host can manage the this local user's data in direct.
        // If not, only the local user can manipulate the local user's data.
        // For Anti-Cheat to rewrite other player data.
        internal bool isJustReconnected;
        public UserIds(){
            LocalUserId = new UserId(EOSManager.Instance.GetProductUserId());
        }
        /// <summary>
        /// Is this local user Game Host?
        /// </summary>
        /// <returns></returns>
        public bool IsHost (){
            return LocalUserId == HostUserId;
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
    }
    public struct UserId {
        readonly ProductUserId value;
        public UserId(ProductUserId id){
            this.value = id;
        }
        public UserId(UserId id){
            this.value = id.AsEpic;
        }
        public UserId(string idString){
            this.value = ProductUserId.FromString(idString);;
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
        public override string ToString() => value.ToString();
        public static bool operator ==(in UserId x, in UserId y) => x.value.Equals(y.value);
        public static bool operator !=(in UserId x, in UserId y) => !x.value.Equals(y.value);
    }
    public class SugarPacket {
        public byte ch;
        public string UserID;
        public ArraySegment<byte> payload;
    }
}
