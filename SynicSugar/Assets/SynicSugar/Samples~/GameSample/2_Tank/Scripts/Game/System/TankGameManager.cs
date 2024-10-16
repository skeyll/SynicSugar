using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SynicSugar.Samples.Tank {
    [NetworkCommons(true)]
    public partial class TankGameManager : MonoBehaviour{
        [SerializeField] Transform tankParent;
        TankCameraControl cameraControl;
        TankConnectionNotify connectionNotify;
        [SerializeField] TankGameResult gameResult;
        [SerializeField] List<Transform> spawners;
        [SerializeField] Button ReadyGame, PlayAgainGame, QuitGame;
        [Synic(0), HideInInspector] public GameState CurrentGameState;
        
        bool everyoneIsReady;
        //Time
        [SerializeField] Text timerText, systemText;
        TankPadGUI padGUI;

        void Start(){
            cameraControl = GetComponent<TankCameraControl>();
            SwitchSystemUIsState(GameState.PreparationForObjects);
            CurrentGameState = GameState.PreparationForObjects;
            InvokeStateProcess(GameState.PreparationForObjects);
        }
        /// <summary>
        /// When the game restart from the pause, reset the time .
        /// </summary>
        /// <param name="pauseStatus"></param>
        void OnApplicationPause(bool pauseStatus){
            if(CurrentGameState != GameState.InGame){
                return;
            }
            
            if (!pauseStatus){
                ConnectHub.Instance.GetInstance<TankRoundTimer>().SetTimer();
            }
        }
        /// <summary>
        /// Manage game processes by State machine in this sample.
        /// </summary>
        /// <param name="newState"></param>
        internal void InvokeStateProcess(GameState newState){
            Debug.Log("TankGameManager: CurrentState is" + newState);
            switch(newState){
                case GameState.PreparationForObjects:
                    PreparationForObjectsProcess();
                break;
                case GameState.PreparationForData:
                    PreparationForDataProcess().Forget();
                break;
                case GameState.Standby:
                    StandbyProcess();
                break;
                case GameState.InGame:
                    InGameProcess().Forget();
                break;
                case GameState.Result:
                    ResultProcess().Forget();
                break;
            }
        }
        /// <summary>
        /// Manage ui active.
        /// </summary>
        /// <param name="newState"></param>
        void SwitchSystemUIsState(GameState newState){
            ReadyGame.gameObject.SetActive(newState == GameState.Standby);
            PlayAgainGame.gameObject.SetActive(newState == GameState.Result);
            QuitGame.gameObject.SetActive(newState == GameState.Result);
            timerText.gameObject.SetActive(newState == GameState.InGame);
        }
    #region PreparationForObjects
        /// <summary>
        /// Generate User Instance to sync data, then StartPacketReceiver and Send Packet.
        /// </summary>
        void PreparationForObjectsProcess(){
            GeneratePlayers();
            RegisterConnectionNotifies();
            cameraControl.SetFollowLocalTarget();
            //Generate objects for pool with member count
            gameResult.GenerateResultsText(p2pInfo.Instance.AllUserIds.Count);
            TankShellManager.Instance.GenerateShellPool(p2pInfo.Instance.AllUserIds.Count);
            //Generate pad 
            GeneratePadGUIs();
            RegisterEventsToGUIs();
            //NetworkCommons
            ConnectHub.Instance.RegisterInstance(this);
            ConnectHub.Instance.RegisterInstance(new TankRoundTimer(120, timerText));
            
            CurrentGameState = GameState.PreparationForData;
            InvokeStateProcess(CurrentGameState);
        }
        void GeneratePlayers(){
            GameObject playerPrefab = (GameObject)Resources.Load("Tank/Tank");
            // SynicSugar sync data via each instance.
            // So, we need to instanctiate the object before send or receive packets.
            SynicObject.AllSpawn(playerPrefab, tankParent);
        }
        void RegisterConnectionNotifies(){
            // Register an event if we need to know connection information.
            connectionNotify = new TankConnectionNotify();
            connectionNotify.RegisterConnectionNotifyEvents();
        }
        void GeneratePadGUIs(){
            //Generate
            GameObject padGUIPrefab = (GameObject)Resources.Load("Tank/PadGUIs");
            GameObject padObjects = Instantiate(padGUIPrefab);

            //Register Events to buttons to call Rpc from gui button.
            padGUI = padObjects.GetComponent<TankPadGUI>();
        }
        void RegisterEventsToGUIs(){
            //To pad buttons
            TankPlayer player = ConnectHub.Instance.GetUserInstance<TankPlayer>(p2pInfo.Instance.LocalUserId);
            padGUI.RegisterButtonEvents(d => player.Move(d), () => player.Stop(), () => player.StartCharge(), () => player.ReleaseTheTrigger());
            //To system buttons
            TankEventRegisterExtenstions.RegisterEvents(ReadyGame, EventTriggerType.PointerClick, () => ReadyToPlayBattle());
            TankEventRegisterExtenstions.RegisterEvents(PlayAgainGame, EventTriggerType.PointerClick, () => TryAgain());
            TankEventRegisterExtenstions.RegisterEvents(QuitGame, EventTriggerType.PointerClick, () => ReturnToTitle());
        }
    #endregion
    #region PreparationForData
        /// <summary>
        /// After creating an instance to send and receive data, start exchanging data.
        /// </summary>
        /// <returns></returns>
        async UniTask PreparationForDataProcess(){
            //If the user is a reconnector, get synic packet before re-start a game.
            if(p2pInfo.Instance.IsReconnecter){
                //If this user comes here by ReconnectAPI, them need receive all the SynicPackets before back to the game.
                ConnectHub.Instance.StartSynicReceiver(5);
                await UniTask.WaitUntil(() => p2pInfo.Instance.HasReceivedAllSyncSynic, cancellationToken: this.GetCancellationTokenOnDestroy());
                SetSynicsToObject();
            }
            //If the game had not yet started, the reconnector also (re)sends the data.
            if(CurrentGameState == GameState.PreparationForData){
                SetLocalPlayerBasicData();
            }

            //One packet receiving per frame is sufficient in this sample,　but just in case set max member amount to the batch size.
            ConnectHub.Instance.StartPacketReceiver(PacketReceiveTiming.FixedUpdate, (byte)p2pInfo.Instance.AllUserIds.Count);
            //If this local user is a reconnector, CurrentGameState has been overwritten via Synic to another State.
            CurrentGameState = CurrentGameState == GameState.PreparationForData ? GameState.Standby : CurrentGameState;
            //If not overwritten by Synic, move to the next State; if overwritten, move to the host’s State
            InvokeStateProcess(CurrentGameState);
        }
        /// <summary>
        /// Set basis data via sync.
        /// </summary>
        void SetLocalPlayerBasicData(){
            TankPlayerStatus status = new TankPlayerStatus()
            {
                Name = TankPassedData.PlayerName, //carring over from Matchmaking scene.
                MaxHP = 100,
                Speed = 12f,
                Attack = 20,
                RespawnPosition = spawners[p2pInfo.Instance.GetUserIndex()].transform.position, 
                RespawnQuaternion = spawners[p2pInfo.Instance.GetUserIndex()].transform.rotation
            };
            //Call RPC process to sync status.
            ConnectHub.Instance.GetUserInstance<TankPlayer>(p2pInfo.Instance.LocalUserId).SetPlayerStatus(status);
        }
        /// <summary>
        /// Reflect synics to player object.
        /// </summary>
        void SetSynicsToObject(){
            foreach(var id in p2pInfo.Instance.CurrentConnectedUserIds){
                ConnectHub.Instance.GetUserInstance<TankPlayer>(id).SetDataAfterReconnect();
            }
        }
    #endregion
    #region Standby
        /// <summary>
        /// Wait for everyone ready.
        /// </summary>
        void StandbyProcess(){
            padGUI.SwitchGUISState(PadState.OnlyMove);
            SwitchSystemUIsState(GameState.Standby);
            
            ConnectHub.Instance.GetInstance<TankRoundTimer>().ResetTimestamp();

            WaitForTheUsersReady().Forget();
        }
        /// <summary>
        /// From call system button
        /// </summary>
        public void ReadyToPlayBattle(){
            //To deactivate all GUI.
            SwitchSystemUIsState(GameState.PreparationForObjects);
            ConnectHub.Instance.GetUserInstance<TankPlayer>(p2pInfo.Instance.LocalUserId).Ready();
        }
        /// <summary>
        /// Monitor isReady flag by everyoneIsReady.
        /// </summary>
        async UniTask WaitForTheUsersReady(){
            //Wait for Ready
            await UniTask.WaitUntil(() => everyoneIsReady, cancellationToken: this.GetCancellationTokenOnDestroy());
            //Host decide the start time of this round.
            if(isHost){
                uint roomForLag = 3;
                uint startTimestamp = p2pInfo.Instance.GetSessionTimestamp() + roomForLag;
                ConnectHub.Instance.GetInstance<TankRoundTimer>().SetTimestamp(startTimestamp);
            }
            //reset flag while waiting for getting the start timestamp.
            foreach(var id in p2pInfo.Instance.CurrentAllUserIds){
                ConnectHub.Instance.GetUserInstance<TankPlayer>(id).status.isReady = false;
            }
            everyoneIsReady = false;
            //Wait for Timestamp
            await UniTask.WaitUntil(() => ConnectHub.Instance.GetInstance<TankRoundTimer>().startTimestamp != 0, cancellationToken: this.GetCancellationTokenOnDestroy());

            CurrentGameState = GameState.InGame;
            InvokeStateProcess(CurrentGameState);
            
        }
        /// <summary>
        /// Go to InGame state after all users are ready. <br />
        /// Called from each ready prcess and disconnected proess.
        /// </summary>
        internal void CheckReadyState(){
            //Compare state with everyone, then only go to InGame if these are the same.
            foreach(var id in p2pInfo.Instance.CurrentConnectedUserIds){
                if(!ConnectHub.Instance.GetUserInstance<TankPlayer>(id).status.isReady){
                    return;
                }
            }
            everyoneIsReady = true;
        }
    #endregion
    #region InGame
        async UniTask InGameProcess(){
            SwitchSystemUIsState(GameState.InGame);
            uint waitingTime = ConnectHub.Instance.GetInstance<TankRoundTimer>().SetTimer();
            //p2pInfo.Instance.IsReconnecter　is valid until a SynicPacket is received once.
            //So, we have to use others for reconnecter flag.
            if(waitingTime > 0){
                await GameStarting(waitingTime);
            }

            padGUI.SwitchGUISState(PadState.ALL);

            await ConnectHub.Instance.GetInstance<TankRoundTimer>().StartTimer();
            await GameEnding();
            CurrentGameState = GameState.Result;
            InvokeStateProcess(CurrentGameState);
        }
        /// <summary>
        /// Change the camera target and if there is the one user in the field, stop the timer and finish the game.
        /// </summary>
        /// <param name="deadUserIndex">To switch camera target</param>
        internal void CheckRoundState(int deadUserIndex){
            // When local user is dead, deactivate pads.
            if(deadUserIndex == p2pInfo.Instance.GetUserIndex()){
                padGUI.SwitchGUISState(PadState.None);
            }
            // Swtich camera position
            cameraControl.SwitchTargetToNextSurvivor(deadUserIndex);
            // Need end this round?
            if(isHost && IsLastSurvivor()){
                ConnectHub.Instance.GetInstance<TankRoundTimer>().StopTimer();
            }
        }
        bool IsLastSurvivor(){
            int survivorCount = 0;
            foreach(var id in p2pInfo.Instance.CurrentAllUserIds){
                TankPlayer player = ConnectHub.Instance.GetUserInstance<TankPlayer>(id);
                // A player who has disconnected is also considered dead.
                if(player.gameObject.activeSelf && player.status.CurrentHP > 0f){
                    survivorCount++;
                }
                // If not alone, stop counting.
                if(survivorCount > 1){
                    break;
                }
            }
            return survivorCount == 1;
        }
        async UniTask GameStarting(uint waitingTime){
            int countTime = (int)waitingTime;
            //More than 3 do not count.
            if(countTime > 3){
                countTime -= 3;
                int timeUntilCountdown = countTime * 1000;
                await UniTask.Delay(timeUntilCountdown);
            }
            padGUI.SwitchGUISState(PadState.None);
            //Count 3　- 1
            for(int i = countTime; i > 0; i--){
                systemText.text = i.ToString();
                await UniTask.Delay(1000);
            }
            //Then, start
            systemText.text = "Start";
            ResetSystemText().Forget();
        }
        async UniTask GameEnding(){
            padGUI.SwitchGUISState(PadState.None);
            systemText.text = "Finish";
            await ResetSystemText();
        }
        async UniTask ResetSystemText(){
            await UniTask.Delay(2000);
            systemText.text = string.Empty;
        }
    #endregion
    #region Result
        async UniTask ResultProcess(){
            //Deactivate pre clown.
            foreach(var id in p2pInfo.Instance.CurrentAllUserIds){
                ConnectHub.Instance.GetUserInstance<TankPlayer>(id).ResetObjectState();
            }

            List<TankResultData> results = GetRoundResult();
            //Switch Camera
            TankPlayer winner = ConnectHub.Instance.GetUserInstance<TankPlayer>(results[0].UserId);
            winner.ActivateClown();
            cameraControl.SetFollowTarget(winner.transform, p2pInfo.Instance.GetUserIndex(results[0].UserId));
            await UniTask.Delay(3000);

            //On GUI
            gameResult.DisplayResult(results);
            await UniTask.Delay(2000);

            //Activate all player.
            foreach(var id in p2pInfo.Instance.CurrentAllUserIds){
                ConnectHub.Instance.GetUserInstance<TankPlayer>(id).ActivatePlayer();
            }
            //Switch Camera and GUI state
            cameraControl.SetFollowLocalTarget();
            SwitchSystemUIsState(GameState.Result);
            padGUI.SwitchGUISState(PadState.OnlyMove);
        }
        List<TankResultData> GetRoundResult(){
            List<TankResultData> result = new List<TankResultData>();

            foreach(var id in p2pInfo.Instance.CurrentAllUserIds){
                TankPlayer player = ConnectHub.Instance.GetUserInstance<TankPlayer>(id);
                // Disconnected playerｓ are treated as having 0 HP.
                result.Add(new TankResultData(id, player.status.Name, player.gameObject.activeSelf ? player.status.CurrentHP : 0f));
            }

            return result.OrderByDescending(r => r.RemainHP).ToList();
        }
        /// <summary>
        /// Quit game and back to title scene.
        /// </summary>
        public void TryAgain(){
            gameResult.DeactivateResult();
            SwitchSystemUIsState(GameState.Standby);
            CurrentGameState = GameState.Standby;
            InvokeStateProcess(CurrentGameState);
        }

        /// <summary>
        /// Quit game and back to title scene.
        /// </summary>
        public void ReturnToTitle(){
            SwitchSystemUIsState(GameState.PreparationForObjects);
            AsyncReturnToTitle().Forget();
        }
        async UniTask AsyncReturnToTitle(){
            Result closeResult;
            if(p2pInfo.Instance.AllUserIds.Count == 1){ //or MatchMakeManager.Instance.GetCurrentLobbyID() == "OFFLINEMODE"
                closeResult = await ConnectHub.Instance.DestoryOfflineLobby();
            }else if(p2pInfo.Instance.CurrentConnectedUserIds.Count == 1){ //If the room is alone, close the room.
                closeResult = await ConnectHub.Instance.CloseSession();
            }else{
                closeResult = await ConnectHub.Instance.ExitSession();
            }

            if(closeResult != Result.Success){
                Debug.Log("Failure to close the room");
                ReadyGame.gameObject.SetActive(true);
                QuitGame.gameObject.SetActive(true);
                return;
            }
            
            SceneChanger.ChangeGameScene(SCENELIST.MainMenu);
        }
        #endregion
    }
}