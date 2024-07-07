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
        /// <summary>
        /// Just current
        /// </summary>
        internal List<UserId> RemoteUserIds = new();
        /// <summary>
        /// All users throughout this session include Local and Leave Users.
        /// </summary>
        internal List<UserId> AllUserIds = new();
        /// <summary>
        /// AllUserIds - LeftUsers.(Not tmp Disconnected)
        /// </summary>
        internal List<UserId> CurrentAllUserIds = new();
        /// <summary>
        /// Current Session include Local user, but exclude Disconencted userｓ
        /// </summary>
        internal List<UserId> CurrentConnectedUserIds = new();

        //Options
        internal UserId HostUserId;
        // For the Host to pass the user's data to the player.
        internal List<UserId> DisconnectedUserIds = new();
        // If true, host can manage the this local user's data in direct.
        // If not, only the local user can manipulate the local user's data.
        // For Anti-Cheat to rewrite other player data.
        internal bool isJustReconnected { get; private set; }
        internal UserIds(){
            LocalUserId = UserId.GetUserId(EOSManager.Instance.GetProductUserId());
        }
        internal UserIds(bool isReconencter){
            LocalUserId = UserId.GetUserId(EOSManager.Instance.GetProductUserId());
            if(isReconencter){
                isJustReconnected = true;
            }
        }
        /// <summary>
        /// Make reconencter flag false.
        /// </summary>
        internal void ReceivedocalUserSynic(){
            isJustReconnected = false;
        }
        /// <summary>
        /// Update AllUserIds with Host's sending data.
        /// </summary>
        /// <param name="data">Contains All UserIds and Disconnected user indexes</param>
        internal void OverwriteAllUserIdsWithOrdered(BasicInfo data){
            AllUserIds.Clear();
            //Change order　to same in host local.
            foreach(var id in data.userIds){
                AllUserIds.Add(UserId.GenerateFromStringForReconnecter(id));
            }

            if(!isJustReconnected){
                return;
            }
            //Create current lefted user list
            foreach(var index in data.disconnectedUserIndexes){
                DisconnectedUserIds.Add(AllUserIds[index]);
            }
            //Complement disconnected users.
            foreach(var id in DisconnectedUserIds){
                CurrentAllUserIds.Add(id);
            }
        }
        /// <summary>
        /// Remove user ID when the user leaves lobby.<br />
        /// </summary>
        /// <param name="targetId"></param>
        internal void RemoveUserId(ProductUserId targetId){
            UserId userId = UserId.GetUserId(targetId);
            RemoteUserIds.Remove(userId);
            CurrentAllUserIds.Remove(userId);
            CurrentConnectedUserIds.Remove(userId);
            p2pInfo.Instance.pings.pingInfo.Remove(userId.ToString());
        }
        /// <summary>
        /// Move UserID from RemotoUserIDs to LeftUsers not to SendPacketToALl in vain.<br />
        /// </summary>
        /// <param name="targetId"></param>
        internal void MoveTargetUserIdToLefts(ProductUserId targetId){
            UserId userId = UserId.GetUserId(targetId);
            RemoteUserIds.Remove(userId);
            CurrentConnectedUserIds.Remove(userId);
            DisconnectedUserIds.Add(userId);
            p2pInfo.Instance.pings.pingInfo[userId.ToString()].Ping = -1;
        }
        /// <summary>
        /// Move UserID to RemotoUserIDs from LeftUsers on reconnect.
        /// </summary>
        /// <param name="targetId"></param>
        /// <returns></returns>
        internal void MoveTargetUserIdToRemoteUsersFromLeft(ProductUserId targetId){
            UserId userId = UserId.GetUserId(targetId);
            DisconnectedUserIds.Remove(userId);
            CurrentConnectedUserIds.Add(userId);
            RemoteUserIds.Add(userId);
        }
    }
}
