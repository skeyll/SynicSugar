using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

namespace SynicSugar.P2P {
    public class p2pInfo : MonoBehaviour {
#region Singleton
        public static p2pInfo Instance { get; private set; }
        void Awake() {
            if( Instance != null ) {
                Destroy( this );
                return;
            }
            Instance = this;
            userIds = new ();
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
        ConnectionManager connectionManager;
        NatRelayManager natRelayManager;
        internal UserIds userIds;
        internal p2pPing pings;
        internal RPCInformation lastRpcInfo;
        internal TargetRPCInformation lastTargetRPCInfo;
        /// <summary>
        /// Whether matching has been completed and the game has started.
        /// </summary>
        internal bool IsConnecting;
        /// <summary>
        /// Set reference of some manager classes.
        /// </summary>
        /// <param name="connectionInstance"></param>
        /// <param name="natrelayInstance"></param>
        internal void SetDependency(ConnectionManager connectionInstance, NatRelayManager natrelayInstance){
            connectionManager = connectionInstance;
            natRelayManager = natrelayInstance;
        }
    #region UserId basic info
        /// <summary>
        /// This lobby's Host UserId.
        /// </summary>
        public UserId HostUserId => userIds.HostUserId;
        /// <summary>
        /// This local UserId.
        /// </summary>
        public UserId LocalUserId => userIds.LocalUserId;
        /// <summary>
        /// Remote UserIds currently connected.
        /// </summary>
        public List<UserId> CurrentRemoteUserIds => userIds.RemoteUserIds;
        /// <summary>
        /// All Connected UserIds　（Excluding disconnected and left users)
        /// </summary>
        public List<UserId> CurrentConnectedUserIds => userIds.CurrentConnectedUserIds;
        /// <summary>
        /// AllUserIds - Left Users.
        /// </summary>
        public List<UserId> CurrentAllUserIds => userIds.CurrentAllUserIds;
        /// <summary>
        /// All UserIds throughout this session include Local and Left Users.<br />
        /// This value is the same value and same order with all locals through the whole game.
        /// </summary>
        public List<UserId> AllUserIds => userIds.AllUserIds;


        /// <summary>
        /// Disconnected user ids. (May come back)
        /// </summary>
        public List<UserId> DisconnectedUserIds => userIds.DisconnectedUserIds;

        /// <summary>
        /// Get LocalUser' UserIndex in AllUserIds<br />
        /// This is the same　value in all locals.
        /// </summary>
        /// <returns>(0, max lobby members count -1)</returns>
        public int GetUserIndex(){
            return userIds.AllUserIds.IndexOf(userIds.LocalUserId);
        }
        /// <summary>
        /// Get UserIndex in AllUserIds<br />
        /// This is the same　value in all locals.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>(0, max lobby members count -1)</returns>
        public int GetUserIndex(UserId id){
            return userIds.AllUserIds.IndexOf(id);
        }
    #endregion


        /// <summary>
        /// The notify events for connection and disconection on current session.
        /// </summary>
        public ConnectionNotifier ConnectionNotifier = new ConnectionNotifier();
        /// <summary>
        /// Reason of the user disconnected from p2p.
        /// </summary>
        public Reason LastDisconnectedUsersReason { get { return ConnectionNotifier.ClosedReason;} }
        /// <summary>
        /// UserId of the user disconnected from p2p.
        /// </summary>
        public UserId LastDisconnectedUsersId { get { return ConnectionNotifier.CloseUserId;} } 
        /// <summary>
        /// UserId of the reconnecter disconnected from p2p.
        /// </summary>
        public UserId LastConnectedUsersId { get { return ConnectionNotifier.ConnectUserId;} } 
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
        public byte SyncedSynicPhase { get { return SyncSnyicNotifier.LastSyncedPhase; } } 
        /// <summary>
        /// UserId of the last SyncSynic to receive to this local.
        /// </summary>
        public UserId LastSyncedUserId { get { return SyncSnyicNotifier.LastSyncedUserId;} } 
        /// <summary>
        /// Always return false. Just on reconnect, returns true until getting SyncSynic for SELF data from Host.
        /// </summary>
        public bool IsReconnecter => userIds.isJustReconnected;
        
        /// <summary>
        /// Update local user's NATType to the latest state.
        /// </summary>
        public async UniTask QueryNATType() => await natRelayManager.QueryNATType();
        /// <summary>
        /// Get last-queried NAT-type, if it has been successfully queried.
        /// </summary>
        /// <returns>Open means being able connect with direct p2p. Otherwise, the connection may be via Epic relay.</returns>
        public NATType GetNATType() => natRelayManager.GetNATType();
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
            return UserId.GetUserId(targetId) == userIds.HostUserId;
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
            return UserId.GetUserId(targetId) == userIds.LocalUserId;
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
        /// Manually update Ping data with Target to latest.
        /// </summary>
        /// <returns></returns>
        public async UniTask RefreshPing(UserId target){
            await pings.RefreshPing(target, connectionManager.rttTokenSource.Token);
        }
        /// <summary>
        /// Manually update Pings data to latest.
        /// </summary>
        /// <returns></returns>
        public async UniTask RefreshPings(){
            await pings.RefreshPings(connectionManager.rttTokenSource.Token);
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
        public bool LastRPCIsLargePacket => lastRpcInfo.isLargePacket;
        public bool LastTargetRPCIsLargePacket => lastTargetRPCInfo.isLargePacket;
        /// <summary>
        /// Checks if the connection has been enabled by the library or user.
        /// This does not necessarily mean that an actual connection has been established.
        /// The IsConnected flag becomes true after the user or library initiates the connection.
        /// </summary>
        /// <returns>True if the connection is open, false otherwise.</returns>
        public bool ConnectionIsValid(){
            return connectionManager.IsConnected;
        }
        /// <summary>
        /// Gets the currently valid (active) packet receiver type.
        /// </summary>
        /// <returns>The active ReceiverType enum value</returns>
        public ReceiverType GetActiveReceiverType(){
            return connectionManager.validReceiverType;
        }
    }
}