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
            foreach(var id in p2pInfo.Instance.RemoteUserIds){
                TankPlayerData playerData = new TankPlayerData(){ OwnerUserID = id };
            }
            localPlayer = new TankPlayerData();
            localPlayer.SetOwnerID(p2pInfo.Instance.LocalUserId);
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
            CancellationTokenSource cnsToken = new CancellationTokenSource();
            await ConnectHub.Instance.CloseSession();
            
            GameModeSelect modeSelect = new GameModeSelect();
            modeSelect.ChangeGameScene(GameModeSelect.GameScene.MainMenu.ToString());
        }
    }
}