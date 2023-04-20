using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
using UnityEngine;

namespace  SynicSugar.Samples {
    [NetworkPlayer]
    public partial class TankPlayerData {
        [SyncVar] public string PlayerName;
        [SyncVar] public int PlayerHP;
        
        public TankPlayerData(){
            InitializePlayer().Forget();
        }
        async UniTask InitializePlayer(){
            Debug.Log("Init Player");
            CancellationTokenSource token = new CancellationTokenSource();
            // Wait for localuser name update in an other client.
            //Can get a specific player network instance with ConnectHub.Instance.GetUserInstance(OwnerUserID, typeInstance).
            //Wait for that user's name to arrive.
            await UniTask.WaitWhile(() => string.IsNullOrEmpty(PlayerName), cancellationToken: token.Token);
            Debug.Log("Init Player2");

            TankPlayer tankPlayer = ConnectHub.Instance.GetUserInstance<TankPlayer>(OwnerUserID);
            tankPlayer.SetNameText(PlayerName);
            tankPlayer.gameObject.SetActive(true);
        }
    }
}
