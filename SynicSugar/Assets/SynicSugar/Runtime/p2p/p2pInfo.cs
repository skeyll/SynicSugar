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
                Instance = null;
            }
        }
#endregion
        public UserId LocalUserId => p2pConfig.Instance.userIds.LocalUserId;
        public List<UserId> RemoteUserIds => p2pConfig.Instance.userIds.RemoteUserIds;
        public bool AcceptHostSynic => p2pConfig.Instance.userIds._AcceptHostSynic();
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
    }
}