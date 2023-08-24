using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
namespace SynicSugar.P2P {
    public class p2pInfo : MonoBehaviour {
#region Singleton
        private p2pInfo(){}
        public static p2pInfo Instance { get; private set; }
        void Awake() {
            if( Instance != null ) {
                Destroy( this );
                return;
            }
            Instance = this;
            userIds = new ();
            infoMethod = new();
            pings = new();
            lastRpcInfo = new();
            lastTargetRPCInfo = new();
        }
        void OnDestroy() {
            if( Instance == this ) {
                UserId.CacheClear();
                ConnectionNotifier.Clear();
                SyncSnyicNotifier.Clear();

                Instance = null;
            }
        }
#endregion
        internal p2pInfoMethod infoMethod;
        internal UserIds userIds;
        internal p2pPing pings;
        internal RPCInformation lastRpcInfo;
        internal TargetRPCInformation lastTargetRPCInfo;
        /// <summary>
        /// UserId of this local user.
        /// </summary>
        public UserId LocalUserId => userIds.LocalUserId;
        /// <summary>
        /// UserIds of current session.
        /// </summary>
        public List<UserId> RemoteUserIds => userIds.RemoteUserIds;
        /// <summary>
        /// The notify events for connection and disconection on current session.
        /// </summary>
        public ConnectionNotifier ConnectionNotifier = new ConnectionNotifier();
        /// <summary>
        /// Reason of the user disconnected from p2p.
        /// </summary>
        public Reason LastDisconnectedUsersReason => ConnectionNotifier.ClosedReason;
        /// <summary>
        /// UserId of the user disconnected from p2p.
        /// </summary>
        public UserId LastDisconnectedUsersId => ConnectionNotifier.CloseUserId;
        /// <summary>
        /// UserId of the reconnecter disconnected from p2p.
        /// </summary>
        public UserId LastConnectedUsersId => ConnectionNotifier.ConnectUserId;
        /// <summary>
        /// The notify events for SyncSynic for recconecter and large packet.
        /// </summary>
        public SyncSnyicNotifier SyncSnyicNotifier = new SyncSnyicNotifier();
        /// <summary>
        /// Return True only once when this local user is received SyncSync from every other peers of the current session. <br />
        /// After return true, all variable for this flag is initialized and returns False again.
        /// </summary>
        /// <returns></returns>
        public bool HasReceivedAllSyncSynic => SyncSnyicNotifier.ReceivedAllSyncSynic();
        /// <summary>
        /// Phase of the last SyncSynic to receive to this local.
        /// </summary>
        public byte SyncedSynicPhase => SyncSnyicNotifier.LastSyncedPhase;
        /// <summary>
        /// UserId of the last SyncSynic to receive to this local.
        /// </summary>
        public UserId LastSyncedUserId => SyncSnyicNotifier.LastSyncedUserId;
        /// <summary>
        /// Always return false. Just on reconnect, returns true until getting SyncSynic for self data from Host.
        /// </summary>
        public bool AcceptHostSynic => userIds.isJustReconnected;
        
        /// <summary>
        /// Get member count in just current match.
        /// </summary>
        /// <param name="targetId"></param>
        /// <returns></returns>
        public int GetCurrentConnectionMemberCount(){
            return 1 + userIds.RemoteUserIds.Count; 
        }
        /// <summary>
        /// Get all member count that is current and past participation member count instead of just current.
        /// </summary>
        /// <param name="targetId"></param>
        /// <returns></returns>
        public int GetAllConnectionMemberCount(){
            return 1 + userIds.RemoteUserIds.Count + userIds.LeftUsers.Count; 
        }
        /// <summary>
        /// Update local user's NATType to the latest state.
        /// </summary>
        public async UniTask QueryNATType() => await infoMethod.QueryNATType();
        /// <summary>
        /// Get last-queried NAT-type, if it has been successfully queried.
        /// </summary>
        /// <returns>Open means being able connect with direct p2p. Otherwise, the connection may be via Epic relay.</returns>
        public NATType GetNATType() => infoMethod.GetNATType();
    #region IsHost
        /// <summary>
        /// Is this local user Game Host?
        /// </summary>
        /// <returns></returns>
        public bool IsHost (){
            return userIds.LocalUserId == userIds.HostUserId;
        }
        /// <summary>
        /// Is this user Game Host?
        /// </summary>
        /// <returns></returns>
        public bool IsHost (UserId targetId){
            return targetId == userIds.HostUserId;
        }
        /// <summary>
        /// Is this user Game Host?
        /// </summary>
        /// <returns></returns>
        public bool IsHost (string targetId){
            return targetId == userIds.HostUserId.ToString();
        }
    #endregion
    #region IsLocalUser
        /// <summary>
        /// Is this user local user?
        /// </summary>
        /// <returns></returns>
        public bool IsLoaclUser (UserId targetId){
            return targetId == userIds.LocalUserId;
        }
        /// <summary>
        /// Is this user local user?
        /// </summary>
        /// <returns></returns>
        public bool IsLoaclUser (string targetId){
            return targetId == userIds.LocalUserId.ToString();
        }
    #endregion
    #region Ping
        /// <summary>
        /// Get last Ping of specific user from local data.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetPing(UserId id){
            return pings.pingInfo[id.ToString()].Ping;
        }
        /// <summary>
        /// Manually update Ping data to latest with RPC.
        /// </summary>
        /// <returns></returns>
        public async UniTask RefreshPing(){
            await pings.RefreshPings(p2pConnectorForOtherAssembly.Instance.p2pToken.Token);
        }
    #endregion
        /// <summary>
        /// the last byte array sent with RPC that record data.
        /// </summary>
        public byte[] LastRPCPayload => lastRpcInfo.payload;
        /// <summary>
        /// the last byte array sent with TargetRPC that record data.
        /// </summary>
        public byte[] LastTargetRPCPayload => lastTargetRPCInfo.payload;
        /// <summary>
        /// the last ch sent with RPC that record data.
        /// </summary>
        public byte LastRPCch => lastRpcInfo.ch;
        /// <summary>
        /// the last ch sent with TargetRPC that record data.
        /// </summary>
        public byte LastTargetRPCch => lastTargetRPCInfo.ch;
        /// <summary>
        /// the last UserId sent with TargetRPC that record data.
        /// </summary>
        public UserId LastTargetRPCUserId => lastTargetRPCInfo.target;
    }
}