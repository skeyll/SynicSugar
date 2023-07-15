using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SynicSugar.MatchMake {
    public class LobbyIDMethod {
        public event Action Save, Delete;

        /// <summary>
        /// Register finctions to save and delete lobby Id to re-connect.<br />
        /// We can use cloud and save assets for this, but these place to be saved and deleted must be in the same. 
        /// </summary>
        /// <param name="save">void function</param>
        /// <param name="delete">void function</param>
        /// <param name="changeType">If true, change SaveType in this method</param>
        public void Register(Action save, Action delete, bool changeType = true){
            if(changeType){
                MatchMakeManager.Instance.lobbyIdSaveType = MatchMakeManager.RecconectLobbyIdSaveType.CustomMethod;
            }
            Save += save;
            Delete += delete;
        }
        internal void Clear(){
            Save = null;
            Delete = null;
        }
        internal void OnSave(){
            Save?.Invoke();
        }
        internal void OnDelete(){
            Delete?.Invoke();
        }
        
    }
    public class AsyncLobbyIDMethod {
        public event Func<UniTask> Save, Delete;
        /// <summary>
        /// Register functions that returns UniTask to save and delete lobby Id to re-connect.<br />
        /// We can use cloud and save assets for this, but these place to be saved and deleted must be in the same. 
        /// </summary>
        /// <param name="save">UniTask function</param>
        /// <param name="delete">UniTask function</param>
        /// <param name="changeType">If true, change SaveType in this method</param>
        public void Register(Func<UniTask> save, Func<UniTask> delete, bool changeType = true){
            if(changeType){
                MatchMakeManager.Instance.lobbyIdSaveType = MatchMakeManager.RecconectLobbyIdSaveType.AsyncCustomMethod;
            }
            Save += save;
            Delete += delete;
        }
        internal void Clear(){
            Save = null;
            Delete = null;
        }
        internal async UniTask OnSave(){
            if(Save == null){
                return;
            }
            await Save().AsAsyncUnitUniTask();
        }
        internal async UniTask  OnDelete(){
            if(Delete == null){
                return;
            }
            await Delete().AsAsyncUnitUniTask();
        }
        
    }
}