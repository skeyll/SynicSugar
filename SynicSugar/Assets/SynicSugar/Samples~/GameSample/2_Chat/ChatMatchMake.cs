using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using SynicSugar.MatchMake;
/// <summary>
/// Awake: Prep for matchmaking (For basis of Events. This sample dosen't have condition for matchmaking)
/// Start: Try to reconnect
/// UserInputs: Matchmaking or OfflineMode
/// </summary>
namespace SynicSugar.Samples.Chat {
    public class ChatMatchMake : MonoBehaviour {
        enum MATCHMAKEING_STATE { 
            NoneAndAfterStart, Standby, InMatchmaking, ReadyToStartGame
        }

        [SerializeField] GameObject matchmakePrefab;
        /// <summary>
        /// Display matchmaking state on GUI.
        /// </summary>
        [SerializeField] Text matchmakeState;
        [SerializeField] Button startMatchMake, startOfflineMode, 
                        cancelMatchMake, backtoMenu, startGame;
        [SerializeField] Text buttonText;
        [SerializeField] MatchMakeConditions matchConditions;
    #region Prep for matchmaking
        //At first, prep GUI events for matchmaking.
        void Awake(){
            if(MatchMakeManager.Instance == null){
                Instantiate(matchmakePrefab);
            }
            //Prep matchmaking
            SetGUIEvents();
        }
        /// <summary>
        /// Register tests and button events for in-matchmaking.
        /// </summary>
        void SetGUIEvents(){
            MatchMakeManager.Instance.MatchMakingGUIEvents = MatchMakeConfig.SetMatchingText(MatchMakeConfig.Langugage.EN);
            MatchMakeManager.Instance.MatchMakingGUIEvents.stateText = matchmakeState;

            MatchMakeManager.Instance.MatchMakingGUIEvents.DisableStart += OnDisableStart;
            MatchMakeManager.Instance.MatchMakingGUIEvents.EnableCancelKick += OnEnableCancel;
        }
        /// <summary>
        /// Just after starting matchmaking
        /// </summary>
        void OnDisableStart(){
            //Cancel action need be disable when creating or joining lobby.
            SwitchButtonsActive(MATCHMAKEING_STATE.NoneAndAfterStart);
        }
        /// <summary>
        /// Finish to create or join a lobby, and accept canceling process.
        /// </summary>
        void OnEnableCancel(){
            SwitchButtonsActive(MATCHMAKEING_STATE.InMatchmaking);
        }
    #endregion
    #region Reconnection
        //Second,　check whether this player is a reconnector.
        //In fact, you had better check id like this on the Title screen after user Login to EOS.
        void Start(){
            SwitchButtonsActive(MATCHMAKEING_STATE.NoneAndAfterStart);
            //Try recconect
            //Sample projects use LobbyID save API of SynicSugar to save into Playerprefs for recconection.
            string LobbyID = MatchMakeManager.Instance.GetReconnectLobbyID();
            TryToreconnect(LobbyID).Forget();
        }
        async UniTask TryToreconnect(string LobbyID){
            //On the default way, return Empty when there is no lobby data in local.
            if(string.IsNullOrEmpty(LobbyID)){
                EOSDebug.Instance.Log($"This user is not Reconnecter.");
                SwitchButtonsActive(MATCHMAKEING_STATE.Standby);
                return;
            }

            Result result = await MatchMakeManager.Instance.ReconnectLobby(LobbyID);

            if(result != Result.Success){
                EOSDebug.Instance.Log("Fail to Re-Connect Lobby.", result);
                SwitchButtonsActive(MATCHMAKEING_STATE.Standby);
                return;
            }
            EOSDebug.Instance.Log($"Success Recconect! LobbyID:{MatchMakeManager.Instance.GetCurrentLobbyID()}");
            SwitchButtonsActive(MATCHMAKEING_STATE.ReadyToStartGame);
        }
    #endregion
        /// <summary>
        /// Switch button active
        /// </summary>
        /// <param name="state"></param>
        void SwitchButtonsActive(MATCHMAKEING_STATE state){
            //To start matchmake
            startMatchMake.gameObject.SetActive(state == MATCHMAKEING_STATE.Standby);
            startOfflineMode.gameObject.SetActive(state == MATCHMAKEING_STATE.Standby);
            //To cancel matchmake
            cancelMatchMake.gameObject.SetActive(state == MATCHMAKEING_STATE.InMatchmaking);
            backtoMenu.gameObject.SetActive(state == MATCHMAKEING_STATE.InMatchmaking);
            //To start game
            startGame.gameObject.SetActive(state == MATCHMAKEING_STATE.ReadyToStartGame);
        }
    #region Matchmaking and Offlinemode
        /// <summary>
        /// Basis way to start matchmaking.
        /// </summary>
        public void StartMatchMake(){
            EOSDebug.Instance.Log("Start MatchMake.");
            StartMatchMakeEntity().Forget();
        }
        /// <summary>
        /// SynicSugar has some async process, so implement only async part separately.
        /// </summary>
        /// <returns></returns>
        internal async UniTask StartMatchMakeEntity(){
            SwitchButtonsActive(MATCHMAKEING_STATE.NoneAndAfterStart);
            Result result = await MatchMakeManager.Instance.SearchAndCreateLobby(matchConditions.GetLobbyCondition(2));
                
            if(result != Result.Success){
                EOSDebug.Instance.Log("MatchMaking Failed.", result);
                SwitchButtonsActive(MATCHMAKEING_STATE.Standby);
                return;
            }

            EOSDebug.Instance.Log($"Success Matching! LobbyID:{MatchMakeManager.Instance.GetCurrentLobbyID()}");
            SwitchButtonsActive(MATCHMAKEING_STATE.ReadyToStartGame);
        }
        /// <summary>
        /// You can register NON-UniTask async process to Unity button.
        /// I implement everything the same way as StartMatchMake in my-own project for the performance, though.
        /// </summary>
        public async void StartOfflineMode(){
            EOSDebug.Instance.Log("Start Offline Mode.");
            //To　simulate matchmaking
            // OfflineMatchmakingDelay delay = new OfflineMatchmakingDelay(2000, 1000, 1000, 1000);
            OfflineMatchmakingDelay delay = OfflineMatchmakingDelay.NoDelay;
            //This is always true.
            Result result = await MatchMakeManager.Instance.CreateOfflineLobby(matchConditions.GetLobbyCondition(2), delay);

            if(result != Result.Success){
                //CreateOfflineLobby always returns Success. So this is no point.
                return;
            }

            EOSDebug.Instance.Log($"Success Matching! LobbyID:{MatchMakeManager.Instance.GetCurrentLobbyID()}");
            SwitchButtonsActive(MATCHMAKEING_STATE.ReadyToStartGame);
        }
        /// <summary>
        /// For button. Just cancel matchmaking.
        /// </summary>
        public void CancelMatchMaking(){
            CancelMatchMaking(isOfflineMode()).Forget();
        }
        public async UniTask CancelMatchMaking(bool isOfflineMode){
            if(isOfflineMode){
                await MatchMakeManager.Instance.DestoryOfflineLobby(false);
            }else{
                await MatchMakeManager.Instance.ExitCurrentMatchMake(false);
            }
        }
        /// <summary>
        /// For button. Cancel matchmaking, and then return to Menu.
        /// </summary>
        public void CanelMatchMakingAndReturnToMenu(){
            CanelMatchMakingAndReturnToMenu(isOfflineMode()).Forget();
        }
        public async UniTask CanelMatchMakingAndReturnToMenu(bool isOfflineMode){
            if(isOfflineMode){
                await MatchMakeManager.Instance.DestoryOfflineLobby();
            }else{
                await MatchMakeManager.Instance.ExitCurrentMatchMake(true);
            }
            SceneChanger.ChangeGameScene(SCENELIST.MainMenu);
        }
        /// <summary>
        /// Offline lobby id is "OFFLINEMODE"
        /// </summary>
        /// <returns></returns>
        bool isOfflineMode(){
            return MatchMakeManager.Instance.GetCurrentLobbyID() == "OFFLINEMODE";
        }
    #endregion
    }
}