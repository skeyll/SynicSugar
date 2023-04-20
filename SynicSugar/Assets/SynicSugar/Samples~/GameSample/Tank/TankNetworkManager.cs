using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
using UnityEngine;
namespace  SynicSugar.Samples {
    public class TankNetworkManager : MonoBehaviour{
        [SerializeField] GameObject playerPrefab;
        public TankPlayerData localPlayer;
        void Start(){
            //Generate other player to get packet
            foreach(var id in p2pManager.Instance.userIds.RemoteUserIds){
                TankPlayerData playerData = new TankPlayerData(){ OwnerUserID = id };
            }
            localPlayer = new TankPlayerData();
            localPlayer.SetOwnerID(p2pManager.Instance.userIds.LocalUserId);
            //Generate all player model
            SynicObject.AllSpawn(playerPrefab);
            //After creating the instances for receive in local, start Packet Receiver.
            ConnectHub.Instance.StartPacketReceiver();
            //Set Player Name(Sync)
            localPlayer.PlayerName = TankPassedData.PlayerName;
        }

        public void ReturnToTitle(){
            AsyncReturnToTitle().Forget();
        }
        async UniTask AsyncReturnToTitle(){
            ConnectHub.Instance.EndConnection();
            //The last player close lobby.
            if(p2pManager.Instance.userIds.IsHost() && p2pManager.Instance.userIds.RemoteUserIds.Count == 0){
                CancellationTokenSource cnsToken = new CancellationTokenSource();
                await SynicSugar.MatchMake.MatchMakeManager.Instance.DestroyHostingLobby(cnsToken);
            }
            GameModeSelect modeSelect = new GameModeSelect();
            modeSelect.ChangeGameScene((int)GameModeSelect.GameScene.MainMenu);
        }
    }
}