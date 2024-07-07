#pragma warning disable CS0414 //The field is assigned but its value is never used
using System;
using System.Collections.Generic;

namespace SynicSugar.P2P {
    public class SyncSnyicNotifier {
        /// <summary>
        /// Invoke when Synic variables is synced.
        /// </summary>
        public event Action OnSyncedSynic;
        
        public void Register(Action syncedSynic){
            OnSyncedSynic += syncedSynic;
        }
        internal void Clear(){
            OnSyncedSynic = null;
        }

        internal UserId LastSyncedUserId { get; private set; }
        internal byte LastSyncedPhase { get; private set; }
        bool _receivedAllSyncSynic;
        List<string> ReceivedUsers = new List<string>();
        bool isForAll;
        internal bool ReceivedAllSyncSynic(){
            if(_receivedAllSyncSynic){
                //Init
                ReceivedUsers.Clear();
                _receivedAllSyncSynic = false;
                isForAll = false;

                return true;
            }
            return false;
        }
        //Access this from public method in p2pAssembleXXX.　We can move this to that for calling cast.
        internal void UpdateSyncedState(string id, byte phase){
            if (!ReceivedUsers.Contains(id)){
                ReceivedUsers.Add(id);
                LastSyncedUserId = UserId.GetUserId(id);
                LastSyncedPhase = phase;
                
                if(!isForAll && !p2pInfo.Instance.CurrentConnectedUserIds.Contains(UserId.GetUserId(id))){
                    isForAll = true;
                }
            }

            OnSyncedSynic?.Invoke();

            if(ReceivedUsers.Count == (isForAll ? p2pInfo.Instance.AllUserIds.Count : p2pInfo.Instance.CurrentConnectedUserIds.Count)){
                _receivedAllSyncSynic = true;
            }
        }
    }
}