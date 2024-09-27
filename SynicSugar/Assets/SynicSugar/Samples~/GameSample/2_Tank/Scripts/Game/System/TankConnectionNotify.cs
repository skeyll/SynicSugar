using SynicSugar.P2P;
using UnityEngine;

namespace SynicSugar.Samples.Tank {
    public class TankConnectionNotify {
        /// <summary>
        /// Register events to get notifies about connection.
        /// </summary>
        internal void RegisterConnectionNotifyEvents(){
            p2pInfo.Instance.ConnectionNotifier.OnTargetDisconnected += OnDisconnected;

            p2pInfo.Instance.ConnectionNotifier.OnTargetEarlyDisconnected += OnEarlyDisconnected;
            p2pInfo.Instance.ConnectionNotifier.OnTargetRestored += OnRestored;

            p2pInfo.Instance.ConnectionNotifier.OnTargetConnected += OnConnected;

            p2pInfo.Instance.ConnectionNotifier.OnTargetLeaved += OnLeaved;
        }
        /// <summary>
        /// Completely disconnected
        /// </summary>
        /// <param name="id"></param>
        void OnDisconnected(UserId id){
            Debug.Log($"{GetPlayerName(id)}: Disconencted");
            TankGameManager gameManager = ConnectHub.Instance.GetInstance<TankGameManager>();
            //If there are users who disconnected during Stanby, check the ready flag with the remaining players.
            switch(gameManager.CurrentGameState){
                case GameState.Standby:
                    gameManager.CheckReadyState();
                break;
                case GameState.InGame:
                    gameManager.CheckRoundState(p2pInfo.Instance.GetUserIndex(id));
                break;
            }
        }
        /// <summary>
        /// Intentionally left the room　by Close or Exit Session.
        /// </summary>
        /// <param name="id"></param>
        void OnLeaved(UserId id){
            Debug.Log($"{GetPlayerName(id)}: Leaved");
            Object.Destroy(ConnectHub.Instance.GetUserInstance<TankPlayer>(id).gameObject);
        }
        /// <summary>
        /// If the target has not sent a heartbeat for some time, this function is called.
        /// </summary>
        /// <param name="id"></param>
        void OnEarlyDisconnected(UserId id){
            Debug.Log($"{GetPlayerName(id)}: EarlyDisconnected");
            ConnectHub.Instance.GetUserInstance<TankPlayer>(id).gameObject.SetActive(false);
        }
        /// <summary>
        /// Called when after　EarlyDisconnected is invoked connection is re-established.
        /// </summary>
        /// <param name="id"></param>
        void OnRestored(UserId id){
            Debug.Log($"{GetPlayerName(id)}: Restored");
            TankPlayer player = ConnectHub.Instance.GetUserInstance<TankPlayer>(id);
            if(ConnectHub.Instance.GetInstance<TankGameManager>().CurrentGameState != GameState.InGame){
                player.ActivatePlayer();
                return;
            }

            if(player.status.CurrentHP > 0){
                player.ActivatePlayer();
            }else{
                //To switch camera to survivor.
                ConnectHub.Instance.GetInstance<TankGameManager>().CheckReadyState();
            }
        }
        /// <summary>
        /// Called when the target reconnects after Disconnected.
        /// </summary>
        /// <param name="id"></param>
        void OnConnected(UserId id){
            //In this sample, the initial position is synchronized via RespawnPos of Synic, so each player needs to update Respawn data before SyncSuynic.
            //Of course, we can update these value as player transform position when player moves, but player don't need these value except for Synic, 
            //so it is updated when it is needed.
            ConnectHub.Instance.GetUserInstance<TankPlayer>(id).UpdateRespawnTransfomData();

            if(p2pInfo.Instance.IsHost()){
                ConnectHub.Instance.GetUserInstance<TankPlayer>(id).UpdateRespawnTransfomData();

                //Update and send the latest data as it may be needed for Synic as Host if a returning user becomes Host.
                foreach(var disconenctedId in p2pInfo.Instance.DisconnectedUserIds){
                    ConnectHub.Instance.GetUserInstance<TankPlayer>(disconenctedId).UpdateRespawnTransfomData();
                }
            }

            ConnectHub.Instance.SyncSynic(id, SynicType.WithOthers);
        }
        string GetPlayerName(UserId id){
            return $"{ConnectHub.Instance.GetUserInstance<TankPlayer>(id).status.Name}({id})";
        }
    }
}