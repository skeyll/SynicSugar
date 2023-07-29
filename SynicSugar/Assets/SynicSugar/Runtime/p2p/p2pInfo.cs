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

                Instance = null;
            }
        }
#endregion
        public ConnectionNotifier ConnectionNotifier = new ConnectionNotifier();
        public Reason LastDisconnectedUsersReason => ConnectionNotifier.ClosedReason;
        public UserId LastDisconnectedUsersId => ConnectionNotifier.TargetUserId;
        public UserId LocalUserId => p2pConfig.Instance.userIds.LocalUserId;
        public List<UserId> RemoteUserIds => p2pConfig.Instance.userIds.RemoteUserIds;
        public bool AcceptHostSynic => p2pConfig.Instance.userIds.isJustReconnected;
    #region IsHost
        /// <summary>
        /// Is this local user Game Host?
        /// </summary>
        /// <returns></returns>
        public bool IsHost (){
            return p2pConfig.Instance.userIds.LocalUserId == p2pConfig.Instance.userIds.HostUserId;
        }
        /// <summary>
        /// Is this user Game Host?
        /// </summary>
        /// <returns></returns>
        public bool IsHost (UserId targetId){
            return targetId == p2pConfig.Instance.userIds.HostUserId;
        }
        /// <summary>
        /// Is this user Game Host?
        /// </summary>
        /// <returns></returns>
        public bool IsHost (string targetId){
            return targetId == p2pConfig.Instance.userIds.HostUserId.ToString();
        }
    #endregion
    #region IsLocalUser
        /// <summary>
        /// Is this user local user?
        /// </summary>
        /// <returns></returns>
        public bool IsLoaclUser (UserId targetId){
            return targetId == p2pConfig.Instance.userIds.LocalUserId;
        }
        /// <summary>
        /// Is this user local user?
        /// </summary>
        /// <returns></returns>
        public bool IsLoaclUser (string targetId){
            return targetId == p2pConfig.Instance.userIds.LocalUserId.ToString();
        }
    #endregion
    }
}