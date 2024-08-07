// using System.Threading;
// using Cysharp.Threading.Tasks;
// using SynicSugar.P2P;
// using UnityEngine;
// using UnityEngine.UI;
// namespace  SynicSugar.Samples {
//     public class TankNetworkManager : MonoBehaviour{
//         [SerializeField] GameObject playerPrefab;
//         [SerializeField] Text pingText;
//         void Start(){
//             //Generate other player data class
//             foreach(var id in p2pInfo.Instance.CurrentRemoteUserIds){
//                 new TankPlayerData(){ OwnerUserID = id };
//             }
//             //For local player data class
//             TankPlayerData localPlayer = new TankPlayerData();
//             localPlayer.SetOwnerID(p2pInfo.Instance.LocalUserId);

//             //Generate player models
//             SynicObject.AllSpawnForCurrent(playerPrefab);

//             p2pInfo.Instance.ConnectionNotifier.OnTargetDisconnected += OnDisconnected;
//             p2pInfo.Instance.ConnectionNotifier.OnTargetEarlyDisconnected += OnEarlyDisconnected;
//             p2pInfo.Instance.ConnectionNotifier.OnTargetRestored += OnRestored;
//             p2pInfo.Instance.ConnectionNotifier.OnTargetLeaved += OnLeaved;
//             //After creating the instances for receive in local, start Packet Receiver.
//             ConnectHub.Instance.StartPacketReceiver();
//             //Set Player Name and Activate PlayerObject
//             //Even if it just sends, each instance must have a UserID in this local.
//             localPlayer.SetPlayerUserName(TankPassedData.PlayerName);

//             DisplayPing(this.GetCancellationTokenOnDestroy()).Forget();
//         }

//         public async UniTaskVoid DisplayPing(CancellationToken token){
//             while(!token.IsCancellationRequested){
//                 string pings = p2pInfo.Instance.GetNATType().ToString() + System.Environment.NewLine;

//                 foreach(var id in p2pInfo.Instance.CurrentRemoteUserIds){
//                     pings += $"{ConnectHub.Instance.GetUserInstance<TankPlayerData>(id).PlayerName}: {p2pInfo.Instance.GetPing(id)}{System.Environment.NewLine}";
//                 }
                
//                 pingText.text = pings;
//                 await UniTask.Delay(3000, cancellationToken: token);
//             }
//         } 

//         void OnDisconnected(UserId id){
//             //This sample doesn't allow reconnection, so this distinction doesn't matters not to need hold the disconnected user data.
//             //However, if your project allows to recconect, Host should hold the objects in order to send data with SyncSynic.
//             if(!p2pInfo.Instance.IsHost()){
//                 Destroy(ConnectHub.Instance.GetUserInstance<TankPlayer>(id).gameObject);
//             }
//             Debug.Log(id + "Disconencted");
//         }
//         void OnLeaved(UserId id){
//             Destroy(ConnectHub.Instance.GetUserInstance<TankPlayer>(id).gameObject);
//             Debug.Log(id + "Leaved");
//         }
//         void OnEarlyDisconnected(UserId id){
//             ConnectHub.Instance.GetUserInstance<TankPlayer>(id).gameObject.SetActive(false);
//             Debug.Log(id + "EarlyDisconnected");
//         }
//         void OnRestored(UserId id){
//             ConnectHub.Instance.GetUserInstance<TankPlayer>(id).gameObject.SetActive(true);
//             Debug.Log(id + "Restored");
//         }
//         public void ReturnToTitle(){
//             AsyncReturnToTitle().Forget();
//         }
//         async UniTask AsyncReturnToTitle(){
//             await ConnectHub.Instance.CloseSession();
            
//             SceneChanger.ChangeGameScene(SCENELIST.MainMenu);
//         }
//     }
// }