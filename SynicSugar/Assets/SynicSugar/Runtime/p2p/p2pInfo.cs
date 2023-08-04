using Epic.OnlineServices.P2P;
using UnityEngine;
using System;
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
        }
        void OnDestroy() {
            if( Instance == this ) {
                ConnectionNotifier.Clear();
                SyncSnyicNotifier.Clear();

                Instance = null;
            }
        }
#endregion
        [HideInInspector] internal UserIds userIds = new UserIds();
        public UserId LocalUserId => userIds.LocalUserId;
        public List<UserId> RemoteUserIds => userIds.RemoteUserIds;

        public ConnectionNotifier ConnectionNotifier = new ConnectionNotifier();
        public Reason LastDisconnectedUsersReason => ConnectionNotifier.ClosedReason;
        public UserId LastDisconnectedUsersId => ConnectionNotifier.CloseUserId;
        public UserId LastConnectedUsersId => ConnectionNotifier.ConnectUserId;

        public SyncSnyicNotifier SyncSnyicNotifier = new SyncSnyicNotifier();
        /// <summary>
        /// Return True only once when this local user is received SyncSync from every other peers of the current session. </ br>
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
    }
}