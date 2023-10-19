using PlayEveryWare.EpicOnlineServices;
using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using MemoryPack;

namespace SynicSugar.P2P {
    /// <summary>
    /// Hold user ids in Room player.
    /// </summary>
    internal class UserIds {
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
            LocalUserId = UserId.GetUserId(EOSManager.Instance.GetProductUserId());
        }
        /// <summary>
        /// Remove user ID of leaving lobby.<br />
        /// </summary>
        /// <param name="targetId"></param>
        internal void RemoveUserId(ProductUserId targetId){
            UserId userId = UserId.GetUserId(targetId);
            RemoteUserIds.Remove(userId);
            p2pInfo.Instance.pings.pingInfo.Remove(userId.ToString());
        }
        /// <summary>
        /// Move UserID from RemotoUserIDs to LeftUsers not to SendPacketToALl in vain.<br />
        /// </summary>
        /// <param name="targetId"></param>
        internal void MoveTargetUserIdToLefts(ProductUserId targetId){
            UserId userId = UserId.GetUserId(targetId);
            RemoteUserIds.Remove(userId);
            LeftUsers.Add(userId);
            p2pInfo.Instance.pings.pingInfo[userId.ToString()].Ping = -1;
        }
        /// <summary>
        /// Move UserID to RemotoUserIDs from LeftUsers on reconnect.
        /// </summary>
        /// <param name="targetId"></param>
        /// <returns></returns>
        internal void MoveTargetUserIdToRemoteUsersFromLeft(ProductUserId targetId){
            UserId userId = UserId.GetUserId(targetId);
            LeftUsers.Remove(userId);
            RemoteUserIds.Add(userId);
        }
    }
    //MEMO: If we pass this with null in an argument, this will not be null. So, when we use AsEpic to that instance, this returns error.
    public class UserId {
    #region Cache
        static internal Dictionary<string, UserId> idCache = new();
        static internal void CacheClear(){
            idCache.Clear();
        }
    #endregion

        readonly ProductUserId value;
        readonly string value_s;
        private UserId(ProductUserId id){
            if(id is null){
                return;
            }
            this.value = id;
            this.value_s = id.ToString();
        }

        /// <summary>
        /// We can only create a new UserID Instance from Epic's product UserID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns> <summary>
        static public UserId GetUserId(ProductUserId id){
            string s = id.ToString();
            if(idCache.ContainsKey(s)){
                return idCache[s];
            }
            UserId obj = new UserId(id);
            idCache.Add(s, obj);
            return obj;
        }
        static public UserId GetUserId(UserId id){
            string s = id.ToString();
            if(idCache.ContainsKey(s)){
                return idCache[s];
            }
            return null;
        }
        static public UserId GetUserId(string id){
            if(idCache.ContainsKey(id)){
                return idCache[id];
            }
            return null;
        }
        static private UserId ToUserId(ProductUserId id){
            string key = id.ToString();
            if(idCache.ContainsKey(key)){
                return idCache[key];
            }
            return null;
        }
        public ProductUserId AsEpic => value;

        public static explicit operator ProductUserId(UserId id) => GetUserId(id).value;
        public static explicit operator UserId(ProductUserId value) => ToUserId(value);
        #nullable enable
        public bool Equals(UserId? other) => value is not null && other is not null && value.Equals(other.value);
        public override bool Equals(object? obj){
            return value is not null && obj is not null && obj.GetType() == typeof(UserId) && Equals((UserId)obj);
        }
        public override int GetHashCode() => value.GetHashCode();
        public override string ToString() => value_s;
        public static bool operator ==(in UserId? x, in UserId? y){
            if (x is null && y is null){ return true; }
            if (x is null || y is null) { return false; }
            return x.value.Equals(y.value);
        } 
        public static bool operator !=(in UserId? x, in UserId? y){
            if (x is null && y is null) { return false; }
            if (x is null || y is null) { return true; }
            return !x.value.Equals(y.value);
        }
        #nullable disable
    }
    /// <summary>
    /// To reconstruction large packet. 
    /// </summary>
    public class LargePacketsInfomation {
        /// <summary>
        /// How many packets are sent in a packet?
        /// </summary>
        public byte chunk;
        /// <summary>
        /// Current packet size received
        /// </summary>
        public int currentSize;
    }
    /// <summary>
    /// Just for Synic.
    /// </summary>
    public class SynicPacketInfomation {
        public LargePacketsInfomation basis = new();
        /// <summary>
        /// Phase specified in SyncSynic
        /// </summary>
        public byte phase;
        /// <summary>
        /// For just a one phase?
        /// </summary>
        public bool syncSinglePhase;
    }

#region OBSOLETE
    public class LargePacketInfomation {
        /// <summary>
        /// How many packets are sent in a packet?
        /// </summary>
        public byte chunk;
        /// <summary>
        /// Phase specified in SyncSynic
        /// </summary>
        public byte phase;
        /// <summary>
        /// For just a one phase?
        /// </summary>
        public bool syncSinglePhase;
        /// <summary>
        /// Current packet size received
        /// </summary>
        public int currentSize;
    }
#endregion
#region For p2pInfo
    public class PingInformation {
        internal int Ping;
        internal DateTime LastUpdatedLocalUTC;
        internal List<double> tmpPings = new List<double>();
    }
    public class RPCInformation {
        internal byte[] payload;
        internal byte ch;
    }
    public class TargetRPCInformation {
        internal byte[] payload;
        internal byte ch;
        internal UserId target;
    }
#endregion
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
