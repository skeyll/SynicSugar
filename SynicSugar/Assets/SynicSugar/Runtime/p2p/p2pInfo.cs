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
        public UserId LocalUserId => userIds.LocalUserId;
        public List<UserId> RemoteUserIds => userIds.RemoteUserIds;

        public ConnectionNotifier ConnectionNotifier = new ConnectionNotifier();
        public Reason LastDisconnectedUsersReason => ConnectionNotifier.ClosedReason;
        public UserId LastDisconnectedUsersId => ConnectionNotifier.CloseUserId;
        public UserId LastConnectedUsersId => ConnectionNotifier.ConnectUserId;

        public SyncSnyicNotifier SyncSnyicNotifier = new SyncSnyicNotifier();
        /// <summary>
        /// Return True only once when this local user is received SyncSync from every other peers of the current session. <br />
        /// After return true, all variable for this flag is initialized and returns False again.
        /// </summary>
        /// <returns></returns>
        public bool HasReceivedAllSyncSynic => SyncSnyicNotifier.ReceivedAllSyncSynic();
        public byte SyncedSynicPhase => SyncSnyicNotifier.LastSyncedPhase;
        public UserId LastSyncedUserId => SyncSnyicNotifier.LastSyncedUserId;

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
    #region 
        public int GetPing(UserId id){
            return pings.pingInfo[id.ToString()].Ping;
        }
        public async UniTask RefreshPing(){
            await pings.RefreshPings(p2pConnectorForOtherAssembly.Instance.p2pToken.Token);
        }
    #endregion

    }
}